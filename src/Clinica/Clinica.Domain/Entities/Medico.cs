using System.ComponentModel.DataAnnotations;

namespace Clinica.Domain.Entities;
public class Medico
{
    [Key]
    public int IdMedico { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CRM { get; set; } = null!;
    public string Especialidade { get; set; } = string.Empty;
}
