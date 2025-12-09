

namespace Agendamentos.Domain.Entities
{
    public class Paciente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = null!;
        public string CPF { get; set; } = null!;    }
}