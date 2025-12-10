using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using Shared.Contracts;

namespace CartService.Producer
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

        private const string k_ExchangeName = RabbitMQConstants.Exchange.Orders;
        private const string k_QueueName = RabbitMQConstants.Queue.Orders;
        private const string k_DeadLetterExchange = RabbitMQConstants.Exchange.DeadLetter;
        private const string k_RoutingKey = RabbitMQConstants.RoutingKey.NewOrder;

        public RabbitMQPublisher(ConnectionFactory i_Factory)
        {
            _connection = i_Factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: k_ExchangeName, type: ExchangeType.Topic, durable: true, autoDelete: false);

            DeclareQueue();

            _channel.QueueBind(queue: k_QueueName, exchange: k_ExchangeName, routingKey: k_RoutingKey);

            _channel.ExchangeDeclare(exchange: k_DeadLetterExchange, type: ExchangeType.Fanout, durable: true, autoDelete: false);
         
            _channel.QueueDeclare(queue: $"{k_QueueName}.dead", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: $"{k_QueueName}.dead", exchange: k_DeadLetterExchange, routingKey: "");
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

        public void PublishOrder(Order order)
        {
            string message = JsonSerializer.Serialize(order);
            byte[] body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: k_ExchangeName,
                routingKey: k_RoutingKey,
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
