

using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces
{
    public interface IMedicoRepository
    {
         Task<Medico> GetMedicoByIdAsync(int id);
         Task<IEnumerable<Medico>> GetAllMedicosAsync();
         Task<Medico> AddMedicoAsync(Medico medico);
         Task<Medico> UpdateMedicoAsync(Medico medico);
         Task<bool> DeleteMedicoAsync(int id);
    }
}