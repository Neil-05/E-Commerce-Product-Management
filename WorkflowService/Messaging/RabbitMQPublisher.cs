using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WorkflowService.Messaging
{
    public class RabbitMQPublisher
    {
        public async Task Publish(object message)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "product_published",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: "product_published",
                body: body
            );
        }
    }
}