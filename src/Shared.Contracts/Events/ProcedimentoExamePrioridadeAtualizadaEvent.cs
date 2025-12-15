using Shared.Contracts.Contracts;
using Shared.Contracts.Enums;

namespace Shared.Contracts.Events;

public class ProcedimentoExamePrioridadeAtualizadaEvent : IClinicaEvent
{
    public int IdExameAgendamento { get; set; }
    public PrioridadeSintoma? Prioridade { get; set; }
    public string? AtualizadoPor { get; set; }
}
