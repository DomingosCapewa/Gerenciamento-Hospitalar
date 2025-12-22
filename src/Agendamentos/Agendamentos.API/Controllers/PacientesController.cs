using Agendamentos.Application.Interfaces;
using Agendamentos.Application.DTOs;
using Agendamentos.Application.Validators;
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
    public async Task<IActionResult> Create([FromBody] PacienteDto dto)
    {
        var entity = new Paciente
        {
            Nome = dto.Nome,
            CPF = dto.CPF,
            Idade = dto.Idade,
            Sexo = dto.Sexo,
            DataNascimento = dto.DataNascimento,
            Telefone = dto.Telefone,
            Email = dto.Email
        };

        var created = await _repo.AddAsync(entity);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

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
        return Ok(new { message = "Pacientes retrieved successfully", count = pacientes.Count(), pacientes });
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Update a paciente", Description = "Updates an existing paciente's information.")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePacienteDTO updatedPaciente)
    {
        var existingPaciente = await _repo.GetByIdAsync(id);
        if (existingPaciente is null)
            return NotFound(new { message = "Paciente not found" });
        existingPaciente.Nome = updatedPaciente.Nome ?? existingPaciente.Nome;
        existingPaciente.CPF = updatedPaciente.CPF ?? existingPaciente.CPF;
        existingPaciente.Idade = updatedPaciente.Idade ?? existingPaciente.Idade;
        existingPaciente.Sexo = updatedPaciente.Sexo ?? existingPaciente.Sexo;
        existingPaciente.DataNascimento = updatedPaciente.DataNascimento ?? existingPaciente.DataNascimento;
        existingPaciente.Telefone = updatedPaciente.Telefone ?? existingPaciente.Telefone;
        existingPaciente.Email = updatedPaciente.Email ?? existingPaciente.Email;

        await _repo.UpdateAsync(existingPaciente);
        return Ok(new { message = "Paciente updated successfully", paciente = existingPaciente });
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Delete a paciente", Description = "Deletes an existing paciente from the system.")]
    public async Task<IActionResult> Delete(int id)
    {
        var existingPaciente = await _repo.GetByIdAsync(id);
        if (existingPaciente is null)
            return NotFound(new { message = "Paciente not found" });

        await _repo.DeleteAsync(existingPaciente);
        return Ok(new { message = "Paciente deleted successfully" });
    }
}