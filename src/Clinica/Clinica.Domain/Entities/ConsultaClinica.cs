namespace Clinica.Domain.Entities;
public class ConsultaClinica
{
    public int Id { get; set; }
    public int PacienteId { get; set; }        
    public string PacienteCPF { get; set; } = string.Empty; 
    public int? MedicoId { get; set; }         
    public DateTime DataHora { get; set; }
    public string Motivo { get; set; } = string.Empty; //verificar com o time se é necessário
    public string Status { get; set; } = "Agendado";
    public ICollection<ConsultaSintoma> Sintomas { get; set; } = new List<ConsultaSintoma>();
}
