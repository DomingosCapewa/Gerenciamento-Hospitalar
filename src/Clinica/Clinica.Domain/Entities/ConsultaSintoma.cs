namespace Clinica.Domain.Entities;
public class ConsultaSintoma
{
    public int Id { get; set; }
    public int ConsultaClinicaId { get; set; }
    public ConsultasClinicas ConsultasClinicas { get; set; } = null!;
    public int SintomaId { get; set; }
    public Sintoma Sintoma { get; set; } = null!;
    public PrioridadeSintoma Prioridade { get; set; }
    public string Observacao { get; set; } = string.Empty;
}
