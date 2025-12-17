using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Http.Json;



namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultasClinicasController : ControllerBase
    {
        private readonly IConsultaClinicaRepository _repo;
        private readonly IHttpClientFactory _httpClientFactory;

        public ConsultasClinicasController(IConsultaClinicaRepository repo, IHttpClientFactory httpClientFactory)
        {
            _repo = repo;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get consulta by ID", Description = "Retorna consulta clínica pelo ID.")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _repo.GetByIdAsync(id);
           return c == null ? NotFound(new { message = "Consulta não encontrada" }) : Ok(new { message = "Consulta encontrada", data = c });
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create consulta", Description = "Cria uma nova consulta clínica.")]
        public async Task<IActionResult> Create([FromBody] ConsultasClinicas consulta)
        {
            if (await _repo.HorarioOcupadoPorCpfAsync(consulta.CpfPaciente, consulta.Horario))
            {
                return Conflict(new { message = "Já existe uma consulta/exame para este CPF neste dia e hora." });
            }

            var created = await _repo.AddAsync(consulta);

            await NotifyClinica(created);

            var msg = $"O medico de {consulta.CpfPaciente} foi marcado para {consulta.Horario:dd/MM/yyyy HH:mm}";
            var codigo = $"O codigo de seu exame é {created.Id}";
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { mensagem = msg, codigo });
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update consulta", Description = "Atualiza uma consulta clínica.")]
        public async Task<IActionResult> Update(int id, [FromBody] ConsultasClinicas consulta)
        {
            if (id != consulta.Id) return BadRequest(new { message = "ID do recurso não coincide." });

            if (await _repo.HorarioOcupadoPorCpfAsync(consulta.CpfPaciente, consulta.Horario))
            {
                return Conflict(new { message = "Já existe uma consulta/exame para este CPF neste dia e hora." });
            }

            var updated = await _repo.UpdateAsync(consulta);
            await NotifyClinica(updated);

            var msg = $"O medico de {consulta.CpfPaciente} foi marcado para {consulta.Horario:dd/MM/yyyy HH:mm}";
            var codigo = $"O codigo de seu exame é {updated.Id}";
            return Ok(new { mensagem = msg, codigo });
        }

        private async Task NotifyClinica(ConsultasClinicas c)
        {
            var client = _httpClientFactory.CreateClient("ClinicaApi");
            var payload = new { id = c.Id, cpf = c.CpfPaciente, dataHora = c.Horario, medicoId = c.IdMedico };
            try
            {
                var response = await client.PostAsJsonAsync("/integrations/consultas", payload);
            }
            catch { } // To do: log failure
        }
    }
}