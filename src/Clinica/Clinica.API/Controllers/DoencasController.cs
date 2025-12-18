using Clinica.Domain.Entities;
using Clinica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/doencas")]
    public class DoencasController : ControllerBase
    {
        private readonly ClinicaDbContext _db;
        public DoencasController(ClinicaDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Listar doenças", Description = "Retorna todas as doenças cadastradas.")]
        public async Task<IActionResult> GetAll()
        {
            var itens = await _db.Doencas.AsNoTracking().ToListAsync();
            return Ok(itens);
        }

        [HttpGet("com-sintomas")]
        [SwaggerOperation(Summary = "Doenças com sintomas", Description = "Retorna todas as doenças com seus sintomas associados.")]
        public async Task<IActionResult> GetAllWithSymptoms()
        {
            var itens = await _db.Doencas
                .AsNoTracking()
                .Select(d => new
                {
                    id = d.Id,
                    nome = d.Nome,
                    descricao = d.Descricao,
                    sintomas = _db.SintomaDoencas
                        .Where(sd => sd.DoencaId == d.Id)
                        .Select(sd => sd.Sintoma)
                        .Select(s => new
                        {
                            id = s.Id,
                            nome = s.Nome,
                            prioridadePadrao = s.PrioridadePadrao
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(itens);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var d = await _db.Doencas.FindAsync(id);
            return d == null ? NotFound() : Ok(d);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Criar doença", Description = "Cria uma nova doença.")]
        public async Task<IActionResult> Create([FromBody] Doenca d)
        {
            _db.Doencas.Add(d);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = d.Id }, d);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Atualizar doença", Description = "Atualiza uma doença existente.")]
        public async Task<IActionResult> Update(int id, [FromBody] Doenca d)
        {
            if (id != d.Id) return BadRequest(new { mensagem = "ID não confere." });
            _db.Doencas.Update(d);
            await _db.SaveChangesAsync();
            return Ok(d);
        }
    }
}
