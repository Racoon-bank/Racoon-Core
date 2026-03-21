using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace api.Broker
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqPublisher()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "admin",
                Password = "admin123"
            };

            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task PublishAsync<T>(T message, string queueName)
        {
            await _channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                body: body);
        }
    }
}