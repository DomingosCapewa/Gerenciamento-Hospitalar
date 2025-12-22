using Agendamentos.Domain.Entities;

namespace Agendamentos.Application.Interfaces
{
    public interface IAgendamentoRepository
    {
        Task<Paciente> EnsurePacienteAsync(Paciente paciente);
        Task<bool> ExistsPacienteAtAsync(string cpf, DateTime hora);

        Task<Consulta> AddConsultaAsync(Consulta consulta);
        Task<Consulta?> GetConsultaByIdAsync(int id);
        Task<Consulta?> GetConsultaByCPFAsync(string cpf);
        Task<Consulta?> UpdateConsultaAsync(Consulta consulta);
        Task<bool> DeleteConsultaAsync(int id);

        Task<Exame> AddExameAsync(Exame exame);
        Task<Exame?> GetExameByIdAsync(int id);
        Task<Exame?> GetExameByCPFAsync(string cpf);
        Task<Exame?> UpdateExameAsync(Exame exame);
        Task<bool> DeleteExameAsync(int id);
    }

    public interface IPacienteRepository
    {
        Task<Paciente> AddAsync(Paciente paciente);
        Task<Paciente?> GetByIdAsync(int id);
        Task<Paciente?> GetByCpfAsync(string cpf);
        Task<IEnumerable<Paciente>> GetAllAsync();
        Task<Paciente> UpdateAsync(Paciente paciente);
        Task DeleteAsync(Paciente paciente);
    }
}