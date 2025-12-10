

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendamentos.API.Controllers
{
    [ApiController]
    [Route("api/cadastro/")]

    public class AgendamentosController : ControllerBase
    {
        [HttpPost("consulta")]
        [SwaggerOperation(Summary = "Create a new agendamento", Description = "Creates a new agendamento in the system.")]
        public IActionResult consulta()
        {
            return Created("", "Agendamento created successfully.");
        }

        [HttpPost("exame")]
        [SwaggerOperation(Summary = "Create a new exame agendamento", Description = "Creates a new exame agendamento in the system.")]
        public IActionResult exame()
        {
            return Created("", "Exame agendamento created successfully.");
        }

        
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve all agendamentos", Description = "Gets a list of all agendamentos in the system.")]
        public IActionResult GetAllAgendamentos()
        {
            return Ok("Agendamentos retrieved successfully.");
        }

    }
}