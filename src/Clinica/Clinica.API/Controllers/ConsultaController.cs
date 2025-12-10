using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicaController : ControllerBase
    {
        [HttpGet]
        [SwaggerOperation(Summary = "Check API status", Description = "Returns a message indicating that the Clinica API is running.")]
        public IActionResult Get()
        {
            return Ok("Clinica API is running!");
        }
    }
}