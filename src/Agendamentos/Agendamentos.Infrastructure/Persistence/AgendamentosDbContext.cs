using Microsoft.EntityFrameworkCore;
using Agendamentos.Domain.Entities;

namespace Agendamentos.Infrastructure.Persistence;
public class AgendamentosDbContext : DbContext
{
    public AgendamentosDbContext(DbContextOptions<AgendamentosDbContext> options) : base(options) { }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Exame> Exames => Set<Exame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Paciente>().HasIndex(p => p.CPF).IsUnique();
        modelBuilder.Entity<Consulta>().HasIndex(c => new { c.PacienteId, c.DataHora });
        modelBuilder.Entity<Exame>().HasIndex(e => new { e.PacienteId, e.DataHora });
    }
}
