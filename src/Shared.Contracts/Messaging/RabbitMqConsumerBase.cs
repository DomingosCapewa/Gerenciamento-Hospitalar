using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Shared.Contracts.Messaging
{
public abstract class RabbitMqConsumerBase<TEvent> : BackgroundService
{
    private readonly ILogger _logger;
    private readonly RabbitMqOptions _options;

    protected abstract string QueueName { get; }

    protected IModel? Channel;
    private IConnection? _connection;

    protected RabbitMqConsumerBase(
        ILogger logger,
        IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);

        if (Channel == null)
            return;

        var consumer = new AsyncEventingBasicConsumer(Channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var message = Deserialize(ea.Body.ToArray());
                await HandleMessageAsync(message, stoppingToken);

                Channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem da fila {Queue}", QueueName);
                Channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        Channel.BasicConsume(QueueName, false, consumer);
    }

    private async Task ConnectWithRetryAsync(CancellationToken token)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.User,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            DispatchConsumersAsync = true
        };

        while (!token.IsCancellationRequested)
        {
            try
            {
                _connection = factory.CreateConnection();
                Channel = _connection.CreateModel();

                Channel.QueueDeclare(
                    QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                Channel.BasicQos(0, 1, false);

                _logger.LogInformation("Conectado ao RabbitMQ - fila {Queue}", QueueName);
                break;
            }
            catch
            {
                _logger.LogWarning("RabbitMQ indisponível. Retry em 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
    }

    protected virtual TEvent Deserialize(byte[] body)
    {
        var json = Encoding.UTF8.GetString(body);
        return System.Text.Json.JsonSerializer.Deserialize<TEvent>(json)!;
    }

    protected abstract Task HandleMessageAsync(TEvent message, CancellationToken token);

    public override void Dispose()
    {
        Channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}

}
