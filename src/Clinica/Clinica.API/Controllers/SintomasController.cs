using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/sintomas")]
    public class SintomasController : ControllerBase
    {
        private readonly ISintomaRepository _repo;
        public SintomasController(ISintomaRepository repo)
        {
            _repo = repo;
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
            if (sintomaId != mapa.SintomaDoencaId) return BadRequest(new { mensagem = "SintomaId do body difere do path." });
            var saved = await _repo.AddOrUpdateMapaAsync(mapa);
            return Ok(saved);
        }

        [HttpGet("{sintomaId:int}/mapa")]
        public async Task<IActionResult> GetMapa(int sintomaId)
        {
            var mapa = await _repo.GetMapaPorSintomaIdAsync(sintomaId);
            return mapa == null ? NotFound() : Ok(mapa);
        }
    }
}
