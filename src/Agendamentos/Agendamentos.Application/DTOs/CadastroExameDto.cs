using System;
namespace Agendamentos.Application.DTOs;
public class CadastroExameDto
{
   public PacienteDto Paciente { get; set; } = new();
   public DateTime Horario { get; set; }
   public string Exame { get; set; } = string.Empty; 
}
