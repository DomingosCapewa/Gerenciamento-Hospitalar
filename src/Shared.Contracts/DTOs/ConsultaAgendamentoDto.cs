using Agendamentos.Application.DTOs;

namespace Shared.Contracts.DTOs;

public class ConsultaAgendamentoDto
{
    public int IdAgendamento { get; set; }
    public PacienteDto Paciente { get; set; } = new();
    public DateTime DataHora { get; set; }
    public string Especialidade { get; set; } = string.Empty;
}
