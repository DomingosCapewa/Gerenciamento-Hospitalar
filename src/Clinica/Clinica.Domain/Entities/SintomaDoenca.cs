namespace Clinica.Domain.Entities;
public class SintomaDoenca
{
    public int SintomaId { get; set; }
    public Sintoma Sintoma { get; set; } = null!;
    public int DoencaId { get; set; }
    public Doenca Doenca { get; set; } = null!;
}
