using Cirurgico.Application.Interfaces;
using Cirurgico.Domain.Entities;
using Cirurgico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cirurgico.Infrastructure.Repositories;

public class ExameRepository : IExameRepository
{
    private readonly CirurgicoDbContext _db;
    public ExameRepository(CirurgicoDbContext db) { _db = db; }

    public async Task<ProcedimentoExame?> GetByExameAgendamentoAsync(int idExameAgendamento)
        => await _db.Exames.AsNoTracking().FirstOrDefaultAsync(x => x.IdExameAgendamento == idExameAgendamento);

    public async Task AddAsync(ProcedimentoExame exame)
    {
        _db.Exames.Add(exame);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> UpdatePrioridadeAsync(int idExameAgendamento, Shared.Contracts.Enums.PrioridadeSintoma? prioridade)
    {
        var entity = await _db.Exames.FirstOrDefaultAsync(x => x.IdExameAgendamento == idExameAgendamento);
        if (entity == null) return false;
        entity.Prioridade = prioridade;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int idExameAgendamento, string status)
    {
        var entity = await _db.Exames.FirstOrDefaultAsync(x => x.IdExameAgendamento == idExameAgendamento);
        if (entity == null) return false;
        entity.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }
}
