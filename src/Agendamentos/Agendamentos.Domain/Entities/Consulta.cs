
namespace Agendamentos.Domain.Entities
{
    public class Consulta
    {
        public int Id { get; set; }
        public int PacienteId { get; set; }
        public string Medico { get; set; } = null!;
        public DateTime DataHora { get; set; }
        public string? Tipo { get; set; }
        public int? ExameId { get; set; }
    }
}