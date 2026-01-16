using Shared.Contracts.Events;
using Confluent.Kafka;
using System.Text.Json;
using Shared.Contracts;

namespace CartService.Producer
{
    public class KafkaPublisher : IEventProducer, IDisposable
    {
        private const int MaxRetries = 3;
        private static readonly TimeSpan BaseDelay = TimeSpan.FromMilliseconds(200);

        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaPublisher> _logger;

        private const string TopicName = KafkaConstants.OrdersTopic;
        public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
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
            _logger = logger;
        }


        public async Task PublishAsync(EventEnvelope eventEnvelope)
        {
            string message;
            try
            {
                message = JsonSerializer.Serialize(eventEnvelope);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serializing event envelope. OrderId: {OrderId}", eventEnvelope.OrderId);
                throw;
            }

            int attempts = 0;

            while (true)
            {
                try
                {
                    var deliveryResult = await _producer.ProduceAsync(
                        topic: TopicName,
                        new Message<string, string>
                        {
                            Key = eventEnvelope.OrderId,
                            Value = message
                        });

                    _logger.LogDebug(
                        "Kafka message delivered. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Key: {Key}, attempts: {Attempt}",
                        deliveryResult.Topic,
                        deliveryResult.Partition,
                        deliveryResult.Offset,
                        eventEnvelope.OrderId,
                        ++attempts);

                    return;
                }
                catch (ProduceException<string, string> ex) when (attempts < MaxRetries)
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
