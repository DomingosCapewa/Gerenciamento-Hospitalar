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
    private readonly IAgendamentoService _service;

    public AgendamentosController(IAgendamentoService service)
    {
        _service = service;
    }


    [HttpPost("~/api/cadastro/consulta")]
    [Authorize(Policy = "Usuario")]
    [SwaggerOperation(Summary = "Cadastro de consulta", Description = "Agenda uma nova consulta.")]
    public async Task<IActionResult> CadastroConsulta([FromBody] CadastroConsultaDto dto)
    {
        var result = await _service.CriarConsultaAsync(dto);
        var mensagem = $"O medico ({dto.Medico}) de {dto.Paciente?.Nome} foi marcado para {dto.Horario:yyyy-MM-dd HH:mm}";
        return Ok(new { mensagem, codigo = result.Id });
    }

    [HttpGet("consultas/{id:int}")]
    public async Task<IActionResult> ObterConsultaPorId(int id)
    {
        var consulta = await _service.ObterConsultaPorIdAsync(id);
        return consulta == null ? NotFound() : Ok(consulta);
    }

    [HttpGet("consultas/paciente/{cpf}")]
    public async Task<IActionResult> ObterConsultasPorCpf(string cpf)
    {
        var consultas = await _service.ObterConsultasPorCpfAsync(cpf);
        return Ok(consultas);
    }

    [HttpPatch("consultas/{id:int}")]
    public async Task<IActionResult> AtualizarConsulta(int id, AtualizaConsultaDto dto)
    {
        await _service.AtualizarConsultaAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("consultas/{id:int}")]
    public async Task<IActionResult> ExcluirConsulta(int id)
    {
        await _service.ExcluirConsultaAsync(id);
        return NoContent();
    }


    [HttpPost("~/api/cadastro/exame")]
    [Authorize(Policy = "Usuario")]
    [SwaggerOperation(Summary = "Cadastro de exame", Description = "Agenda um novo exame.")]
    public async Task<IActionResult> CadastroExame([FromBody] CadastroExameDto dto)
    {
        var result = await _service.CriarExameAsync(dto);
        var mensagem = $"O exame ({dto.Exame}) de {dto.Paciente?.Nome} foi marcado para {dto.Horario:yyyy-MM-dd HH:mm}";
        return Ok(new { mensagem, codigo = result.Id });
    }

    [HttpGet("exames/{id:int}")]
    public async Task<IActionResult> ObterExamePorId(int id)
    {
        var exame = await _service.ObterExamePorIdAsync(id);
        return exame == null ? NotFound() : Ok(exame);
    }

    [HttpPatch("exames/{id:int}")]
    public async Task<IActionResult> AtualizarExame(int id, AtualizaExameDto dto)
    {
        await _service.AtualizarExameAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("exames/{id:int}")]
    public async Task<IActionResult> ExcluirExame(int id)
    {
        await _service.ExcluirExameAsync(id);
        return NoContent();
    }
}
