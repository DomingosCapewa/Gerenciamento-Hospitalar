namespace Agendamentos.Application.Interfaces;

public interface IAvailabilityService
{
    Task<(bool Available, object? ConflictPayload)> IsClinicaSpecialtyAvailableAsync(string especialidade, DateTime horario);
    Task<(bool Available, object? ConflictPayload)> IsCirurgicoExamTypeAvailableAsync(string tipoExame, DateTime horario);
}
