using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Cirurgico.API.Controllers
{
    [ApiController]
    [Route("api/exames")] 
    public class ExamesController : ControllerBase
    {
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
        public IActionResult Solicitar([FromBody] SolicitarExameDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Cpf) || string.IsNullOrWhiteSpace(dto.Exame))
                return BadRequest(new { mensagem = "Informe CPF e tipo de exame." });

            var protocolo = $"PRT-{Guid.NewGuid():N}";

            var resposta = new
            {
                protocolo,
                exame = dto.Exame,
                cpf = dto.Cpf,
                consultaId = dto.ConsultaId,
                status = "Recebido",
                observacao = dto.Observacao ?? string.Empty
            };

            return Ok(resposta);
        }
    }
}