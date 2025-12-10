

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/")]
    public class ConsultasClinicasController : ControllerBase
    {
        [HttpGet("AtenderConsulta")]
        [SwaggerOperation(Summary = "Check API status", Description = "Returns a message indicating that the ConsultasClinicas API is running.")]
        public IActionResult Get()
        {
            return Ok("ConsultasClinicas API is running!");
        }
    }
}