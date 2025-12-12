using Agendamentos.Application.DTOs;
using Agendamentos.Application.Interfaces;
using Agendamentos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Shared.Contracts.Events;
using Agendamentos.API.HATEOAS;

[ApiController]
[Route("api/agendamentos")]
public class AgendamentosController : ControllerBase
{
    private readonly IAgendamentoRepository _repo;
    private readonly IRabbitPublisher _publisher;
    private HATEOAS HATEOAS;   

    public AgendamentosController(IAgendamentoRepository repo, IRabbitPublisher publisher)
    {
        _repo = repo;
        _publisher = publisher;
        HATEOAS = new HATEOAS();
        HATEOAS.AddLink("self", "GET", "/api/agendamentos/pesquisar/consultas/{id}");
        HATEOAS.AddLink("update", "PATCH", "/api/agendamentos/alterar/consultas/{id}");
        HATEOAS.AddLink("delete", "DELETE", "/api/agendamentos/deletar/consultas/{id}");
    }

    

    [HttpPost("cadastro/consulta")]
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

        var paciente = new Paciente { Nome = dto.Paciente.Nome, CPF = dto.Paciente.CPF, Idade = dto.Paciente.Idade, Sexo = dto.Paciente.Sexo };
        await _repo.EnsurePacienteAsync(paciente);

        var consulta = new Consulta { PacienteCpf = dto.Paciente.CPF, DataHora = dto.Horario, MedicoEspecialidade = dto.Medico };
        var criado = await _repo.AddConsultaAsync(consulta);

        var evento = new ConsultaCriadaEvent
        {
            IdAgendamento = criado.Id,
            PacienteCpf = criado.PacienteCpf,
            DataHora = criado.DataHora,
            MedicoEspecialidade = criado.MedicoEspecialidade
        };
        _publisher.Publish("clinica-consultas", evento);

        return Ok(new { mensagem = $"O medico {criado.MedicoEspecialidade} de {dto.Paciente.Nome} foi marcado para {criado.DataHora:O}", codigo = criado.Id });
    }

    [HttpGet("pesquisar/consultas/{pacienteCPF}")]
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
    [HttpPatch("alterar/consultas/{pacienteId}")]
    [SwaggerOperation(Summary = "Atualizar detalhes da consulta", Description = "Atualiza os detalhes de uma consulta existente pelo pacienteId.")]
    public IActionResult AtualizarConsulta(int pacienteId, [FromBody] object detalhesAtualizados)
    {
        return Ok(new { mensagem = $"Consulta com ID {pacienteId} atualizada com sucesso." });
    }
    [HttpDelete("deletar/consultas/{pacienteId}")]
    [SwaggerOperation(Summary = "Excluir uma consulta", Description = "Exclui uma consulta existente pelo pacienteId.")]
    public IActionResult ExcluirConsulta(int pacienteId)
    {
        return Ok(new { mensagem = $"Consulta com ID {pacienteId} excluída com sucesso." });
    }

    [HttpPost("cadastro/exame")]
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

        var paciente = new Paciente { Nome = dto.Paciente.Nome, CPF = dto.Paciente.CPF, Idade = dto.Paciente.Idade, Sexo = dto.Paciente.Sexo };
        await _repo.EnsurePacienteAsync(paciente);

        var exame = new Exame { PacienteCpf = dto.Paciente.CPF, DataHora = dto.Horario, TipoExame = dto.Exame };
        var criado = await _repo.AddExameAsync(exame);

        var eventoExame = new ExameCriadaEvent
        {
            IdAgendamento = criado.Id,
            PacienteCpf = criado.PacienteCpf,
            DataHora = criado.DataHora,
            TipoExame = criado.TipoExame
        };
        _publisher.Publish("cirurgico-exames", eventoExame);

        return Ok(new { mensagem = $"O exame {criado.TipoExame} de {dto.Paciente.Nome} foi marcado para {criado.DataHora:O}", codigo = criado.Id });
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

    [HttpPatch("alterar/exames/{pacienteId}")]
    [SwaggerOperation(Summary = "Atualizar detalhes do exame", Description = "Atualiza os detalhes de um exame existente pelo pacienteId.")]
    public IActionResult AtualizarExame(int pacienteId, [FromBody] object detalhesAtualizados)
    {
        return Ok(new { mensagem = $"Exame com ID {pacienteId} atualizado com sucesso." });
    }

    [HttpDelete("deletar/exames/{pacienteId}")]
    [SwaggerOperation(Summary = "Excluir um exame", Description = "Exclui um exame existente pelo pacienteId.")]
    public IActionResult ExcluirExame(int pacienteId)
    {
        return Ok(new { mensagem = $"Exame com ID {pacienteId} excluído com sucesso." });
    }

    
}
