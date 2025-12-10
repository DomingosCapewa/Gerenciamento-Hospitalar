using Clinica.Infrastructure.Persistence;


namespace Clinica.Infrastructure.Repositories
{

    public class ClinicaRepository
    {
        private readonly ClinicaDbContext _context;
        public ClinicaRepository(ClinicaDbContext context)
        {
            _context = context;
        }

    
        
    }
}