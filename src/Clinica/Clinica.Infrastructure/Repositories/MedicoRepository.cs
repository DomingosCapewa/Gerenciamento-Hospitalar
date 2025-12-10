

using Clinica.Application.Interfaces;
using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Clinica.Domain.Entities;

namespace Clinica.Infrastructure.Repositories
{
    public class MedicoRepository: IMedicoRepository
    {
        private readonly ClinicaDbContext _context;
        public MedicoRepository(ClinicaDbContext context)
        {
            _context = context;
        }

        public async Task<Medico> AddMedicoAsync(Medico medico)
        {
           try
           {
                await _context.Medicos.AddAsync(medico);
                await _context.SaveChangesAsync();
                return medico;
           }
           catch(Exception ex)
           {
                throw new Exception("Erro ao adicionar médico: " + ex.Message);
           }
        }

        public async Task<bool> DeleteMedicoAsync(int id)
        {
            try
            {
                var medico = await _context.Medicos.FindAsync(id);
                if (medico == null)
                {
                    return false;
                }

                _context.Medicos.Remove(medico);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao deletar médico: " + ex.Message);
            }
        }

        public async Task<IEnumerable<Medico>> GetAllMedicosAsync()
        {
            var medicos = await _context.Medicos.ToListAsync();
            return medicos;
        }

        public async Task<Medico> GetMedicoByIdAsync(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            return medico;
        }

        public async Task<Medico> UpdateMedicoAsync(Medico medico)
        {
            try
            {
                _context.Medicos.Update(medico);
                await _context.SaveChangesAsync();
                return medico;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao atualizar médico: " + ex.Message);
            }
        }
    }
}