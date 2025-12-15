using Shared.Contracts.Enums;

namespace Cirurgico.Domain.Entities;

public class ProcedimentoExame
{
    public int Id { get; set; }
    public int IdProcedimento { get; set; }
    public int IdExameAgendamento { get; set; }
    public string CpfPaciente { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public PrioridadeSintoma? Prioridade { get; set; }
    public string TipoProcedimento { get; set; } = string.Empty;
    public string Status { get; set; } = "Recebido";
}
