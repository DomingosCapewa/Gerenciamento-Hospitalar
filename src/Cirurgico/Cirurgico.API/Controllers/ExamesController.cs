using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;
using Cirurgico.Infrastructure.Persistence;
using Cirurgico.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cirurgico.API.Controllers
{
    [ApiController]
    [Route("api/exames")] 
    public class ExamesController : ControllerBase
    {
        private readonly CirurgicoDbContext _db;
        public ExamesController(CirurgicoDbContext db)
        {
            _db = db;
        }
        public class SolicitarExameDto
        {
            public int ConsultaId { get; set; }
            public string Cpf { get; set; } = string.Empty;
            public string Exame { get; set; } = string.Empty;
            public string? Observacao { get; set; }
        }

        [HttpPost("solicitacoes")]
        [Authorize]
        [SwaggerOperation(Summary = "Solicitar exame", Description = "Recebe a solicitação de exame e retorna um protocolo para agendamento posterior.")]
        public async Task<IActionResult> Solicitar([FromBody] SolicitarExameDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cpf) || string.IsNullOrWhiteSpace(dto.Exame))
                return BadRequest(new { mensagem = "Informe CPF e tipo de exame." });

            var entity = new ProcedimentoExame
            {
                CpfPaciente = dto.Cpf,
                TipoProcedimento = dto.Exame,
                Status = "Recebido",
                Horario = default,
            };

            _db.Exames.Add(entity);
            await _db.SaveChangesAsync();

            entity.IdExameAgendamento = entity.Id;
            await _db.SaveChangesAsync();

            var resposta = new
            {
                mensagem = $"Exame registrado sobre o CPF: {dto.Cpf}. Por favor agendar o horário em nosso sistema.",
                codigo = entity.IdExameAgendamento,
                exame = dto.Exame,
                cpf = dto.Cpf,
                consultaId = dto.ConsultaId,
                status = entity.Status,
                observacao = dto.Observacao ?? string.Empty
            };

            return Ok(resposta);
        }
    }
}