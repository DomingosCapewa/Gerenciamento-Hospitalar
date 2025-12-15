using Shared.Contracts.Contracts;
using Shared.Contracts.Enums;

namespace Shared.Contracts.Events;

public class ProcedimentoExameCriadoEvent : IAgendamentoEvent
{
    public int IdProcedimento { get; set; }
    public int IdExameAgendamento { get; set; }
    public string CpfPaciente { get; set; } = string.Empty;
    public string TipoProcedimento { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public PrioridadeSintoma? Prioridade { get; set; }
    public string Status { get; set; } = string.Empty;
}
