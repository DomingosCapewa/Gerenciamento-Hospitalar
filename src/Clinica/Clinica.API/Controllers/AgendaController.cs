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
        [SwaggerOperation(Summary = "Checar disponibilidade", Description = "Retorna 200 se disponível, 409 se houver consulta para a especialidade/médico no horário.")]
        public async Task<IActionResult> Disponibilidade([FromQuery] string especialidade, [FromQuery] DateTime horario, [FromQuery] int? medicoId)
        {
            if (string.IsNullOrWhiteSpace(especialidade))
                return BadRequest(new { mensagem = "Especialidade é obrigatória." });

            var query = _db.ConsultasClinicas.AsNoTracking().Where(c => c.Horario == horario && c.Especialidade.ToLower() == especialidade.ToLower());
            if (medicoId.HasValue)
                query = query.Where(c => c.IdMedico == medicoId.Value);
            var ocupado = await query.AnyAsync();

            if (ocupado)
                return Conflict(new { mensagem = "Já existe consulta para esta especialidade no dia e hora informados." });

            return Ok(new { disponivel = true });
        }
    }
}