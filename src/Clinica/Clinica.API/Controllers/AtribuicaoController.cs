using Clinica.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Shared.Contracts.Messaging;
using Shared.Contracts.Common;
using Shared.Contracts.Events;
using System.Linq;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/consultas")]
    [Authorize(Policy = "Medico")]
    public class AtribuicaoController : ControllerBase
    {
        private readonly IConsultaClinicaRepository _consultasRepo;
        private readonly IMedicoRepository _medicoRepo;
        private readonly IRabbitPublisher _publisher;

        public AtribuicaoController(IConsultaClinicaRepository consultasRepo, IMedicoRepository medicoRepo, IRabbitPublisher publisher)
        {
            _consultasRepo = consultasRepo;
            _medicoRepo = medicoRepo;
            _publisher = publisher;
        }

        [HttpPost("{consultaId:int}/check-in")]
        [SwaggerOperation(Summary = "Check-in da consulta", Description = "Atribui automaticamente um médico disponível pela especialidade da consulta.")]
        public async Task<IActionResult> CheckInConsulta(int consultaId)
        {
            var consulta = await _consultasRepo.GetByIdAsync(consultaId);
            if (consulta == null)
                return NotFound(new { mensagem = "Consulta não encontrada." });

            if (consulta.IdMedico.HasValue)
                return Conflict(new { mensagem = "Consulta já possui médico atribuído." });

            var medicos = await _medicoRepo.GetAllMedicosAsync();
            var medico = medicos.FirstOrDefault(m => string.Equals(m.Especialidade, consulta.Especialidade, StringComparison.OrdinalIgnoreCase));
            if (medico == null)
                return NotFound(new { mensagem = $"Nenhum médico encontrado para a especialidade '{consulta.Especialidade}'." });

            var ocupado = await _consultasRepo.HorarioOcupadoAsync(medico.IdMedico, consulta.Horario);
            if (ocupado)
                return Conflict(new { mensagem = "Médico indisponível neste horário." });

            consulta.IdMedico = medico.IdMedico;
            await _consultasRepo.UpdateAsync(consulta);

            var evento = new ConsultaAtribuidaMedicoEvent
            {
                IdConsultaClinica = consulta.Id,
                IdConsultaAgendamento = consulta.IdConsultaAgendamento,
                CpfPaciente = consulta.CpfPaciente,
                Horario = consulta.Horario,
                IdMedico = medico.IdMedico,
                Especialidade = consulta.Especialidade
            };
            _publisher.Publish(QueueNames.ClinicaConsultasAtribuidas, evento);

            return Ok(new { mensagem = $"Médico '{medico.Nome}' atribuído à consulta {consulta.Id}.", idMedico = medico.IdMedico });
        }
    }
}
