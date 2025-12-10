

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinica.Infrastructure
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ClinicaDbContext>
    {
        public ClinicaDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ClinicaDbContext>();
            var conn = "Server=localhost;Port=3308;Database=clinica_db;User=clin_user;Password=clin_pass;";
            optionsBuilder.UseMySql(conn, ServerVersion.AutoDetect(conn));
            return new ClinicaDbContext(optionsBuilder.Options);
        }
    }
   
}