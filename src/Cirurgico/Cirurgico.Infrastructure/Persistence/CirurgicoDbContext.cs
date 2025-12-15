using Cirurgico.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cirurgico.Infrastructure.Persistence;

public class CirurgicoDbContext : DbContext
{
    public CirurgicoDbContext(DbContextOptions<CirurgicoDbContext> options) : base(options) { }

    public DbSet<ProcedimentoExame> Exames => Set<ProcedimentoExame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ProcedimentoExame>()
            .HasIndex(e => new { e.IdExameAgendamento })
            .HasDatabaseName("IX_Exame_IdExameAgendamento");
    }
}
