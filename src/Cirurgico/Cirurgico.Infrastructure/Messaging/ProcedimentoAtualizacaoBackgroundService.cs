using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using Cirurgico.Application.Interfaces;
using Shared.Contracts.Enums;

namespace Cirurgico.Infrastructure.Messaging;

public class ProcedimentoAtualizacaoBackgroundService : BackgroundService
{
    private readonly ILogger<ProcedimentoAtualizacaoBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "cirurgico-procedimentos-atualizacoes";

    public ProcedimentoAtualizacaoBackgroundService(
        ILogger<ProcedimentoAtualizacaoBackgroundService> logger,
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var host = _configuration["RabbitMQ:Host"] ?? "localhost";
        var vhost = _configuration["RabbitMQ:VirtualHost"] ?? "/";
        var user = _configuration["RabbitMQ:Username"] ?? "guest";
        var pass = _configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = host,
            VirtualHost = vhost,
            UserName = user,
            Password = pass,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.BasicQos(0, 1, false);
        _logger.LogInformation("[Cirurgico] Consumer de atualizações conectado ao RabbitMQ em {Host}.", host);
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("Canal RabbitMQ não inicializado (atualizações).");
            return Task.CompletedTask;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IExameRepository>();

            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                // Tenta prioridade primeiro
                var prioridadeEvt = JsonSerializer.Deserialize<ProcedimentoExamePrioridadeAtualizadaEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (prioridadeEvt?.IdExameAgendamento > 0 && prioridadeEvt.Prioridade != null)
                {
                    await repo.UpdatePrioridadeAsync(prioridadeEvt.IdExameAgendamento, prioridadeEvt.Prioridade);
                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("[Cirurgico] Prioridade atualizada para ExAgId={ExAgId}: {Prioridade}", prioridadeEvt.IdExameAgendamento, prioridadeEvt.Prioridade);
                    return;
                }

                // Tenta status
                var statusEvt = JsonSerializer.Deserialize<ProcedimentoExameStatusAtualizadoEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (statusEvt?.IdExameAgendamento > 0 && !string.IsNullOrWhiteSpace(statusEvt.Status))
                {
                    await repo.UpdateStatusAsync(statusEvt.IdExameAgendamento, statusEvt.Status);
                    _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                    _logger.LogInformation("[Cirurgico] Status atualizado para ExAgId={ExAgId}: {Status}", statusEvt.IdExameAgendamento, statusEvt.Status);
                    return;
                }

                _logger.LogWarning("[Cirurgico] Mensagem de atualização não reconhecida: {Json}", json);
                _channel!.BasicReject(ea.DeliveryTag, requeue: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cirurgico] Erro ao aplicar atualização do procedimento.");
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        try { _channel?.Close(); _connection?.Close(); } catch { }
        return Task.CompletedTask;
    }
}
