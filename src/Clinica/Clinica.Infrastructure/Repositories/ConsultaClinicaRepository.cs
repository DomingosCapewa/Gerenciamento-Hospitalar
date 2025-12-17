
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories
{
    public class ConsultaClinicaRepository : IConsultaClinicaRepository
    {
        private readonly ClinicaDbContext _context;

        public ConsultaClinicaRepository(ClinicaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HorarioOcupadoAsync(int? medicoId, DateTime dataHora)
        {
            return await _context.ConsultasClinicas
                .AnyAsync(c => c.IdMedico == medicoId && c.Horario == dataHora);
        }

        public async Task<bool> HorarioOcupadoPorCpfAsync(string cpf, DateTime dataHora)
        {
            return await _context.ConsultasClinicas
                .AnyAsync(c => c.CpfPaciente == cpf && c.Horario == dataHora);
        }

        public async Task<ConsultasClinicas?> GetByIdAsync(int id)
        {
            return await _context.ConsultasClinicas
                .Include(c => c.Sintomas)
                .ThenInclude(cs => cs.Sintoma)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ConsultasClinicas?> GetByCpfAndHorarioAsync(string cpf, DateTime dataHora)
        {
            return await _context.ConsultasClinicas
                .Include(c => c.Sintomas)
                .ThenInclude(cs => cs.Sintoma)
                .FirstOrDefaultAsync(c => c.CpfPaciente == cpf && c.Horario == dataHora);
        }

        public async Task<ConsultasClinicas?> GetByCpfAndIdConsultaAsync(string cpf, int idConsulta)
        {
            return await _context.ConsultasClinicas
                .Include(c => c.Sintomas)
                .ThenInclude(cs => cs.Sintoma)
                .FirstOrDefaultAsync(c => c.CpfPaciente == cpf && c.Id == idConsulta);
        }

        public async Task<ConsultasClinicas> AddAsync(ConsultasClinicas c)
        {
            await _context.ConsultasClinicas.AddAsync(c);
            await _context.SaveChangesAsync();
            return c;
        }

        public async Task<ConsultasClinicas> UpdateAsync(ConsultasClinicas c)
        {
            _context.ConsultasClinicas.Update(c);
            await _context.SaveChangesAsync();
            return c;
        }

        public async Task<IEnumerable<ConsultasClinicas>> GetAllConsultasAsync()
        {
            return await _context.ConsultasClinicas
                .Include(c => c.Sintomas)
                .ThenInclude(cs => cs.Sintoma)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ConsultasClinicas> AtenderConsulta(ConsultasClinicas consulta)
        {
            if (consulta.Sintomas != null && consulta.Sintomas.Count > 0)
            {
                foreach (var s in consulta.Sintomas)
                {
                    if (s.Sintoma == null)
                    {
                        s.Sintoma = await _context.Sintomas.FindAsync(s.SintomaId) ?? new Sintoma { Id = s.SintomaId };
                    }
                }
            }

            _context.ConsultasClinicas.Update(consulta);
            await _context.SaveChangesAsync();
            return consulta;
        }
    }
}