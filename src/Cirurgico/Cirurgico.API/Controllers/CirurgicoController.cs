

using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Cirurgico.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CirurgicoController : ControllerBase
    {
        [HttpGet]
        [SwaggerOperation(Summary = "Check API status", Description = "Returns a message indicating that the Cirurgico API is running.")]
        public IActionResult Get()
        {
            return Ok("Cirurgico API is running!");
        }
    }
}