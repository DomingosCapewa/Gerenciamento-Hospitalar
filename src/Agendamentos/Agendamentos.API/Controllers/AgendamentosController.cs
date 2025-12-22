using Agendamentos.Application.DTOs;
using Agendamentos.Application.Interfaces;
using Agendamentos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Shared.Contracts.Events;
using Agendamentos.API.HATEOAS;
using Shared.Contracts.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using System.Net.Http.Json;

[ApiController]
[Route("api/agendamentos")]
[Authorize]
public class AgendamentosController : ControllerBase
{
    private readonly IAgendamentoRepository _repo;
    private readonly IRabbitPublisher _publisher;
    private readonly IHttpClientFactory _httpClientFactory;
    private HATEOAS HATEOAS;   

    public AgendamentosController(IAgendamentoRepository repo, IRabbitPublisher publisher, IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _publisher = publisher;
        _httpClientFactory = httpClientFactory;
        HATEOAS = new HATEOAS();
        HATEOAS.AddLink("self", "GET", "/api/agendamentos/pesquisar/consultas/{id}");
        HATEOAS.AddLink("update", "PATCH", "/api/agendamentos/alterar/consultas/{id}");
        HATEOAS.AddLink("delete", "DELETE", "/api/agendamentos/deletar/consultas/{id}");
    }

    

    [HttpPost("cadastro/consulta")]
    [Authorize(Policy = "Usuario")]
    [SwaggerOperation(Summary = "Cadastrar uma nova consulta", Description = "Adiciona uma nova consulta ao sistema.")]
    public async Task<IActionResult> CadastroConsulta([FromBody] CadastroConsultaDto dto)
    {
        if (dto?.Paciente == null) return BadRequest(new { erro = "Paciente é obrigatório" });
        if (string.IsNullOrWhiteSpace(dto.Paciente.CPF)) return BadRequest(new { erro = "CPF do paciente é obrigatório" });
        if (dto.Horario == default) return BadRequest(new { erro = "Horario é obrigatório" });
        if (string.IsNullOrWhiteSpace(dto.Medico)) return BadRequest(new { erro = "Medico (especialidade) é obrigatório" });

        var conflito = await _repo.ExistsPacienteAtAsync(dto.Paciente.CPF, dto.Horario);
        if (conflito)
            return Conflict(new { mensagem = $"O paciente já possui agendamento no dia e hora informados." });

        var clientClinica = _httpClientFactory.CreateClient("ClinicaApi");
        var queryConsultaUrl = $"/api/clinica/agenda/disponibilidade?especialidade={Uri.EscapeDataString(dto.Medico)}&horario={Uri.EscapeDataString(dto.Horario.ToString("o"))}";
        try
        {
            var resp = await clientClinica.GetAsync(queryConsultaUrl);
            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                var payload = await resp.Content.ReadFromJsonAsync<object>();
                return Conflict(payload ?? new { mensagem = "Horário indisponível para a especialidade informada." });
            }
        }
        catch { /* If upstream is down, proceed but local rules still apply */ }

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
        _publisher.Publish("clinica-consultas", evento);

