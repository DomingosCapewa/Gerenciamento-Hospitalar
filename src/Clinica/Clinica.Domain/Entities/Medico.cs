namespace Clinica.Domain.Entities;
public class Medico
{
    public int IdMedico { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CRM { get; set; } = string.Empty; // Cadastro de Médico
    public string Especialidade { get; set; } = string.Empty;
}
