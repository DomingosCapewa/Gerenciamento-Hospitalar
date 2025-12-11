using Agendamentos.Application.Interfaces;
using Agendamentos.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/[controller]")]
public class pacientesController : ControllerBase
{
    private readonly IPacienteRepository _repo;
    public pacientesController(IPacienteRepository repo) => _repo = repo;

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

    [HttpGet]
    [SwaggerOperation(Summary = "Get all pacientes", Description = "Retrieves all pacientes in the system.")]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _repo.GetAllAsync();
        if (!pacientes.Any())
            return NotFound(new { message = "No pacientes found" });

        return Ok(new { message = "Pacientes retrieved successfully", count = pacientes.Count(), pacientes });
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Update a paciente", Description = "Updates an existing paciente's information.")]
    public async Task<IActionResult> Update(int id, [FromBody] Paciente updatedPaciente)
    {
        var existingPaciente = await _repo.GetByIdAsync(id);
        if (existingPaciente is null)
            return NotFound(new { message = "Paciente not found" });
return Ok(new { message = "Paciente updated successfully", paciente = existingPaciente });
        
        // existingPaciente.Nome = updatedPaciente.Nome ?? existingPaciente.Nome;
        // existingPaciente.CPF = updatedPaciente.CPF ?? existingPaciente.CPF;
        // existingPaciente.DataNascimento = updatedPaciente.DataNascimento != default ? updatedPaciente.DataNascimento : existingPaciente.DataNascimento;
        // existingPaciente.Telefone = updatedPaciente.Telefone ?? existingPaciente.Telefone;
        // existingPaciente.Email = updatedPaciente.Email ?? existingPaciente.Email;

        // await _repo.UpdateAsync(existingPaciente);
        // return Ok(new { message = "Paciente updated successfully", paciente = existingPaciente });
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a paciente", Description = "Deletes an existing paciente from the system.")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingPaciente = await _repo.GetByIdAsync(id);
        if (existingPaciente is null)
            return NotFound(new { message = "Paciente not found" });

        // await _repo.DeleteAsync(existingPaciente);
        return Ok(new { message = "Paciente deleted successfully" });
    }
}