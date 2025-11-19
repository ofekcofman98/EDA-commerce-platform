using OrderService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;
using Shared.Contracts;
using OrderService.ShippingCost;
using RabbitMQ.Client.Exceptions;

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

        private readonly IOrderRepository _repository;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly ConnectionFactory _factory;

        private const int k_MaxRetries = 15;
        private const int k_DelayMs = 5000; 
                                            
        private const string k_ExchangeName = RabbitMQConstants.ExchangeName;
        private const string k_QueueName = RabbitMQConstants.QueueName;
        private const string k_DeadLetterExchange = RabbitMQConstants.DeadLetterExchangeName;

        public RabbitMQOrderListener(IOrderRepository i_Repository, ConnectionFactory i_Factory)
        {
            _repository = i_Repository;
            _factory = i_Factory;

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

                    _channel.ExchangeDeclare(exchange: k_ExchangeName, type: ExchangeType.Fanout);
                    _channel.ExchangeDeclare(exchange: k_DeadLetterExchange, type: ExchangeType.Direct);
                    DeclareQueue();
                    _channel.QueueBind(queue: k_QueueName, exchange: k_ExchangeName, routingKey: "");

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

            Order? order = JsonSerializer.Deserialize<Order>(i_MessageJson, options);

            if (order == null || order.Status != OrderStatus.New)
            {
                Console.WriteLine($"order == null :{order == null} ");
                Console.WriteLine($"order.Status != OrderStatus.New: {order.Status != OrderStatus.New}");
                return;
            }

            decimal shippingCost = ShippingCostService.CalculateShippingCost(order);

            OrderDetails orderDetails = new OrderDetails(order, shippingCost);

            _repository.Add(orderDetails);

            Console.WriteLine("orderDetails added to repository!");
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
