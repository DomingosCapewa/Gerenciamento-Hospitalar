using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Clinica.Infrastructure.Messaging;

public class EventPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public EventPublisher(string host = "localhost", int port = 5672, string user = "guest", string pass = "guest")
    {
        var factory = new ConnectionFactory() { HostName = host, Port = port, UserName = user, Password = pass };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish(string queueName, object payload)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    }

    public void Dispose()
    {
        try { _channel?.Close(); _connection?.Close(); } catch { }
    }
}
