using Agendamentos.Domain.Entities;

namespace Agendamentos.Application.Interfaces
{
    public interface IPacienteRepository
    {
        Task<Paciente> AddAsync(Paciente p);
        Task<Paciente?> GetByIdAsync(int id);
        Task<IEnumerable<Paciente>> GetAllAsync();
        Task<Paciente> UpdateAsync(Paciente p);
        Task DeleteAsync(Paciente p);
    }
}

