namespace Agendamentos.Application.DTOs;
public class PacienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Sexo { get; set; } = string.Empty;
}
