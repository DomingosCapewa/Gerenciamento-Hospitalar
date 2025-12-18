using Clinica.Application.Interfaces;
using Clinica.Application.DTOs;
using Clinica.Domain.Entities;
using Clinica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/sintomas")]
    public class SintomasController : ControllerBase
    {
        private readonly ISintomaRepository _repo;
        private readonly ClinicaDbContext _db;
        public SintomasController(ISintomaRepository repo, ClinicaDbContext db)
        {
            _repo = repo;
            _db = db;
        }
        [HttpGet]
        [SwaggerOperation(Summary = "Listar sintomas", Description = "Retorna todos os sintomas cadastrados.")]
        public async Task<IActionResult> GetAll()
        {
            var itens = await _repo.GetAllAsync();
            return Ok(itens);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Criar sintoma", Description = "Cria um novo sintoma.")]
        public async Task<IActionResult> Create([FromBody] Sintoma s)
        {
            var created = await _repo.AddAsync(s);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _repo.GetByIdAsync(id);
            return s == null ? NotFound() : Ok(s);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Atualizar sintoma", Description = "Atualiza um sintoma existente.")]
        public async Task<IActionResult> Update(int id, [FromBody] Sintoma s)
        {
            if (id != s.Id) return BadRequest(new { mensagem = "ID não confere." });
            var updated = await _repo.UpdateAsync(s);
            return Ok(updated);
        }
       
        [HttpPut("{sintomaId:int}/mapa")]
        [SwaggerOperation(Summary = "Atualizar mapa de doenças por sintoma", Description = "Atualiza ou cria o mapeamento de doenças associadas e nível de prioridade do sintoma.")]
        public async Task<IActionResult> UpsertMapa(int sintomaId, [FromBody] SintomaDoenca mapa)
        {
            if (sintomaId != mapa.SintomaId) return BadRequest(new { mensagem = "SintomaId do body difere do path." });
            var saved = await _repo.AddOrUpdateMapaAsync(mapa);
            return Ok(saved);
        }

        [HttpGet("{sintomaId:int}/mapa")]
        public async Task<IActionResult> GetMapa(int sintomaId)
        {
            var mapa = await _repo.GetMapaPorSintomaIdAsync(sintomaId);
            return mapa == null ? NotFound() : Ok(mapa);
        }

        [HttpPost("{sintomaId:int}/associar/{doencaId:int}")]
        [SwaggerOperation(Summary = "Associar sintoma a doença", Description = "Cria a associação entre um sintoma e uma doença.")]
        public async Task<IActionResult> Associar(int sintomaId, int doencaId)
        {
            var link = await _repo.AssociarSintomaDoencaAsync(sintomaId, doencaId);
            return Ok(link);
        }

        [HttpPost("associar-por-nome")]
        [SwaggerOperation(Summary = "Associar por nomes", Description = "Associa um sintoma e uma doença usando seus nomes.")]
        public async Task<IActionResult> AssociarPorNome([FromBody] AssociarSintomaDoencaPorNomeDto body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.NomeSintoma) || string.IsNullOrWhiteSpace(body.NomeDoenca))
                return BadRequest(new { mensagem = "Informe NomeSintoma e NomeDoenca." });

            var sintoma = await _repo.GetByNomeAsync(body.NomeSintoma);
            if (sintoma == null)
                return NotFound(new { mensagem = $"Sintoma '{body.NomeSintoma}' não encontrado." });
            static string Normalize(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return string.Empty;
                var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
                var sb = new System.Text.StringBuilder(normalized.Length);
                foreach (var ch in normalized)
                {
                    var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                        sb.Append(ch);
                }
                return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
            }

            var alvo = Normalize(body.NomeDoenca).ToLowerInvariant();
            var doencas = await _db.Doencas.AsNoTracking().ToListAsync();
            var doenca = doencas.FirstOrDefault(d => Normalize(d.Nome).ToLowerInvariant() == alvo);
            if (doenca == null)
                return NotFound(new { mensagem = $"Doença '{body.NomeDoenca}' não encontrada." });

            var link = await _repo.AssociarSintomaDoencaAsync(sintoma.Id, doenca.Id);
            return Ok(link);
        }
    }
}
