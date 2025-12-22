using Agendamentos.Application.DTOs;
using Agendamentos.Application.Interfaces;
using Agendamentos.Domain.Entities;
using Shared.Contracts.Events;
using Shared.Contracts.Enums;
using Agendamentos.Infrastructure.Repositories;
using Shared.Contracts.Common;
using Shared.Contracts.Messaging;

namespace Agendamentos.API.Services;

public class SchedulingService : ISchedulingService
{
    private readonly IAgendamentoRepository _repo;
    private readonly IAvailabilityService _availability;
    private readonly IRabbitPublisher _publisher;

    public SchedulingService(IAgendamentoRepository repo, IAvailabilityService availability, IRabbitPublisher publisher)
    { _repo = repo; _availability = availability; _publisher = publisher; }

    public async Task<(bool Success, object? Error, Consulta? Created)> ScheduleConsultaAsync(CadastroConsultaDto dto)
    {
        if (dto?.Paciente == null) return (false, new { erro = "Paciente é obrigatório" }, null);
        if (string.IsNullOrWhiteSpace(dto.Paciente.CPF)) return (false, new { erro = "CPF do paciente é obrigatório" }, null);
        if (dto.Horario == default) return (false, new { erro = "Horario é obrigatório" }, null);
        if (string.IsNullOrWhiteSpace(dto.Medico)) return (false, new { erro = "Medico (especialidade) é obrigatório" }, null);

        if (await _repo.ExistsPacienteAtAsync(dto.Paciente.CPF, dto.Horario))
            return (false, new { mensagem = "O paciente já possui agendamento no dia e hora informados." }, null);

        var (available, conflict) = await _availability.IsClinicaSpecialtyAvailableAsync(dto.Medico, dto.Horario);
        if (!available) return (false, conflict ?? new { mensagem = "Horário indisponível para a especialidade informada." }, null);

        var paciente = new Paciente { Nome = dto.Paciente.Nome, CPF = dto.Paciente.CPF, Idade = dto.Paciente.Idade, Sexo = dto.Paciente.Sexo };
        await _repo.EnsurePacienteAsync(paciente);
        var consulta = new Consulta { CpfPaciente = dto.Paciente.CPF, Horario = dto.Horario, MedicoEspecialidade = dto.Medico };
        var criado = await _repo.AddConsultaAsync(consulta);

        var evento = new ConsultaCriadaEvent
        {
            IdAgendamento = criado.Id,
            CpfPaciente = criado.CpfPaciente,
            Horario = criado.Horario,
            MedicoEspecialidade = criado.MedicoEspecialidade
        };
        _publisher.Publish(QueueNames.ClinicaConsultas, evento);
        return (true, null, criado);
    }

    public async Task<(bool Success, object? Error, Exame? Created)> ScheduleExameAsync(CadastroExameDto dto)
    {
        if (dto?.Paciente == null) return (false, new { erro = "Paciente é obrigatório" }, null);
        if (string.IsNullOrWhiteSpace(dto.Paciente.CPF)) return (false, new { erro = "CPF do paciente é obrigatório" }, null);
        if (dto.Horario == default) return (false, new { erro = "Horario é obrigatório" }, null);
        if (string.IsNullOrWhiteSpace(dto.Exame)) return (false, new { erro = "Exame é obrigatório" }, null);

        if (await _repo.ExistsPacienteAtAsync(dto.Paciente.CPF, dto.Horario))
            return (false, new { mensagem = "O paciente já possui agendamento no dia e hora informados." }, null);

        var (available, conflict) = await _availability.IsCirurgicoExamTypeAvailableAsync(dto.Exame, dto.Horario);
        if (!available) return (false, conflict ?? new { mensagem = "Horário indisponível para o tipo de exame informado." }, null);

        var paciente = new Paciente { Nome = dto.Paciente.Nome, CPF = dto.Paciente.CPF, Idade = dto.Paciente.Idade, Sexo = dto.Paciente.Sexo };
        await _repo.EnsurePacienteAsync(paciente);
        var exame = new Exame { CpfPaciente = dto.Paciente.CPF, Horario = dto.Horario, TipoExame = dto.Exame };
        var criado = await _repo.AddExameAsync(exame);

        var eventoExame = new ProcedimentoExameCriadoEvent
        {
            IdProcedimento = criado.Id,
            IdExameAgendamento = criado.Id,
            CpfPaciente = criado.CpfPaciente,
            Horario = criado.Horario,
            TipoProcedimento = criado.TipoExame,
            Prioridade = PrioridadeSintoma.Padrao,
            Status = "Recebido"
        };
        _publisher.Publish(QueueNames.CirurgicoProcedimentos, eventoExame);
        return (true, null, criado);
    }
}
