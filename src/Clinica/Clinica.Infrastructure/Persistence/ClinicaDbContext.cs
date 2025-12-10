using Clinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Persistence
{
    public class ClinicaDbContext : DbContext
    {
        public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options)
            : base(options) { }

        public DbSet<Medico> Medicos => Set<Medico>();
        public DbSet<ConsultaClinica> ConsultasClinicas => Set<ConsultaClinica>();
        public DbSet<Sintoma> Sintomas => Set<Sintoma>();
        public DbSet<Doenca> Doencas => Set<Doenca>();
        public DbSet<SintomaDoenca> SintomaDoencas => Set<SintomaDoenca>();
        public DbSet<ConsultaSintoma> ConsultaSintomas => Set<ConsultaSintoma>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ConsultaClinica>()
                .HasIndex(c => new { c.MedicoId, c.DataHora })
                .HasDatabaseName("IX_Consulta_Medico_DataHora");

            modelBuilder.Entity<Medico>()
                .HasIndex(m => m.CRM)
                .IsUnique();
            modelBuilder.Entity<Medico>().HasIndex(m => m.Nome);
            modelBuilder.Entity<ConsultaClinica>().HasIndex(c => new { c.MedicoId, c.DataHora });
        }
    }
}
