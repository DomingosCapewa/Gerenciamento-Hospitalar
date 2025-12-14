namespace Shared.Contracts.Events;

public class ConsultaAtribuidaMedicoEvent
{
    public int IdConsultaClinica { get; set; }
    public int IdConsultaAgendamento { get; set; }
    public string CpfPaciente { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public int IdMedico { get; set; }
    public string Especialidade { get; set; } = string.Empty;
}
