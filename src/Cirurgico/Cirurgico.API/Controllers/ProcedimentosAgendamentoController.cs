using Cirurgico.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Cirurgico.API.Controllers
{
    [ApiController]
    [Route("api/procedimentos")] 
    public class ProcedimentosAgendamentoController : ControllerBase
    {
        private readonly CirurgicoDbContext _db;
        public ProcedimentosAgendamentoController(CirurgicoDbContext db) { _db = db; }

        public class MarcarProcedimentoRequest
        {
            public string PacienteCPF { get; set; } = string.Empty;
            public int CodigoExame { get; set; }
            public DateTime HorarioDesejado { get; set; }
        }

        [HttpPost("marcar")]
        [SwaggerOperation(Summary = "Marcar horário de procedimento/exame", Description = "Atualiza o horário do procedimento identificado pelo código do exame, retornando 200 ou 409 em caso de conflito.")]
        public async Task<IActionResult> Marcar([FromBody] MarcarProcedimentoRequest body)
        {
            if (string.IsNullOrWhiteSpace(body.PacienteCPF))
                return BadRequest(new { mensagem = "CPF do paciente é obrigatório." });
            if (body.CodigoExame <= 0)
                return BadRequest(new { mensagem = "Código do exame é obrigatório." });
            if (body.HorarioDesejado == default)
                return BadRequest(new { mensagem = "Horário desejado é obrigatório." });

            var procedimento = await _db.Exames.FirstOrDefaultAsync(e => e.IdExameAgendamento == body.CodigoExame);
            if (procedimento == null)
                return NotFound(new { mensagem = "Exame não encontrado. Registre o procedimento primeiro.", codigoExame = body.CodigoExame });

            var ocupado = await _db.Exames.AsNoTracking().AnyAsync(e => e.Horario == body.HorarioDesejado && e.TipoProcedimento.ToLower() == procedimento.TipoProcedimento.ToLower());
            if (ocupado)
                return Conflict(new { mensagem = "Horário indisponível para este tipo de exame.", codigoExame = body.CodigoExame });

            procedimento.Horario = body.HorarioDesejado;
            procedimento.CpfPaciente = body.PacienteCPF;
            procedimento.Status = "Agendado";
            await _db.SaveChangesAsync();

            return Ok(new { mensagem = "Exame marcado com sucesso.", codigoExame = body.CodigoExame });
        }
    }
}