        return Ok(new { mensagem = $"O medico {criado.MedicoEspecialidade} de {dto.Paciente.Nome} foi marcado para {criado.Horario:dd/MM/yyyy HH:mm}", codigo = criado.Id });
    }

    [HttpGet("pesquisar/consultas/{pacienteCPF}")]
    [Authorize(Policy = "Usuario")]
    [SwaggerOperation(Summary = "Obter detalhes da consulta ", Description = "Recupera os detalhes de uma consulta pelo pacienteCPF.")]
    public async Task<IActionResult> ObterConsulta(string pacienteCPF)
    {
        try
        {
            var consultasPaciente = await _repo.GetConsultaByCPFAsync(pacienteCPF);
            if (consultasPaciente == null)
            {
                return NotFound(new { mensagem = "Consulta não encontrada." });
            }
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var container = new AgendamentoContainer
            {
                Consulta = consultasPaciente,
                Links = HATEOAS.GetActions(baseUrl, consultasPaciente.Id.ToString())
            };

            return Ok(new { mensagem = $"Detalhes da consulta com CPF {pacienteCPF}.", data = container });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao obter detalhes da consulta.", detalhe = ex.Message });
        }
    }
    [HttpPatch("alterar/consultas/{id:int}")]
    [SwaggerOperation(Summary = "Atualizar detalhes da consulta", Description = "Atualiza horário, especialidade ou status da consulta.")]
    public async Task<IActionResult> AtualizarConsulta(int id, [FromBody] AtualizaConsultaDto body)
    {
        var existing = await _repo.GetConsultaByIdAsync(id);
        if (existing == null) return NotFound(new { mensagem = "Consulta não encontrada." });

        if (body.Horario.HasValue)
        {
            var conflito = await _repo.ExistsPacienteAtAsync(existing.CpfPaciente, body.Horario.Value);
            if (conflito)
                return Conflict(new { mensagem = "Paciente já possui agendamento neste dia e hora." });

            existing.Horario = body.Horario.Value;
        }
        if (!string.IsNullOrWhiteSpace(body.Medico)) existing.MedicoEspecialidade = body.Medico!;
        if (!string.IsNullOrWhiteSpace(body.Status)) existing.Status = body.Status!;

        var updated = await _repo.UpdateConsultaAsync(existing);
        return Ok(new { mensagem = $"Consulta {id} atualizada com sucesso.", data = updated });
    }
    [HttpDelete("deletar/consultas/{id:int}")]
    [SwaggerOperation(Summary = "Excluir uma consulta", Description = "Exclui uma consulta existente pelo ID.")]
    public async Task<IActionResult> ExcluirConsulta(int id)
    {
        var ok = await _repo.DeleteConsultaAsync(id);
        return ok ? Ok(new { mensagem = $"Consulta {id} excluída com sucesso." }) : NotFound(new { mensagem = "Consulta não encontrada." });
    }

    [HttpPost("cadastro/exame")]
    [Authorize(Policy = "Usuario")]
    [SwaggerOperation(Summary = "Cadastrar um novo exame", Description = "Adiciona um novo exame ao sistema.")]
    public async Task<IActionResult> CadastroExame([FromBody] CadastroExameDto dto)
    {
        if (dto?.Paciente == null) return BadRequest(new { erro = "Paciente é obrigatório" });
        if (string.IsNullOrWhiteSpace(dto.Paciente.CPF)) return BadRequest(new { erro = "CPF do paciente é obrigatório" });
        if (dto.Horario == default) return BadRequest(new { erro = "Horario é obrigatório" });
        if (string.IsNullOrWhiteSpace(dto.Exame)) return BadRequest(new { erro = "Exame é obrigatório" });

        var conflito = await _repo.ExistsPacienteAtAsync(dto.Paciente.CPF, dto.Horario);
        if (conflito)
            return Conflict(new { mensagem = $"O paciente já possui agendamento no dia e hora informados." });

        var clientCirurgico = _httpClientFactory.CreateClient("CirurgicoApi");
        var queryExameUrl = $"/api/cirurgico/procedimentos/disponibilidade?tipo={Uri.EscapeDataString(dto.Exame)}&horario={Uri.EscapeDataString(dto.Horario.ToString("o"))}";
        try
        {
            var resp = await clientCirurgico.GetAsync(queryExameUrl);
            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                var payload = await resp.Content.ReadFromJsonAsync<object>();
                return Conflict(payload ?? new { mensagem = "Horário indisponível para o tipo de exame informado." });
            }
        }
        catch { /* Upstream optional in this version; fall back to local rule */ }

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
        _publisher.Publish("cirurgico-procedimentos", eventoExame);

        return Ok(new { mensagem = $"O exame {criado.TipoExame} de {dto.Paciente.Nome} foi marcado para {criado.Horario:dd/MM/yyyy HH:mm}", codigo = criado.Id });
    }
    [HttpGet("pesquisar/exames/{pacienteId}")]
    [SwaggerOperation(Summary = "Obter detalhes do exame", Description = "Recupera os detalhes de um exame pelo pacienteId.")]
    public IActionResult ObterExame(int pacienteId)
    {
        try{
            var examesPaciente = _repo.GetExameByIdAsync(pacienteId);
            if (examesPaciente == null)
            {
                return NotFound(new { mensagem = "Exame não encontrado." });
            }
            return Ok(new { mensagem = $"Detalhes do exame com ID {pacienteId}.", data = examesPaciente });

        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensagem = "Erro ao obter detalhes do exame.", detalhe = ex.Message });
        }
    }

    [HttpPatch("alterar/exames/{id:int}")]
    [SwaggerOperation(Summary = "Atualizar detalhes do exame", Description = "Atualiza horário, tipo ou status do exame.")]
    public async Task<IActionResult> AtualizarExame(int id, [FromBody] AtualizaExameDto body)
    {
        var existing = await _repo.GetExameByIdAsync(id);
        if (existing == null) return NotFound(new { mensagem = "Exame não encontrado." });

        if (body.Horario.HasValue)
        {
            var conflito = await _repo.ExistsPacienteAtAsync(existing.CpfPaciente, body.Horario.Value);
            if (conflito)
                return Conflict(new { mensagem = "Paciente já possui agendamento neste dia e hora." });
            existing.Horario = body.Horario.Value;
        }
        if (!string.IsNullOrWhiteSpace(body.Exame)) existing.TipoExame = body.Exame!;
        if (!string.IsNullOrWhiteSpace(body.Status)) existing.Status = body.Status!;

        var updated = await _repo.UpdateExameAsync(existing);
        return Ok(new { mensagem = $"Exame {id} atualizado com sucesso.", data = updated });
    }

    [HttpDelete("deletar/exames/{id:int}")]
    [SwaggerOperation(Summary = "Excluir um exame", Description = "Exclui um exame existente pelo ID.")]
    public async Task<IActionResult> ExcluirExame(int id)
    {
        var ok = await _repo.DeleteExameAsync(id);
        return ok ? Ok(new { mensagem = $"Exame {id} excluído com sucesso." }) : NotFound(new { mensagem = "Exame não encontrado." });
    }

    
}
