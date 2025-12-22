using Cirurgico.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Cirurgico.API.Controllers
{
    [ApiController]
    [Route("api/cirurgico/procedimentos")] 
    public class DisponibilidadeController : ControllerBase
    {
        private readonly CirurgicoDbContext _db;
        public DisponibilidadeController(CirurgicoDbContext db) { _db = db; }

        [HttpGet("disponibilidade")] 
        [SwaggerOperation(Summary = "Checar disponibilidade por tipo de procedimento", Description = "Retorna 200 se disponível, 409 se houver procedimento/exame marcado no horário para o mesmo tipo.")]
        public async Task<IActionResult> Disponibilidade([FromQuery] string tipo, [FromQuery] DateTime horario)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                return BadRequest(new { mensagem = "Tipo de procedimento é obrigatório." });

            var ocupado = await _db.Exames
                .AsNoTracking()
                .AnyAsync(e => e.Horario == horario && e.TipoProcedimento.ToLower() == tipo.ToLower());

            if (ocupado)
                return Conflict(new { mensagem = "Já existe procedimento/exame deste tipo no dia e hora informados." });

            return Ok(new { disponivel = true });
        }
    }
}