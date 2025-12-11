using Agendamentos.Application.DTOs;

namespace Shared.Contracts.DTOs;

public class ExameAgendamentoDto
{
    public int IdAgendamento { get; set; }
    public PacienteDto Paciente { get; set; } = new();
    public DateTime DataHora { get; set; }
    public string TipoExame { get; set; } = string.Empty;
}
