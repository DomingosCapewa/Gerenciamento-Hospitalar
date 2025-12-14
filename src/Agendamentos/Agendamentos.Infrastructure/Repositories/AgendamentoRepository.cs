using Agendamentos.Application.Interfaces;
using Agendamentos.Domain.Entities;
using Agendamentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Agendamentos.Infrastructure.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly AgendamentosDbContext _db;
    public AgendamentoRepository(AgendamentosDbContext db) => _db = db;

    public async Task<Paciente> EnsurePacienteAsync(Paciente p)
    {
        var existing = await _db.Pacientes.FirstOrDefaultAsync(x => x.CPF == p.CPF);
        if (existing != null) return existing;
        _db.Pacientes.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<bool> ExistsPacienteAtAsync(string cpf, DateTime hora)
    {
        var hasConsulta = await _db.Consultas.AnyAsync(c => c.CpfPaciente == cpf && c.Horario == hora);
        var hasExame = await _db.Exames.AnyAsync(e => e.CpfPaciente == cpf && e.Horario == hora);
        return hasConsulta || hasExame;
    }

    public async Task<Consulta> AddConsultaAsync(Consulta c)
    {
        _db.Consultas.Add(c);
        await _db.SaveChangesAsync();
        return c;
    }


    public async Task<Exame> AddExameAsync(Exame e)
    {
        _db.Exames.Add(e);
        await _db.SaveChangesAsync();
        return e;
    }

    public async Task<Consulta?> GetConsultaByIdAsync(int id)
    {
        return await _db.Consultas.FindAsync(id);
    }
    public async Task<Exame?> GetExameByIdAsync(int id)
    {
        return await _db.Exames.FindAsync(id);
    }

    public async Task<Consulta?> GetConsultaByCPFAsync(string cpf)
    {
        return await _db.Consultas.FirstOrDefaultAsync(c => c.CpfPaciente == cpf);
    }

    public async Task<Exame?> GetExameByCPFAsync(string cpf)
    {
        return await _db.Exames.FirstOrDefaultAsync(e => e.CpfPaciente == cpf);
    }

}
