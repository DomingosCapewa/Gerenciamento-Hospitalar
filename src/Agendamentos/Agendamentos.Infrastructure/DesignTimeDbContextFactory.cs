using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Agendamentos.Infrastructure.Persistence;
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AgendamentosDbContext>
{
    public AgendamentosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AgendamentosDbContext>();
        var conn = "Server=localhost;Port=3307;Database=agendamentos_db;User=agend_user;Password=agend_pass;";
        optionsBuilder.UseMySql(conn, ServerVersion.AutoDetect(conn));
        return new AgendamentosDbContext(optionsBuilder.Options);
    }
}
