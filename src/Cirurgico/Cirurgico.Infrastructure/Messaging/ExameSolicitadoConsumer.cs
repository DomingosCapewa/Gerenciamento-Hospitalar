using Cirurgico.Application.Interfaces;
using Cirurgico.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QueueNamesAlias = Shared.Contracts.Common.QueueNames;
using Shared.Contracts.Events;
using Shared.Contracts.Messaging;

namespace Cirurgico.Infrastructure.Messaging;

public class ExameSolicitadoConsumer : RabbitMqConsumerBase<ProcedimentoExameCriadoEvent>
{
    protected override string QueueName => QueueNamesAlias.CirurgicoProcedimentos;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    public ExameSolicitadoConsumer(
        ILogger<ExameSolicitadoConsumer> logger,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory)
        : base(logger, options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task HandleMessageAsync(ProcedimentoExameCriadoEvent message, CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IExameRepository>();

        var existente = await repo.GetByExameAgendamentoAsync(message.IdExameAgendamento);
        if (existente == null)
        {
            var exame = new ProcedimentoExame
            {
                IdProcedimento = message.IdProcedimento,
                IdExameAgendamento = message.IdExameAgendamento,
                CpfPaciente = message.CpfPaciente,
                Horario = message.Horario,
                TipoProcedimento = message.TipoProcedimento,
                Prioridade = message.Prioridade,
                Status = string.IsNullOrWhiteSpace(message.Status) ? "Recebido" : message.Status
            };

            await repo.AddAsync(exame);
            _logger.LogInformation("[Cirurgico] Procedimento inserido: ProcId={ProcId}, ExAgId={ExAgId}, Tipo={Tipo}, Horario={Horario}", message.IdProcedimento, message.IdExameAgendamento, message.TipoProcedimento, message.Horario);
        }
        else
        {
            _logger.LogInformation("[Cirurgico] Procedimento já existente para ExAgId={ExAgId}", message.IdExameAgendamento);
        }
    }
}
