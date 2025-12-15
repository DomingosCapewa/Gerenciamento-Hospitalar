using Cirurgico.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cirurgico.Infrastructure.Persistence;

public class CirurgicoDbContext : DbContext
{
    public CirurgicoDbContext(DbContextOptions<CirurgicoDbContext> options) : base(options) { }

    public DbSet<ExamePedido> Exames => Set<ExamePedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ExamePedido>()
            .HasIndex(e => new { e.IdAgendamento })
            .HasDatabaseName("IX_Exame_IdAgendamento");
    }
}
