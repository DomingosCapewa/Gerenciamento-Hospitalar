

using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces
{
    public interface IClinicaRepository
    {
       public Task<ConsultasClinicas> AtenderConsulta(ConsultasClinicas consulta);
       public Task<ConsultasClinicas?> GetConsultaByIdAsync(int id);
         public Task<IEnumerable<ConsultasClinicas>> GetAllConsultasAsync();

    }
}