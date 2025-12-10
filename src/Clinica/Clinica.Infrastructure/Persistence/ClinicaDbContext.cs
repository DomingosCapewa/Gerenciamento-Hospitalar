using Microsoft.EntityFrameworkCore;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options)
        : base(options) { }

    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<ConsultaClinica> ConsultasClinicas => Set<ConsultaClinica>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Medico>().HasIndex(m => m.Nome);
        modelBuilder.Entity<ConsultaClinica>().HasIndex(c => new { c.MedicoId, c.DataHora });
    }
}
