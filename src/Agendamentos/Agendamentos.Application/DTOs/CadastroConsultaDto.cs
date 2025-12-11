namespace Agendamentos.Application.DTOs;
public class CadastroConsultaDto
{
   public PacienteDto Paciente { get; set; } = new();
   public DateTime Horario { get; set; }
   public string Medico { get; set; } = string.Empty;
}
