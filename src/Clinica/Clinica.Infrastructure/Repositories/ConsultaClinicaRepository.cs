
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
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ConsultasClinicas?> GetByCpfAndDateAsync(string cpf, DateTime dataHora)
        {
            return await _context.ConsultasClinicas
                .Include(c => c.Sintomas)
                .FirstOrDefaultAsync(c => c.CpfPaciente == cpf && c.Horario == dataHora);
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
    }
}