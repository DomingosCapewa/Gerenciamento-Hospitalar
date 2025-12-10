namespace Clinica.Domain.Entities;
public enum PrioridadeSintoma { Baixa = 1, Padrao = 2, Alta = 3, Emergencial = 4 }

public class Sintoma
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public PrioridadeSintoma PrioridadePadrao { get; set; } = PrioridadeSintoma.Padrao;
}
