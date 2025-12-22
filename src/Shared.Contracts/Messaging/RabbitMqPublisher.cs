using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Shared.Contracts.Messaging;

public class RabbitMqPublisher : IRabbitPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> options)
    {
        var cfg = options.Value ?? new RabbitMqOptions();
        var factory = new ConnectionFactory
        {
            HostName = cfg.Host,
            VirtualHost = cfg.VirtualHost,
            UserName = cfg.User,
            Password = cfg.Password
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish(string queueName, object payload)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
        _channel.BasicPublish(exchange: string.Empty, routingKey: queueName, basicProperties: null, body: body);
    }

    public void Dispose()
    {
        try { _channel?.Close(); _connection?.Close(); } catch { }
    }
}
