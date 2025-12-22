using Agendamentos.Application.DTOs;
using Agendamentos.Application.Interfaces;
using Agendamentos.Domain.Entities;

namespace Agendamentos.API.Services;

public class AgendamentoService : IAgendamentoService
{
    private readonly IAgendamentoRepository _repo;
    private readonly ISchedulingService _scheduler;

    public AgendamentoService(IAgendamentoRepository repo, ISchedulingService scheduler)
    {
        _repo = repo;
        _scheduler = scheduler;
    }

    public async Task<Consulta> CriarConsultaAsync(CadastroConsultaDto dto)
    {
        var (ok, err, created) = await _scheduler.ScheduleConsultaAsync(dto);
        if (!ok || created == null) throw new InvalidOperationException($"Falha ao criar consulta: {System.Text.Json.JsonSerializer.Serialize(err)}");
        return created;
    }

    public Task<Consulta?> ObterConsultaPorIdAsync(int id) => _repo.GetConsultaByIdAsync(id);

    public async Task<IEnumerable<Consulta>> ObterConsultasPorCpfAsync(string cpf)
    {
        var one = await _repo.GetConsultaByCPFAsync(cpf);
        return one != null ? new[] { one } : Array.Empty<Consulta>();
    }

    public async Task AtualizarConsultaAsync(int id, AtualizaConsultaDto dto)
    {
        var existing = await _repo.GetConsultaByIdAsync(id) ?? throw new KeyNotFoundException("Consulta não encontrada");
        if (dto.Horario.HasValue) existing.Horario = dto.Horario.Value;
        if (!string.IsNullOrWhiteSpace(dto.Medico)) existing.MedicoEspecialidade = dto.Medico!;
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status!;
        await _repo.UpdateConsultaAsync(existing);
    }

    public Task ExcluirConsultaAsync(int id) => _repo.DeleteConsultaAsync(id);

    public async Task<Exame> CriarExameAsync(CadastroExameDto dto)
    {
        var (ok, err, created) = await _scheduler.ScheduleExameAsync(dto);
        if (!ok || created == null) throw new InvalidOperationException($"Falha ao criar exame: {System.Text.Json.JsonSerializer.Serialize(err)}");
        return created;
    }

    public Task<Exame?> ObterExamePorIdAsync(int id) => _repo.GetExameByIdAsync(id);

    public async Task AtualizarExameAsync(int id, AtualizaExameDto dto)
    {
        var existing = await _repo.GetExameByIdAsync(id) ?? throw new KeyNotFoundException("Exame não encontrado");
        if (dto.Horario.HasValue) existing.Horario = dto.Horario.Value;
        if (!string.IsNullOrWhiteSpace(dto.Exame)) existing.TipoExame = dto.Exame!;
        if (!string.IsNullOrWhiteSpace(dto.Status)) existing.Status = dto.Status!;
        await _repo.UpdateExameAsync(existing);
    }

    public Task ExcluirExameAsync(int id) => _repo.DeleteExameAsync(id);
}
