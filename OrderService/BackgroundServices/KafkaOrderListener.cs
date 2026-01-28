using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using OrderService.Data;
using OrderService.OrderHandling;
using Shared.Contracts;
using Shared.Contracts.Events;
using System.Text.Json;

namespace OrderService.BackgroundServices
{
    public class KafkaOrderListener : BackgroundService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IConsumer<string, AvroEventEnvelope> _consumer;
        private readonly Dictionary<EventType, IOrderEventHandler> _handlers;
        private const string TopicName = KafkaConstants.OrdersTopic;
        private readonly ILogger<KafkaOrderListener> _logger;

        private readonly ISchemaRegistryClient _schemaRegistryClient;

        public KafkaOrderListener(
          IConfiguration configuration,
          IOrderRepository orderRepository,
          IEnumerable<IOrderEventHandler> handlers,
          ILogger<KafkaOrderListener> i_Logger)
        {
            _logger = i_Logger;
            _orderRepository = orderRepository;
            _handlers = handlers.ToDictionary(h => h.EventType, h => h);

            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = configuration["Kafka:SchemaRegistryUrl"] ?? "http://schema-registry:8081"
            };
            _schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
                GroupId = "order-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, AvroEventEnvelope>(consumerConfig)
                    .SetValueDeserializer(new AvroDeserializer<AvroEventEnvelope>(_schemaRegistryClient).AsSyncOverAsync())
                    .Build();
            
            _consumer.Subscribe(TopicName);
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        ConsumeResult<string, AvroEventEnvelope> message = null;
                        try
                        {
                            message = _consumer.Consume(stoppingToken);

                            if (message != null)
                            {
                                ProcessOrderEvent(message);
                            }
                        }
                        catch (ConsumeException ce)
                        {
                            _logger.LogWarning("Kafka consume warning. Reason: {Reason}", ce.Error.Reason);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                        ex,
                        "Unhandled error while consuming Kafka message. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}, Key: {Key}",
                        message?.Topic,
                        message?.Partition,
                        message?.Offset,
                        message?.Message?.Key);
                        }
                    }
                }
                finally
                {
                    _consumer.Close();
                }

            }, stoppingToken);
        }

        private void ProcessOrderEvent(ConsumeResult<string, AvroEventEnvelope> message)
        {
            try
            {
                var avroValue = message.Message.Value;

                if (avroValue == null)
                {
                    _logger.LogError("Received null Avro value at offset {Offset}", message.Offset);
                    return;
                }

                var envelope = new EventEnvelope
                {
                    EventType = Enum.Parse<EventType>(avroValue.EventType),
                    OrderId = avroValue.OrderId,
                    Payload = JsonDocument.Parse(avroValue.Payload).RootElement
                };

                _orderRepository.AddOrderToTopic(message.Topic, envelope.OrderId);

                if (_handlers.TryGetValue(envelope.EventType, out IOrderEventHandler? handler))
                {
                    handler.Handle(envelope.Payload);

                    _logger.LogInformation($"Processed event {envelope.EventType} for Order ID: {envelope.OrderId}");
                }
                else
                {
                    _logger.LogWarning("No handler registered for event type {EventType}", envelope.EventType);
                }
                
                _consumer.Commit(message);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize Kafka message. Payload: {Payload}", message.Value);
                _consumer.Commit(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message. OrderId: {OrderId}", message?.Message?.Key);
                _consumer.Commit(message);
            }
        }
    }
}
