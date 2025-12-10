

using Clinica.Domain.Entities;

namespace Clinica.Application.Interfaces
{
    public interface IConsultaClinicaRepository
    {
        Task<bool> HorarioOcupadoAsync(int? medicoId, DateTime dataHora);
        Task<ConsultaClinica?> GetByIdAsync(int id);
        Task<ConsultaClinica?> GetByCpfAndDateAsync(string cpf, DateTime dataHora);
        Task<ConsultaClinica> AddAsync(ConsultaClinica c);
        Task<ConsultaClinica> UpdateAsync(ConsultaClinica c);
    }
}