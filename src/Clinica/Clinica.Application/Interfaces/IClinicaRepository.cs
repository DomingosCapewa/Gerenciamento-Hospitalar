

using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces
{
    public interface IClinicaRepository
    {
       public Task<ConsultasClinicas> AtenderConsulta(ConsultasClinicas consulta);
       public Task<ConsultasClinicas?> GetConsultaByCpfAndHorarioAsync(string cpf, DateTime horario);
       public Task<ConsultasClinicas?> GetConsultaByCpfAndIdAsync(string cpf, int consultaId);
       public Task<ConsultasClinicas?> GetConsultaByIdAsync(int id);
         public Task<IEnumerable<ConsultasClinicas>> GetAllConsultasAsync();

    }
}