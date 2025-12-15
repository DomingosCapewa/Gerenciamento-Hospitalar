using System.Text;
using System.Text.RegularExpressions;
using Clinica.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Enums;
using Shared.Contracts.Events;

namespace Clinica.API.Controllers;

[ApiController]
[Route("api/procedimentos")] 
public class ProcedimentosController : ControllerBase
{
    private readonly EventPublisher _publisher;
    public ProcedimentosController(EventPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPut("{idExameAgendamento:int}/prioridade")] 
    public IActionResult AtualizarPrioridade([FromRoute] int idExameAgendamento, [FromBody] AtualizaPrioridadeRequest req)
    {
        var prioridade = NormalizarPrioridade(req.Prioridade);
        var evt = new ProcedimentoExamePrioridadeAtualizadaEvent
        {
            IdExameAgendamento = idExameAgendamento,
            Prioridade = prioridade,
            AtualizadoPor = req.AtualizadoPor
        };
        _publisher.Publish("cirurgico-procedimentos-atualizacoes", evt);
        return Accepted(new { message = "Prioridade enviada para processamento", idExameAgendamento, prioridade = prioridade?.ToString() });
    }

    [HttpPut("{idExameAgendamento:int}/status")] 
    public IActionResult AtualizarStatus([FromRoute] int idExameAgendamento, [FromBody] AtualizaStatusRequest req)
    {
        var status = string.IsNullOrWhiteSpace(req.Status) ? "" : req.Status.Trim();
        var evt = new ProcedimentoExameStatusAtualizadoEvent
        {
            IdExameAgendamento = idExameAgendamento,
            Status = status,
            AtualizadoPor = req.AtualizadoPor
        };
        _publisher.Publish("cirurgico-procedimentos-atualizacoes", evt);
        return Accepted(new { message = "Status enviado para processamento", idExameAgendamento, status });
    }

    private static PrioridadeSintoma? NormalizarPrioridade(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        var s = RemoverAcentos(valor).Trim();
        if (Enum.TryParse<PrioridadeSintoma>(s, ignoreCase: true, out var parsed)) return parsed;
        // Mapear aliases comuns
        return s.ToLowerInvariant() switch
        {
            "padrao" or "padr" => PrioridadeSintoma.Padrao,
            "baixa" => PrioridadeSintoma.Baixa,
            "alta" => PrioridadeSintoma.Alta,
            "emergencial" or "emergencia" => PrioridadeSintoma.Emergencial,
            _ => null
        };
    }

    private static string RemoverAcentos(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
        return regex.Replace(normalized, string.Empty).Normalize(NormalizationForm.FormC);
    }

    public class AtualizaPrioridadeRequest { public string? Prioridade { get; set; } public string? AtualizadoPor { get; set; } }
    public class AtualizaStatusRequest { public string Status { get; set; } = string.Empty; public string? AtualizadoPor { get; set; } }
}
