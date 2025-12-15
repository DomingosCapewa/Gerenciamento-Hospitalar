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
using Cirurgico.Domain.Entities;

namespace Cirurgico.Infrastructure.Messaging;

public class ExameSolicitadoBackgroundService : BackgroundService
{
    private readonly ILogger<ExameSolicitadoBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    private IConnection? _connection;
    private IModel? _channel;

    private const string QueueName = "cirurgico-procedimentos";

    public ExameSolicitadoBackgroundService(
        ILogger<ExameSolicitadoBackgroundService> logger,
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

        _logger.LogInformation("[Cirurgico] Consumer conectado ao RabbitMQ em {Host} vhost {VHost}.", host, vhost);
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("Canal RabbitMQ não inicializado.");
            return Task.CompletedTask;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IExameRepository>();

            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var mensagem = JsonSerializer.Deserialize<ProcedimentoExameCriadoEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (mensagem == null)
                {
                    _logger.LogWarning("[Cirurgico] Mensagem nula recebida.");
                    _channel!.BasicReject(ea.DeliveryTag, requeue: false);
                    return;
                }

                var existente = await repo.GetByExameAgendamentoAsync(mensagem.IdExameAgendamento);
                if (existente == null)
                {
                    var exame = new ProcedimentoExame
                    {
                        IdProcedimento = mensagem.IdProcedimento,
                        IdExameAgendamento = mensagem.IdExameAgendamento,
                        CpfPaciente = mensagem.CpfPaciente,
                        Horario = mensagem.Horario,
                        TipoProcedimento = mensagem.TipoProcedimento,
                        Prioridade = mensagem.Prioridade,
                        Status = string.IsNullOrWhiteSpace(mensagem.Status) ? "Recebido" : mensagem.Status
                    };

                    await repo.AddAsync(exame);

                    _logger.LogInformation("[Cirurgico] Procedimento inserido: ProcId={ProcId}, ExAgId={ExAgId}, Tipo={Tipo}, Horario={Horario}", mensagem.IdProcedimento, mensagem.IdExameAgendamento, mensagem.TipoProcedimento, mensagem.Horario);
                }
                else
                {
                    _logger.LogInformation("[Cirurgico] Procedimento já existente para ExAgId={ExAgId}", mensagem.IdExameAgendamento);
                }

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Cirurgico] Erro ao processar exame.");
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch { }
        return Task.CompletedTask;
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
