using Agendamentos.Application.DTOs;
using Agendamentos.Domain.Entities;

namespace Agendamentos.Application.Interfaces;

public interface ISchedulingService
{
    Task<(bool Success, object? Error, Consulta? Created)> ScheduleConsultaAsync(CadastroConsultaDto dto);
    Task<(bool Success, object? Error, Exame? Created)> ScheduleExameAsync(CadastroExameDto dto);
}
