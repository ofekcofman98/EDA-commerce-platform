using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Shared.Contracts;

namespace CartService
{
    public class RabbitMQPublisher : IEventProducer, IDisposable
    {
        /// <summary>
        /// 
        /// The class publishes the order as serialied JSON into RabbitMQ
        /// 
        /// </summary>
        
        private IConnection? _connection;
        private IModel? _channel;

        private const string k_ExchangeName = "orders_exchange";
        private const string k_QueueName = "orders.queue";

        public RabbitMQPublisher(ConnectionFactory i_Factory)
        {
            _connection = i_Factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: k_ExchangeName, type: ExchangeType.Fanout);
            DeclareQueue();
            _channel.QueueBind(queue: k_QueueName, exchange: k_ExchangeName, routingKey: "");

        }

        public void DeclareQueue()
        {
            string queueName = _channel.QueueDeclare().QueueName;

            _channel.QueueDeclare(queue: k_QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
                );
        }

        public void PublishOrder(Order order)
        {
            string message = JsonSerializer.Serialize(order);
            byte[] body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: k_ExchangeName,
                routingKey: "",
                basicProperties: null,
                body: body);
        }

        public void Dispose()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
            _connection?.Close();
            _channel?.Close();
        }

    }
}
