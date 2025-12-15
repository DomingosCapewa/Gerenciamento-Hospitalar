using Cirurgico.Domain.Entities;

namespace Cirurgico.Application.Interfaces;

public interface IExameRepository
{
    Task<ProcedimentoExame?> GetByExameAgendamentoAsync(int idExameAgendamento);
    Task AddAsync(ProcedimentoExame exame);
    Task<bool> UpdatePrioridadeAsync(int idExameAgendamento, Shared.Contracts.Enums.PrioridadeSintoma? prioridade);
    Task<bool> UpdateStatusAsync(int idExameAgendamento, string status);
}
