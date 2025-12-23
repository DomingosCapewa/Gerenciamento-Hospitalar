using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Events;
using Shared.Contracts.Messaging;
using QueueNamesAlias = Shared.Contracts.Common.QueueNames;

namespace Clinica.Infrastructure.Messaging;

public class ConsultaCriadaConsumer : RabbitMqConsumerBase<ConsultaCriadaEvent>
{
    protected override string QueueName => QueueNamesAlias.ClinicaConsultas;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    public ConsultaCriadaConsumer(
        ILogger<ConsultaCriadaConsumer> logger,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory)
        : base(logger, options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(ConsultaCriadaEvent message, CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var consultasRepo = scope.ServiceProvider.GetRequiredService<IConsultaClinicaRepository>();

        var existente = await consultasRepo.GetByCpfAndHorarioAsync(message.CpfPaciente, message.Horario);
        if (existente == null)
        {
            var consulta = new ConsultasClinicas
            {
                IdConsultaAgendamento = message.IdAgendamento,
                CpfPaciente = message.CpfPaciente,
                Horario = message.Horario,
                Especialidade = message.MedicoEspecialidade,
                Diagnostico = string.Empty,
                TratamentoSugerido = string.Empty,
                Sintomas = new List<ConsultaSintoma>()
            };

            await consultasRepo.AddAsync(consulta);
            _logger.LogInformation("[Clinica] Consulta inserida: AgId={AgId}, CPF={CPF}, Horario={Horario}", message.IdAgendamento, message.CpfPaciente, message.Horario);
        }
        else
        {
            _logger.LogInformation("[Clinica] Consulta já existente para CPF={CPF} em {Horario}", message.CpfPaciente, message.Horario);
        }
    }
}
