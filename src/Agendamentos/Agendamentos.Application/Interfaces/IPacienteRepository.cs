using Agendamentos.Domain.Entities;
public interface IPacienteRepository
{
    Task<Paciente> AddAsync(Paciente p);
    Task<Paciente?> GetByIdAsync(int id);
    Task<IEnumerable<Paciente>> GetAllAsync();
}
