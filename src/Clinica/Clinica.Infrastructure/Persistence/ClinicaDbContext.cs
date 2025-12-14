using Clinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Persistence
{
    public class ClinicaDbContext : DbContext
    {
        public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options)
            : base(options) { }

        public DbSet<Medico> Medicos => Set<Medico>();
        public DbSet<ConsultasClinicas> ConsultasClinicas => Set<ConsultasClinicas>();
        public DbSet<Sintoma> Sintomas => Set<Sintoma>();
        public DbSet<Doenca> Doencas => Set<Doenca>();
        public DbSet<SintomaDoenca> SintomaDoencas => Set<SintomaDoenca>();
        public DbSet<ConsultaSintoma> ConsultaSintomas => Set<ConsultaSintoma>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ConsultasClinicas>()
                .HasIndex(c => new { c.IdMedico, c.Horario })
                .HasDatabaseName("IX_Consulta_Medico_Horario");

            modelBuilder.Entity<Medico>()
                .HasIndex(m => m.CRM)
                .IsUnique();
            modelBuilder.Entity<Medico>().HasIndex(m => m.Nome);
            modelBuilder.Entity<ConsultasClinicas>().HasIndex(c => new { c.IdMedico, c.Horario });
        }
    }
}
