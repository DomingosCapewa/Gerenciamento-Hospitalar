namespace Agendamentos.Domain.Entities;
public class Exame
{
    public int Id { get; set; }
    public string CpfPaciente { get; set; } = string.Empty;
    public DateTime Horario { get; set; } 
    public string TipoExame { get; set; } = string.Empty;
    public string Status { get; set; } = "Agendado";
}
