using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces
{
    public interface IConsultaClinicaRepository
    {
        Task<bool> HorarioOcupadoAsync(int? medicoId, DateTime dataHora);
        Task<bool> HorarioOcupadoPorCpfAsync(string cpf, DateTime dataHora);
        Task<ConsultasClinicas?> GetByIdAsync(int id);
        Task<ConsultasClinicas?> GetByCpfAndHorarioAsync(string cpf, DateTime dataHora);
        Task<ConsultasClinicas?> GetByCpfAndIdConsultaAsync(string cpf, int idConsulta);
        Task<ConsultasClinicas> AddAsync(ConsultasClinicas c);
        Task<ConsultasClinicas> UpdateAsync(ConsultasClinicas c);
        Task<IEnumerable<ConsultasClinicas>> GetAllConsultasAsync();
        Task<ConsultasClinicas> AtenderConsulta(ConsultasClinicas consulta);
    }
}