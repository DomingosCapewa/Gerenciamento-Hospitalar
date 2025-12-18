using Agendamentos.Application.Interfaces;
using Agendamentos.Domain.Entities;
using Agendamentos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class PacienteRepository : IPacienteRepository
{
    private readonly AgendamentosDbContext _db;
    public PacienteRepository(AgendamentosDbContext db) => _db = db;
    public async Task<Paciente> AddAsync(Paciente p) { _db.Pacientes.Add(p); await _db.SaveChangesAsync(); return p; }
    public Task<Paciente?> GetByIdAsync(int id) => _db.Pacientes.FindAsync(id).AsTask();
    public Task<IEnumerable<Paciente>> GetAllAsync() => Task.FromResult<IEnumerable<Paciente>>( _db.Pacientes.AsNoTracking().ToList() );
    public async Task<Paciente> UpdateAsync(Paciente p) { _db.Pacientes.Update(p); await _db.SaveChangesAsync(); return p; }
    public async Task DeleteAsync(Paciente p) { _db.Pacientes.Remove(p); await _db.SaveChangesAsync(); }
}
