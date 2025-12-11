namespace Agendamentos.Domain.Entities;
public class Consulta
{
    public int Id { get; set; }
    public string PacienteCpf { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public string MedicoEspecialidade { get; set; } = string.Empty;
    public string Status { get; set; } = "Agendado";
}
