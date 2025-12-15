namespace Clinica.Domain.Entities;
public class SintomaDoenca
{
    public int SintomaDoencaId { get; set; }
   public string NomeSintoma { get; set; } = string.Empty;
   public string DoencasAssociadas { get; set; } = string.Empty;
   public int NivelPrioridade { get; set; }
}
