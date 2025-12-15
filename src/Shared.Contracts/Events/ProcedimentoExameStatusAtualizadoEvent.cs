using Shared.Contracts.Contracts;

namespace Shared.Contracts.Events;

public class ProcedimentoExameStatusAtualizadoEvent : IClinicaEvent
{
    public int IdExameAgendamento { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AtualizadoPor { get; set; }
}
