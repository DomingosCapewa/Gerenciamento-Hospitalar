using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicosController : ControllerBase
    {
        private readonly IMedicoRepository _medicoRepository;

        public MedicosController(IMedicoRepository medicoRepository)
        {
            _medicoRepository = medicoRepository;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all medicos", Description = "Retrieves a list of all medicos.")]
        public async Task<IActionResult> GetAllMedicos()
        {
            var medicos = await _medicoRepository.GetAllMedicosAsync();
            return Ok(new{ message = "Medicos retrieved successfully", data = medicos });
        }
        
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get a medico by ID", Description = "Retrieves a medico by their unique ID.")]
        public async Task<IActionResult> GetMedicoById(int id)
        {
            var medico = await _medicoRepository.GetMedicoByIdAsync(id);
            if (medico == null)
            {
                return NotFound();
            }

            return Ok(new{ message = "Medico retrieved successfully", data = medico });
        }
        
        [HttpPost]
        [SwaggerOperation(Summary = "Add a new medico", Description = "Creates a new medico in the system.")]
        public async Task<IActionResult> AddMedico([FromBody] Medico medico)
        {
            var createdMedico = await _medicoRepository.AddMedicoAsync(medico);
            return CreatedAtAction(nameof(GetMedicoById), new { id = createdMedico.Id }, new { message = "Medico created successfully", data = createdMedico });
        }
    }
}