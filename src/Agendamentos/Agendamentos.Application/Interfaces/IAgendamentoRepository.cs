using Agendamentos.Domain.Entities;

namespace Agendamentos.Application.Interfaces
{
    public interface IAgendamentoRepository
    {
        Task<Paciente> EnsurePacienteAsync(Paciente p);
        Task<bool> ExistsPacienteAtAsync(string cpf, DateTime hora);
        Task<Consulta> AddConsultaAsync(Consulta c);
        Task<Exame> AddExameAsync(Exame e);
        Task<Consulta?> GetConsultaByIdAsync(int id);
        Task<Consulta?> GetConsultaByCPFAsync(string cpf);
        Task<Exame?> GetExameByIdAsync(int id);
        Task<Consulta?> UpdateConsultaAsync(Consulta c);
        Task<bool> DeleteConsultaAsync(int id);
        Task<Exame?> UpdateExameAsync(Exame e);
        Task<bool> DeleteExameAsync(int id);
    }
}
