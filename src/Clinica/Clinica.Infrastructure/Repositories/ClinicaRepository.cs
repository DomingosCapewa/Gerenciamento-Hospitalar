using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories
{
    public class ClinicaRepository : IClinicaRepository
    {
        private readonly ClinicaDbContext _db;

        public ClinicaRepository(ClinicaDbContext db)
        {
            _db = db;
        }

        public async Task<ConsultasClinicas?> GetConsultaByIdAsync(int id)
        {
            return await _db.ConsultasClinicas
                .Include(c => c.Sintomas)
                .ThenInclude(cs => cs.Sintoma)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ConsultasClinicas>> GetAllConsultasAsync()
        {
            return await _db.ConsultasClinicas
                .Include(c => c.Sintomas)
                .ThenInclude(cs => cs.Sintoma)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ConsultasClinicas> AtenderConsulta(ConsultasClinicas consulta)
        {
            // Anexa sintomas e calcula prioridade geral baseada no maior nível informado
            if (consulta.Sintomas != null && consulta.Sintomas.Count > 0)
            {
                foreach (var s in consulta.Sintomas)
                {
                    // Se o Sintoma não estiver carregado, tente obter do banco
                    if (s.Sintoma == null)
                    {
                        s.Sintoma = await _db.Sintomas.FindAsync(s.SintomaId) ?? new Sintoma { Id = s.SintomaId };
                    }
                }
            }

            _db.ConsultasClinicas.Update(consulta);
            await _db.SaveChangesAsync();
            return consulta;
        }
    }
    }