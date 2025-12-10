using Agendamentos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IPacienteRepository _repo;
    public PacientesController(IPacienteRepository repo) => _repo = repo;

    [HttpPost]
    [SwaggerOperation(Summary = "Create a new paciente", Description = "Adds a new paciente to the system.")]
    public async Task<IActionResult> Create([FromBody] Paciente p) => Ok(new { message = "Paciente created successfully", paciente = await _repo.AddAsync(p) });

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get a paciente by ID", Description = "Retrieves a paciente by their unique ID.")]
    public async Task<IActionResult> Get(int id)
    {
        var paciente = await _repo.GetByIdAsync(id);
        return paciente is not null ? Ok(paciente) : NotFound();
    }
}
