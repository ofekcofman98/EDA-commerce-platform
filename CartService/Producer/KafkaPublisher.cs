using Shared.Contracts.Events;
using Confluent.Kafka;
using System.Text.Json;

namespace CartService.Producer
{
    public class KafkaPublisher : IEventProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private const string TopicName = "orders";
        public KafkaPublisher(IConfiguration configuration)
        {
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

            _producer = new ProducerBuilder<string, string>(config).Build();
        }


        public async Task PublishAsync(EventEnvelope eventEnvelope)
        {
            string message = JsonSerializer.Serialize(eventEnvelope);

            var deliveryResult = await _producer.ProduceAsync(
                topic: TopicName,
                new Message<string, string>
                {
                    Key = eventEnvelope.OrderId,
                    Value = message
                });

            Console.WriteLine($"Delivered to {deliveryResult.TopicPartitionOffset}");
        }

        public void Dispose()
        {
            _producer.Flush(TimeSpan.FromSeconds(10));
            _producer.Dispose();
        }

    }
}
