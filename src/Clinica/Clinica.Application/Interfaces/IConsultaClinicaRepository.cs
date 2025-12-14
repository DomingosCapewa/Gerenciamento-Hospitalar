using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces
{
    public interface IConsultaClinicaRepository
    {
        Task<bool> HorarioOcupadoAsync(int? medicoId, DateTime dataHora);
        Task<bool> HorarioOcupadoPorCpfAsync(string cpf, DateTime dataHora);
        Task<ConsultasClinicas?> GetByIdAsync(int id);
        Task<ConsultasClinicas?> GetByCpfAndDateAsync(string cpf, DateTime dataHora);
        Task<ConsultasClinicas> AddAsync(ConsultasClinicas c);
        Task<ConsultasClinicas> UpdateAsync(ConsultasClinicas c);
    }
}