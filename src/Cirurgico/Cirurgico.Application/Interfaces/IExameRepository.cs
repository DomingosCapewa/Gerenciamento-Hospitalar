using Cirurgico.Domain.Entities;

namespace Cirurgico.Application.Interfaces;

public interface IExameRepository
{
    Task<ExamePedido?> GetByAgendamentoAsync(int idAgendamento);
    Task AddAsync(ExamePedido exame);
}
