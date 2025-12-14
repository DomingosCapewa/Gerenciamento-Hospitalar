using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;

namespace Clinica.Infrastructure.Messaging;

public class ConsultaCriadaBackgroundService : BackgroundService
{
    private readonly ILogger<ConsultaCriadaBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;
    private const string QueueName = "clinica-consultas";

    private readonly IConsultaClinicaRepository _consultasRepo;

    public ConsultaCriadaBackgroundService(ILogger<ConsultaCriadaBackgroundService> logger, IConfiguration configuration, IConsultaClinicaRepository consultasRepo)
    {
        _logger = logger;
        _configuration = configuration;
        _consultasRepo = consultasRepo;
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

        _logger.LogInformation("Clinica consumer conectado ao RabbitMQ em {Host} vhost {VHost}.", host, vhost);
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
        consumer.Received += async (ch, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var mensagem = JsonSerializer.Deserialize<ConsultaCriadaEvent>(json, options);

                if (mensagem == null)
                {
                    _logger.LogWarning("Mensagem nula recebida, rejeitando.");
                    _channel.BasicReject(ea.DeliveryTag, requeue: false);
                    return;
                }

                // Persistir no banco
                var existente = await _consultasRepo.GetByCpfAndDateAsync(mensagem.CpfPaciente, mensagem.Horario);
                if (existente == null)
                {
                    var consulta = new ConsultasClinicas
                    {
                        IdConsultaAgendamento = mensagem.IdAgendamento,
                        CpfPaciente = mensagem.CpfPaciente,
                        Horario = mensagem.Horario,
                        Especialidade = mensagem.MedicoEspecialidade,
                        // IdMedico pode ser resolvido por especialidade em outro fluxo
                        Diagnostico = string.Empty,
                        TratamentoSugerido = string.Empty,
                        Sintomas = new List<ConsultaSintoma>()
                    };
                    await _consultasRepo.AddAsync(consulta);
                    _logger.LogInformation("[Clinica] Consulta inserida: AgId={AgId}, CPF={CPF}, Horario={Horario}", mensagem.IdAgendamento, mensagem.CpfPaciente, mensagem.Horario);
                }
                else
                {
                    _logger.LogInformation("[Clinica] Consulta já existente para CPF={CPF} em {Horario}", mensagem.CpfPaciente, mensagem.Horario);
                }

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem de consulta criada.");
                _channel?.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
            await Task.CompletedTask;
        };

        _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

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
