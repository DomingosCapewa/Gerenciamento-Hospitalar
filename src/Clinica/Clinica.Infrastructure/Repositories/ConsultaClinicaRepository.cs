
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
                .AnyAsync(c => c.MedicoId == medicoId && c.DataHora == dataHora);
        }

        public async Task<ConsultaClinica?> GetByIdAsync(int id)
        {
            return await _context.ConsultasClinicas
                .Include(c => c.Sintomas)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ConsultaClinica?> GetByCpfAndDateAsync(string cpf, DateTime dataHora)
        {
            return await _context.ConsultasClinicas
                .Include(c => c.Sintomas)
                .FirstOrDefaultAsync(c => c.PacienteCPF == cpf && c.DataHora == dataHora);
        }

        public async Task<ConsultaClinica> AddAsync(ConsultaClinica c)
        {
            await _context.ConsultasClinicas.AddAsync(c);
            await _context.SaveChangesAsync();
            return c;
        }

        public async Task<ConsultaClinica> UpdateAsync(ConsultaClinica c)
        {
            _context.ConsultasClinicas.Update(c);
            await _context.SaveChangesAsync();
            return c;
        }
    }
}