namespace Clinica.Domain.Entities;
public class ConsultaClinica
{
    public int Id { get; set; }
    public int PacienteId { get; set; }        // id referenciado no Agendamentos (ou CPF se preferir)
    public string PacienteCPF { get; set; } = string.Empty; // para atender via CPF
    public int? MedicoId { get; set; }         // null até atribuição
    public DateTime DataHora { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string Status { get; set; } = "Agendado";
    public ICollection<ConsultaSintoma> Sintomas { get; set; } = new List<ConsultaSintoma>();
}
