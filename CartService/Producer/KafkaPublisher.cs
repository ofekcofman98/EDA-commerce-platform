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
        private const int MaxRetries = 3;
        private static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(200);

        private readonly IProducer<string, AvroEventEnvelope> _producer;
        private readonly ILogger<KafkaPublisher> _logger;

        private const string TopicName = KafkaConstants.OrdersTopic;
        private readonly ISchemaRegistryClient _schemaRegistryClient;

        public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
        {
            _logger = logger;
            
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = configuration["Kafka:SchemaRegistryUrl"] ?? "http://schema-registry:8081"
            };
            _schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
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
                        topic: TopicName,
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
                catch (ProduceException<string, AvroEventEnvelope> ex) when (attempts < MaxRetries)
                {
                    attempts++;

                    var delay = TimeSpan.FromMilliseconds(BaseDelay.TotalMilliseconds * Math.Pow(2, attempts));

                    _logger.LogWarning(ex, "Kafka publish failed. Retrying {Attempt}/{MaxRetries} after {Delay}ms. OrderId: {OrderId}",
                        attempts,
                        MaxRetries,
                        delay.TotalMilliseconds,
                        eventEnvelope.OrderId);

                    await Task.Delay(delay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Kafka publish failed after {Retries} retries. OrderId: {OrderId}",
                        MaxRetries,
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
