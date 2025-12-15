namespace Cirurgico.Domain.Entities;

public class ExamePedido
{
    public int Id { get; set; }
    public int IdAgendamento { get; set; }
    public string CpfPaciente { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public string TipoExame { get; set; } = string.Empty;
    public string Status { get; set; } = "Recebido"; // Recebido, EmAndamento, Concluido, Falhou
}
