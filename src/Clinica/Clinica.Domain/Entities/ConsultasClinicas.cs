namespace Clinica.Domain.Entities;
public class ConsultasClinicas
{
    public int Id { get; set; }
    public int IdConsultaAgendamento { get; set; }
    public string CpfPaciente { get; set; } = string.Empty; 
    public int? IdMedico { get; set; }      
    public DateTime Horario { get; set; }
    public string Especialidade { get; set; } = string.Empty;
    // public string Sintomas { get; set; } = string.Empty;
    public ICollection<ConsultaSintoma> Sintomas { get; set; } = new List<ConsultaSintoma>();
    public string Diagnostico { get; set; } = string.Empty;
    public string TratamentoSugerido { get; set; } = string.Empty;
}
