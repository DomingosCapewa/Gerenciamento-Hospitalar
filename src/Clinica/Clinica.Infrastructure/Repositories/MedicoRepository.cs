

using Clinica.Application.Interfaces;

namespace Clinica.Infrastructure.Repositories
{
    public class MedicoRepository: IMedicoRepository
    {
        private readonly ClinicaDbContext _context;
        public MedicoRepository(ClinicaDbContext context)
        {
            _context = context;
        }

        public Task<Medico> AddMedicoAsync(Medico medico)
        {
           try
           {
                _context.Medicos.Add(medico);
                _context.SaveChanges();
                return Task.FromResult(medico);
           }
           catch(Exception ex)
           {
                throw new Exception("Erro ao adicionar médico: " + ex.Message);
           }
        }

        public Task<bool> DeleteMedicoAsync(int id)
        {
            try
            {
                var medico = _context.Medicos.Find(id);
                if (medico == null)
                {
                    return Task.FromResult(false);
                }

                _context.Medicos.Remove(medico);
                _context.SaveChanges();
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao deletar médico: " + ex.Message);
            }
        }

        public Task<IEnumerable<Medico>> GetAllMedicosAsync()
        {
            var medicos = _context.Medicos.ToList();
            return Task.FromResult<IEnumerable<Medico>>(medicos);
        }

        public Task<Medico> GetMedicoByIdAsync(int id)
        {
            var medico = _context.Medicos.Find(id);
            return Task.FromResult(medico);
        }

        public Task<Medico> UpdateMedicoAsync(Medico medico)
        {
            try
            {
                _context.Medicos.Update(medico);
                _context.SaveChanges();
                return Task.FromResult(medico);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao atualizar médico: " + ex.Message);
            }
        }
    }
}