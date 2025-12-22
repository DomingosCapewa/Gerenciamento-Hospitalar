using Clinica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/agenda")] 
    public class AgendaController : ControllerBase
    {
        private readonly ClinicaDbContext _db;
        public AgendaController(ClinicaDbContext db) { _db = db; }

        [HttpGet("disponibilidade")] 
        [SwaggerOperation(Summary = "Checar disponibilidade por especialidade", Description = "Retorna 200 se disponível, 409 se houver consulta marcada para a especialidade no horário.")]
        public async Task<IActionResult> Disponibilidade([FromQuery] string especialidade, [FromQuery] DateTime horario)
        {
            if (string.IsNullOrWhiteSpace(especialidade))
                return BadRequest(new { mensagem = "Especialidade é obrigatória." });

            var ocupado = await _db.ConsultasClinicas
                .AsNoTracking()
                .AnyAsync(c => c.Horario == horario && c.Especialidade.ToLower() == especialidade.ToLower());

            if (ocupado)
                return Conflict(new { mensagem = "Já existe consulta para esta especialidade no dia e hora informados." });

            return Ok(new { disponivel = true });
        }
    }
}