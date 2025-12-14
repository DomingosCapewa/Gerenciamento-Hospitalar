using Shared.Contracts.DTOs;
using Shared.Contracts.Contracts;

namespace Shared.Contracts.Events;

public class ConsultaCriadaEvent : IAgendamentoEvent
{
    public int IdAgendamento { get; set; }
    public string CpfPaciente { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public string MedicoEspecialidade { get; set; } = string.Empty;
}
