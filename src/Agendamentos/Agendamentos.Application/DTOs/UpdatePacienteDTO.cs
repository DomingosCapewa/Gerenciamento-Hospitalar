

namespace Agendamentos.Application.DTOs
{
    public class UpdatePacienteDTO
    {
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public int? Idade { get; set; }
        public string? Sexo { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
    }
}