

namespace Agendamentos.Domain.Entities
{
   public class ExameAgendamento
{
   public int Id { get; set; }
   public string CPF { get; set; }
   public DateTime Horario { get; set; }
   public string TipoExame { get; set; }
}

}