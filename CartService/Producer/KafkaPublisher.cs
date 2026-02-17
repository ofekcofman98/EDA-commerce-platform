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
        private readonly ISchemaRegistryClient _schemaRegistryClient;

        public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
        {
            _logger = logger;
            
            // Read settings from KafkaSettings section
            _topicName = configuration["KafkaSettings:TopicName"] ?? "orders.topic";
            
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = configuration["KafkaSettings:SchemaRegistryUrl"] ?? "http://schema-registry:8081"
            };
            _schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["KafkaSettings:BootstrapServers"] ?? "kafka:9092",
                Acks = Acks.Leader,
                MessageTimeoutMs = 5000,
                RetryBackoffMs = 100
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
                    "AVRO MESSAGE DELIVERED. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Key: {Key}",
                    deliveryResult.Topic,
                    deliveryResult.Partition,
                    deliveryResult.Offset,
                    eventEnvelope.OrderId);
            }
            catch (ProduceException<string, AvroEventEnvelope> ex)
            {
                _logger.LogError(
                    ex,
                    "Kafka publish failed. OrderId: {OrderId}",
                    eventEnvelope.OrderId);

                throw;
            }
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }

    }
}
