

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Agendamentos.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AgendamentosController : ControllerBase
    {
        [HttpGet]
        [SwaggerOperation(Summary = "Retrieve all agendamentos", Description = "Gets a list of all agendamentos in the system.")]
        public IActionResult GetAllAgendamentos()
        {
            return Ok("Agendamentos retrieved successfully.");
        }
    }
}