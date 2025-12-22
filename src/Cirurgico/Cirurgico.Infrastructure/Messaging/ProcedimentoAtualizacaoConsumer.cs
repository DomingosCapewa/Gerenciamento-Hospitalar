using System.Text.Json;
using Cirurgico.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Contracts.Common;
using QueueNamesAlias = Shared.Contracts.Common.QueueNames;
using Shared.Contracts.Events;
using Shared.Contracts.Messaging;

namespace Cirurgico.Infrastructure.Messaging;

public class ProcedimentoAtualizacaoConsumer : RabbitMqConsumerBase<string>
{
    protected override string QueueName => QueueNamesAlias.CirurgicoProcedimentosAtualizacoes;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    public ProcedimentoAtualizacaoConsumer(
        ILogger<ProcedimentoAtualizacaoConsumer> logger,
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory)
        : base(logger, options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override string Deserialize(byte[] body)
    {
        return System.Text.Encoding.UTF8.GetString(body);
    }

    protected override async Task HandleMessageAsync(string json, CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IExameRepository>();

        try
        {
            var prioridadeEvt = JsonSerializer.Deserialize<ProcedimentoExamePrioridadeAtualizadaEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (prioridadeEvt?.IdExameAgendamento > 0 && prioridadeEvt.Prioridade != null)
            {
                await repo.UpdatePrioridadeAsync(prioridadeEvt.IdExameAgendamento, prioridadeEvt.Prioridade);
                _logger.LogInformation("[Cirurgico] Prioridade atualizada para ExAgId={ExAgId}: {Prioridade}", prioridadeEvt.IdExameAgendamento, prioridadeEvt.Prioridade);
                return;
            }

            var statusEvt = JsonSerializer.Deserialize<ProcedimentoExameStatusAtualizadoEvent>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (statusEvt?.IdExameAgendamento > 0 && !string.IsNullOrWhiteSpace(statusEvt.Status))
            {
                await repo.UpdateStatusAsync(statusEvt.IdExameAgendamento, statusEvt.Status);
                _logger.LogInformation("[Cirurgico] Status atualizado para ExAgId={ExAgId}: {Status}", statusEvt.IdExameAgendamento, statusEvt.Status);
                return;
            }

            _logger.LogWarning("[Cirurgico] Mensagem de atualização não reconhecida: {Json}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Cirurgico] Erro ao aplicar atualização do procedimento.");
            throw;
        }
    }
}
