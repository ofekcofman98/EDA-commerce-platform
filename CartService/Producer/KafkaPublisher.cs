using Shared.Contracts.Events;
using Confluent.Kafka;
using System.Text.Json;
using Shared.Contracts;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace CartService.Producer
{
    public class KafkaPublisher : IEventProducer, IDisposable
    {
        private readonly IProducer<string, AvroEventEnvelope> _producer;
        private readonly ILogger<KafkaPublisher> _logger;
        private readonly string _topicName;
        private readonly int _maxRetries;
        private readonly TimeSpan _baseDelay;
        private readonly ISchemaRegistryClient _schemaRegistryClient;

        public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
        {
            _logger = logger;
            
            // Read settings from KafkaSettings section
            _topicName = configuration["KafkaSettings:TopicName"] ?? "orders.topic";
            
            // Read retry configuration with validation
            var maxRetriesStr = configuration["KafkaSettings:MaxRetries"];
            if (string.IsNullOrEmpty(maxRetriesStr) || !int.TryParse(maxRetriesStr, out _maxRetries))
            {
                _logger.LogWarning("KafkaSettings:MaxRetries not configured or invalid. Using default: 3");
                _maxRetries = 3;
            }
            
            var baseDelayMsStr = configuration["KafkaSettings:BaseDelayMs"];
            int baseDelayMs;
            if (string.IsNullOrEmpty(baseDelayMsStr) || !int.TryParse(baseDelayMsStr, out baseDelayMs))
            {
                _logger.LogWarning("KafkaSettings:BaseDelayMs not configured or invalid. Using default: 200ms");
                baseDelayMs = 200;
            }
            _baseDelay = TimeSpan.FromMilliseconds(baseDelayMs);
            
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = configuration["KafkaSettings:SchemaRegistryUrl"] ?? "http://schema-registry:8081"
            };
            _schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["KafkaSettings:BootstrapServers"] ?? "kafka:9092",
                Acks = Acks.All,
                EnableIdempotence = true,
                BatchSize = 32 * 1024,
                LingerMs = 10,
                RetryBackoffMs = 100,
                MessageTimeoutMs = 3000
            };

            _producer = new ProducerBuilder<string, AvroEventEnvelope>(config)
                            .SetValueSerializer(new AvroSerializer<AvroEventEnvelope>(_schemaRegistryClient))
                            .Build();
        }


        public async Task PublishAsync(EventEnvelope eventEnvelope)
        {
            var avroEnvelope = new AvroEventEnvelope
            {
                EventType = eventEnvelope.EventType.ToString(),
                OrderId = eventEnvelope.OrderId,
                Payload = eventEnvelope.Payload.GetRawText()
            };

            int attempts = 0;

            while (true)
            {
                try
                {
                    var deliveryResult = await _producer.ProduceAsync(
                        topic: _topicName,
                        new Message<string, AvroEventEnvelope>
                        {
                            Key = eventEnvelope.OrderId,
                            Value = avroEnvelope
                        });

                    _logger.LogInformation(
                        "AVRO MESSAGE DELIVERED. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Key: {Key}, attempts: {Attempt}",
                        deliveryResult.Topic,
                        deliveryResult.Partition,
                        deliveryResult.Offset,
                        eventEnvelope.OrderId,
                        ++attempts);

                    return;
                }
                catch (ProduceException<string, AvroEventEnvelope> ex) when (attempts < _maxRetries)
                {
                    attempts++;

                    var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempts));

                    _logger.LogWarning(ex, "Kafka publish failed. Retrying {Attempt}/{MaxRetries} after {Delay}ms. OrderId: {OrderId}",
                        attempts,
                        _maxRetries,
                        delay.TotalMilliseconds,
                        eventEnvelope.OrderId);

                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Kafka publish failed after {Retries} retries. OrderId: {OrderId}",
                        _maxRetries,
                        eventEnvelope.OrderId);

                    throw;
                }
            } 
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }

    }
}
