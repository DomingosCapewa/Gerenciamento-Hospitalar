namespace Agendamentos.Application.DTOs;

public class CreatePacienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}
