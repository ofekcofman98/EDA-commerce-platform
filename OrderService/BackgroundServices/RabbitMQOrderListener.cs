using OrderService.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;
using Shared.Contracts;

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

        public RabbitMQOrderListener(IOrderRepository i_Repository, ConnectionFactory i_Factory)
        {
            _repository = i_Repository;

            _connection = i_Factory.CreateConnection();
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare(exchange: "orderExchange", type: ExchangeType.Fanout);

        }

        public void DeclareQueue()
        {
            string queueName = _channel.QueueDeclare().QueueName;

            _channel.QueueDeclare(queue: "orderQueue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, eventArg) =>
            {
                var body = eventArg.Body.ToArray(); // body of the message (Order in JSON), in byte array 
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    ProcessOrderEvent(message);
                    _channel.BasicAck(deliveryTag: eventArg.DeliveryTag, multiple: false); // Acknowledge that the message was recieved
                    Console.WriteLine($" [x] Received {message}");
                }
                catch (Exception ex)
                {
                    _channel.BasicNack(deliveryTag: eventArg.DeliveryTag, multiple: false, requeue: true); // Acknowledge that the message was NOT recieved
                }

                _channel.BasicConsume(queue: "order_processing_queue", autoAck: false, consumer: consumer);

            };

            return Task.CompletedTask;
        }

        private void ProcessOrderEvent(string i_MessageJson)
        {
            Order? order = JsonSerializer.Deserialize<Order>(i_MessageJson);

            if (order == null || order.Status != OrderStatus.New)
            {
                return;
            }

            decimal shippingCost = ShippingCostService.CalculateShippingCost(order);

            OrderDetails orderDetails = new OrderDetails(order, shippingCost);

            _repository.Add(orderDetails);

            //TODO: Log
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
