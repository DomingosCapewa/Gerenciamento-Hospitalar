

namespace Agendamentos.Domain.Entities
{
public class Exame
{
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public string Tipo { get; set; } = null!;
        public DateTime DataHora { get; set; }
    }
}
