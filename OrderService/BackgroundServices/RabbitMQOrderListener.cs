using OrderService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;
using Shared.Contracts;
using RabbitMQ.Client.Exceptions;
using OrderService.ShippingCost;
using Shared.Contracts.Events;
using Shared.Contracts.Orders;
using OrderService.OrderHandling;

namespace OrderService.BackgroundServices
{
    public class RabbitMQOrderListener : BackgroundService
    {
        /// <summary>
        /// 
        /// The class listens in async from RabbitMQ.
        /// The class gets a new order which its status is new, the order is serialized.
        /// The class deserializes the order, calculates the shipping cost and builds a OrderDetails instance
        /// The class returns a message of success or failure, and add the new order to the repository.
        /// 
        /// </summary>

        private IConnection? _connection;
        private IModel? _channel;
        private readonly ConnectionFactory _factory;

        private readonly Dictionary<EventType, IOrderEventHandler> _handlers;

        private const int k_MaxRetries = 15;
        private const int k_DelayMs = 5000;

        private const string k_ExchangeName = RabbitMQConstants.Exchange.Orders;
        private const string k_QueueName = RabbitMQConstants.Queue.Orders;
        private const string k_DeadLetterExchange = RabbitMQConstants.Exchange.DeadLetter;
        //private const string k_RoutingKey = RabbitMQConstants.RoutingKey.AllOrders;
        private const string k_BindingKey = RabbitMQConstants.BindingKey.AllOrders;

        public RabbitMQOrderListener(ConnectionFactory i_Factory, IEnumerable<IOrderEventHandler> handlers)
        {
            _factory = i_Factory;
            _handlers = handlers.ToDictionary(h => h.EventType, h => h);

            InitializeRabbitMQConnection();
        }

        private void InitializeRabbitMQConnection()
        {
            for (int i = 0; i < k_MaxRetries; i++)
            {
                try
                {
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    DeclareQueue();
                    _channel.QueueBind(queue: k_QueueName, exchange: k_ExchangeName, routingKey: RabbitMQConstants.BindingKey.AllOrders);


                    Console.WriteLine("Connection to RabbitMQ established successfully.");
                    return;
                }
                catch (BrokerUnreachableException ex)
                {
                    Console.WriteLine($"RabbitMQ connection failed on attempt {i + 1}/{k_MaxRetries}. Retrying in {k_DelayMs / 1000}s...");
                    Thread.Sleep(k_DelayMs);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred during connection: {ex.Message}");
                    throw;
                }
            }
            throw new Exception("Failed to connect to RabbitMQ after maximum retries.");
        }

        public void DeclareQueue()
        {
            _channel.QueueDeclare(
                queue: k_QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                            { "x-dead-letter-exchange", k_DeadLetterExchange }
                });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (model, eventArg) =>
            {
                var body = eventArg.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    ProcessOrderEvent(message);
                    _channel.BasicAck(deliveryTag: eventArg.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CONSUMER FATAL ERROR] Processing failed: {ex.Message}");
                    Console.WriteLine($"[CONSUMER FATAL ERROR] Stack Trace: {ex.StackTrace}");

                    _channel.BasicNack(deliveryTag: eventArg.DeliveryTag, multiple: false, requeue: false);
                }

                await Task.Yield();
            };

            _channel.BasicConsume(queue: k_QueueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private void ProcessOrderEvent(string i_MessageJson)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            EventEnvelope? envelope = JsonSerializer.Deserialize<EventEnvelope>(i_MessageJson, options);

            if (envelope == null)
            {
                Console.WriteLine("Envelope is null");
                return;
            }

            if (!_handlers.TryGetValue(envelope.EventType, out var handler))
            {
                Console.WriteLine($"No handler found for event type: {envelope.EventType}");
                return;
            }

            handler.Handle(envelope.Payload);
        }

        public override void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch { }
            base.Dispose();
        }
    }
}
