using Clinica.Application.DTOs;
using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Clinica.API.Controllers
{
    [ApiController]
    [Route("api/clinica/consultas")]
    [Authorize(Policy = "Medico")]
    public class AtendimentoController : ControllerBase
    {
        private readonly IConsultaClinicaRepository _consultasRepo;
        private readonly ISintomaRepository _sintomasRepo;
        private readonly IHttpClientFactory _httpClientFactory;

        public AtendimentoController(IConsultaClinicaRepository consultasRepo, ISintomaRepository sintomasRepo, IHttpClientFactory httpClientFactory)
        {
            _consultasRepo = consultasRepo;
            _sintomasRepo = sintomasRepo;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("atender-horario")]
        [SwaggerOperation(Summary = "Atender por horário", Description = "Localiza a consulta por CPF e horário e registra o atendimento.")]
        public async Task<IActionResult> AtenderConsultaPorHorario([FromBody] AtenderConsultaPorHorario body)
        {
            if (string.IsNullOrWhiteSpace(body.PacienteCPF))
                return BadRequest(new { mensagem = "CPF do paciente é obrigatório." });
            if (!body.Horario.HasValue)
                return BadRequest(new { mensagem = "Horário da consulta é obrigatório." });
            if (body.Sintomas == null || body.Sintomas.Count == 0)
                return BadRequest(new { mensagem = "Informe ao menos um sintoma por nome." });

            var consulta = await _consultasRepo.GetByCpfAndHorarioAsync(body.PacienteCPF, body.Horario.Value);
            if (consulta == null)
                return NotFound(new { mensagem = "Consulta não encontrada para CPF e horário informados." });
            var sintomasConsulta = new List<ConsultaSintoma>();

            foreach (var nome in body.Sintomas)
            {
                var baseSintomaPorNome = await _sintomasRepo.GetByNomeAsync(nome);
                if (baseSintomaPorNome == null)
                    return BadRequest(new { mensagem = $"Sintoma com nome='{nome}' não encontrado." });
                var mapa = await _sintomasRepo.GetMapaPorSintomaIdAsync(baseSintomaPorNome.Id);
                if (mapa == null)
                    return BadRequest(new { mensagem = $"Sintoma nome='{nome}' não possui mapeamento de doenças." });
                sintomasConsulta.Add(new ConsultaSintoma
                {
                    ConsultaClinicaId = consulta.Id,
                    SintomaId = baseSintomaPorNome.Id,
                    Sintoma = baseSintomaPorNome,
                    Prioridade = baseSintomaPorNome.PrioridadePadrao,
                    Observacao = string.Empty
                });
            }

            consulta.Sintomas = sintomasConsulta;
      

            var atualizado = await _consultasRepo.AtenderConsulta(consulta);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var links = new Shared.Contracts.Hateoas.HateoasBuilder()
                .Add("self","GET","/api/clinica/consultas/{id}")
                .Add("atender-codigo","POST","/api/clinica/consultas/atender-codigo")
                .Add("sintomas","GET","/api/clinica/sintomas")
                .Build(baseUrl, new Dictionary<string,string>{{"id", atualizado.Id.ToString()}});

            var resposta = new
            {
                idConsultaClinica = atualizado.Id,
                idConsultaAgendamento = atualizado.IdConsultaAgendamento,
                cpfPaciente = atualizado.CpfPaciente,
                idMedico = atualizado.IdMedico,
                horario = atualizado.Horario,
                sintomas = atualizado.Sintomas.Select(x => new { x.SintomaId, nomeSintoma = x.Sintoma.Nome, prioridade = x.Prioridade, observacao = x.Observacao }),
                diagnostico = atualizado.Diagnostico,
                tratamentoSugerido = atualizado.TratamentoSugerido,
                _links = links
            };

            return Ok(resposta);
        }

        [HttpPost("atender-codigo")]
        [SwaggerOperation(Summary = "Atender consulta pelo código", Description = "Retorna possíveis doenças e exames adicionais.")]
        public async Task<IActionResult> AtenderConsultaPorCodigo([FromBody] AtenderConsultaPorCodigo body)
        {
            if (string.IsNullOrWhiteSpace(body.PacienteCPF))
                return BadRequest(new { mensagem = "CPF do paciente é obrigatório." });
            if (!body.ConsultaId.HasValue)
                return BadRequest(new { mensagem = "Código da consulta é obrigatório." });
            if (body.Sintomas == null || body.Sintomas.Count == 0)
                return BadRequest(new { mensagem = "Informe ao menos um sintoma por nome." });

            var consulta = await _consultasRepo.GetByCpfAndIdConsultaAsync(body.PacienteCPF, body.ConsultaId.Value);
            if (consulta == null)
                return NotFound(new { mensagem = "Consulta não encontrada para o CPF e código informados." });
            var resolved = new List<int>();
            foreach (var nome in body.Sintomas)
            {
                var s = await _sintomasRepo.GetByNomeAsync(nome);
                if (s == null)
                    return BadRequest(new { mensagem = $"Sintoma com nome='{nome}' não encontrado." });
                resolved.Add(s.Id);
            }
            IEnumerable<int> idsSintomas = resolved;
            var doencasSugeridas = (await _sintomasRepo.SugerirDoencasPorSintomasAsync(idsSintomas)).ToList();

            var examesNecessarios = new List<string>();
            foreach (var d in doencasSugeridas)
            {
                var nome = d.Nome.ToLowerInvariant();
                if (nome.Contains("hemorragia") || nome.Contains("fratura"))
                    examesNecessarios.Add("Tomografia Computadorizada");
                if (nome.Contains("enxaqueca") || nome.Contains("fraturas internas"))
                    examesNecessarios.Add("Ressonância Magnética");
                if (nome.Contains("covid") || nome.Contains("gripe"))
                    examesNecessarios.Add("Painel Respiratório/Laboratório");
            }
            examesNecessarios = examesNecessarios.Distinct().ToList();

            var protocolosExames = new List<object>();
            if (examesNecessarios.Count > 0)
            {
                var clientCirurgico = _httpClientFactory.CreateClient("CirurgicoApi");
                foreach (var exame in examesNecessarios)
                {
                    var payload = new { consultaId = consulta.Id, cpf = consulta.CpfPaciente, exame };
                    try
                    {
                        var resp = await clientCirurgico.PostAsJsonAsync("/exames/solicitacoes", payload);
                        var protocolo = resp.IsSuccessStatusCode ? (object)new { exame, protocolo = await resp.Content.ReadAsStringAsync() } : new { exame, protocolo = "pendente" };
                        protocolosExames.Add(protocolo);
                    }
                    catch
                    {
                        protocolosExames.Add(new { exame, protocolo = "erro-integracao" });
                    }
                }
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var links = new Shared.Contracts.Hateoas.HateoasBuilder()
                .Add("self","GET","/api/clinica/consultas/{id}")
                .Add("atender-horario","POST","/api/clinica/consultas/atender-horario")
                .Add("procedimentos-status","PUT","/api/procedimentos/{exameId}/status")
                .Add("procedimentos-prioridade","PUT","/api/procedimentos/{exameId}/prioridade")
                .Build(baseUrl, new Dictionary<string,string>{{"id", consulta.Id.ToString()}});

            return Ok(new
            {
                consulta = new { consulta.Id, consulta.CpfPaciente, consulta.Horario, consulta.IdMedico },
                doencasSugeridas = doencasSugeridas.Select(d => new { d.Id, d.Nome }),
                examesSolicitados = protocolosExames,
                _links = links
            });
        }
}
}