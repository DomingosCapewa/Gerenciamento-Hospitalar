using Clinica.Application.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/consultas")]
    public class AtendimentoController : ControllerBase
    {
        private readonly IClinicaRepository _clinicaRepo;
        private readonly IConsultaClinicaRepository _consultasRepo;
        private readonly ISintomaRepository _sintomasRepo;

        public AtendimentoController(IClinicaRepository clinicaRepo, IConsultaClinicaRepository consultasRepo, ISintomaRepository sintomasRepo)
        {
            _clinicaRepo = clinicaRepo;
            _consultasRepo = consultasRepo;
            _sintomasRepo = sintomasRepo;
        }

        [HttpPost("{consultaId:int}/atender")]
        [SwaggerOperation(Summary = "Atender consulta", Description = "Registra sintomas, diagnóstico e tratamento para a consulta.")]
        public async Task<IActionResult> AtenderConsulta(int consultaId, [FromBody] AtenderConsultaRequestDto body)
        {
            var consulta = await _consultasRepo.GetByIdAsync(consultaId);
            if (consulta == null)
                return NotFound(new { mensagem = "Consulta não encontrada." });

            // Monta lista de sintomas da consulta
            var sintomasConsulta = new List<ConsultaSintoma>();
            foreach (var s in body.Sintomas)
            {
                var baseSintoma = await _sintomasRepo.GetByIdAsync(s.SintomaId);
                if (baseSintoma == null)
                    return BadRequest(new { mensagem = $"SintomaId={s.SintomaId} inválido." });

                var prioridade = s.Prioridade ?? baseSintoma.PrioridadePadrao;
                // Se houver mapa admin para o sintoma, usa o nível configurado
                var mapa = await _sintomasRepo.GetMapaPorSintomaIdAsync(baseSintoma.Id);
                if (mapa != null)
                {
                    var nivel = (PrioridadeSintoma)Math.Clamp(mapa.NivelPrioridade, 1, 4);
                    prioridade = nivel;
                }
                sintomasConsulta.Add(new ConsultaSintoma
                {
                    ConsultaClinicaId = consulta.Id,
                    SintomaId = baseSintoma.Id,
                    Sintoma = baseSintoma,
                    Prioridade = prioridade,
                    Observacao = s.Observacao
                });
            }

            consulta.Sintomas = sintomasConsulta;
            consulta.Diagnostico = body.Diagnostico;
            consulta.TratamentoSugerido = body.TratamentoSugerido;

            var atualizado = await _clinicaRepo.AtenderConsulta(consulta);
            var resposta = new
            {
                idConsultaClinica = atualizado.Id,
                idConsultaAgendamento = atualizado.IdConsultaAgendamento,
                cpfPaciente = atualizado.CpfPaciente,
                idMedico = atualizado.IdMedico,
                horario = atualizado.Horario,
                sintomas = atualizado.Sintomas.Select(x => new { x.SintomaId, nomeSintoma = x.Sintoma.Nome, prioridade = x.Prioridade, observacao = x.Observacao }),
                diagnostico = atualizado.Diagnostico,
                tratamentoSugerido = atualizado.TratamentoSugerido
            };

            return Ok(resposta);
        }
    }
}
