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

    [HttpPost("consultas")]
    [Authorize(Policy = "Usuario")]
    public async Task<IActionResult> CriarConsulta([FromBody] CadastroConsultaDto dto)
    {
        var result = await _service.CriarConsultaAsync(dto);
        return CreatedAtAction(nameof(ObterConsultaPorId), new { id = result.Id }, result);
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


    [HttpPost("exames")]
    public async Task<IActionResult> CriarExame([FromBody] CadastroExameDto dto)
    {
        var result = await _service.CriarExameAsync(dto);
        return CreatedAtAction(nameof(ObterExamePorId), new { id = result.Id }, result);
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
