using Shared.Contracts.Contracts;

namespace Shared.Contracts.Events;

public class ExameCriadaEvent : IAgendamentoEvent
{
    public int IdAgendamento { get; set; }
    public string CpfPaciente { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public string TipoExame { get; set; } = string.Empty;
}
