using Cirurgico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cirurgico.Infrastructure
{
    public class DesignTimeDbContext : IDesignTimeDbContextFactory<CirurgicoDbContext>
    {
        public CirurgicoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CirurgicoDbContext>();
            var conn = "Server=localhost;Port=3309;Database=cirurgico_db;User=ciru_user;Password=ciru_pass;";
            optionsBuilder.UseMySql(conn, ServerVersion.AutoDetect(conn));
            return new CirurgicoDbContext(optionsBuilder.Options);
        }
    }
        
    
}