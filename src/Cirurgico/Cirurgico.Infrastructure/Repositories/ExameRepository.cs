using Cirurgico.Application.Interfaces;
using Cirurgico.Domain.Entities;
using Cirurgico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cirurgico.Infrastructure.Repositories;

public class ExameRepository : IExameRepository
{
    private readonly CirurgicoDbContext _db;
    public ExameRepository(CirurgicoDbContext db) { _db = db; }

    public async Task<ExamePedido?> GetByAgendamentoAsync(int idAgendamento)
        => await _db.Exames.AsNoTracking().FirstOrDefaultAsync(x => x.IdAgendamento == idAgendamento);

    public async Task AddAsync(ExamePedido exame)
    {
        _db.Exames.Add(exame);
        await _db.SaveChangesAsync();
    }
}
