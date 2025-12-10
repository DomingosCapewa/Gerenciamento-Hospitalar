

using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces
{
    public interface IClinicaRepository
    {
       public Task<ConsultaClinica> AtenderConsulta(ConsultaClinica consulta);
       public Task<ConsultaClinica?> GetConsultaByIdAsync(int id);
         public Task<IEnumerable<ConsultaClinica>> GetAllConsultasAsync();

    }
}