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

            // Sugestão de diagnóstico e tratamento com base nas doenças associadas aos sintomas
            var idsSintomas = sintomasConsulta.Select(s => s.SintomaId).Distinct().ToList();
            var doencasSugeridas = (await _sintomasRepo.SugerirDoencasPorSintomasAsync(idsSintomas)).ToList();
            if (doencasSugeridas.Count > 0)
            {
                consulta.Diagnostico = "Possíveis doenças: " + string.Join(", ", doencasSugeridas.Select(d => d.Nome));
                consulta.TratamentoSugerido = SugerirTratamento(doencasSugeridas.Select(d => d.Nome));
            }
      

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

            consulta.Diagnostico = "Possíveis doenças: " + string.Join(", ", doencasSugeridas.Select(d => d.Nome));
            consulta.TratamentoSugerido = SugerirTratamento(doencasSugeridas.Select(d => d.Nome));
            await _consultasRepo.UpdateAsync(consulta);

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
                        string codigo;
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            try
                            {
                                var doc = System.Text.Json.JsonDocument.Parse(json);
                                codigo = doc.RootElement.TryGetProperty("protocolo", out var prop) ? prop.GetString() ?? json : json;
                            }
                            catch { codigo = json; }
                        }
                        else
                        {
                            codigo = "pendente";
                        }
                        var protocolo = new { mensagem = $"Exame registrado para o CPF: {consulta.CpfPaciente}. Agende o horário.", codigo, tipo = exame };
                        protocolosExames.Add(protocolo);
                    }
                    catch
                    {
                        protocolosExames.Add(new { mensagem = "Falha ao integrar com centro cirúrgico", codigo = "erro-integracao", tipo = exame });
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

        // Alias compatível com o desafio: POST /api/clinica/AtenderConsulta
        [HttpPost("~/api/clinica/AtenderConsulta")]
        [SwaggerOperation(Summary = "Atender consulta (alias)", Description = "Aceita atendimento por CPF+horário ou por código.")]
        public async Task<IActionResult> AtenderConsultaAlias([FromBody] AtenderConsultaRequestDto body)
        {
            if (string.IsNullOrWhiteSpace(body.PacienteCPF))
                return BadRequest(new { mensagem = "CPF do paciente é obrigatório." });

            ConsultasClinicas? consulta = null;
            if (body.ConsultaId.HasValue)
            {
                consulta = await _consultasRepo.GetByCpfAndIdConsultaAsync(body.PacienteCPF, body.ConsultaId.Value);
                if (consulta == null) return NotFound(new { mensagem = "Consulta não encontrada pelo código informado." });
                if (body.DataHora.HasValue)
                {
                    consulta.Horario = body.DataHora.Value;
                }
            }
            else if (body.DataHora.HasValue)
            {
                consulta = await _consultasRepo.GetByCpfAndHorarioAsync(body.PacienteCPF, body.DataHora.Value);
                if (consulta == null) return NotFound(new { mensagem = "Consulta não encontrada pelo horário informado." });
            }
            else
            {
                return BadRequest(new { mensagem = "Informe 'ConsultaId' ou 'DataHora'." });
            }

            var sintomasConsulta = new List<ConsultaSintoma>();
            if (body.Sintomas != null && body.Sintomas.Count > 0)
            {
                foreach (var s in body.Sintomas)
                {
                    var baseSintoma = await _sintomasRepo.GetByIdAsync(s.SintomaId);
                    if (baseSintoma == null) return BadRequest(new { mensagem = $"SintomaId={s.SintomaId} não encontrado." });
                    sintomasConsulta.Add(new ConsultaSintoma
                    {
                        ConsultaClinicaId = consulta!.Id,
                        SintomaId = baseSintoma.Id,
                        Sintoma = baseSintoma,
                        Prioridade = s.Prioridade ?? baseSintoma.PrioridadePadrao,
                        Observacao = s.Observacao ?? string.Empty
                    });
                }
            }
            else if (body.SintomasNomes != null && body.SintomasNomes.Count > 0)
            {
                foreach (var nome in body.SintomasNomes)
                {
                    var baseSintoma = await _sintomasRepo.GetByNomeAsync(nome);
                    if (baseSintoma == null) return BadRequest(new { mensagem = $"Sintoma '{nome}' não encontrado." });
                    sintomasConsulta.Add(new ConsultaSintoma
                    {
                        ConsultaClinicaId = consulta!.Id,
                        SintomaId = baseSintoma.Id,
                        Sintoma = baseSintoma,
                        Prioridade = baseSintoma.PrioridadePadrao,
                        Observacao = string.Empty
                    });
                }
            }
            else
            {
                return BadRequest(new { mensagem = "Informe sintomas por IDs ou nomes." });
            }

            consulta!.Sintomas = sintomasConsulta;
            var ids = sintomasConsulta.Select(x => x.SintomaId);
            var doencas = (await _sintomasRepo.SugerirDoencasPorSintomasAsync(ids)).ToList();
            consulta.Diagnostico = "Possíveis doenças: " + string.Join(", ", doencas.Select(d => d.Nome));
            consulta.TratamentoSugerido = SugerirTratamento(doencas.Select(d => d.Nome));
            await _consultasRepo.UpdateAsync(consulta);

            var examesNecessarios = DeterminarExames(doencas.Select(d => d.Nome));
            var resultados = new List<object>();
            if (examesNecessarios.Count > 0)
            {
                var clientCirurgico = _httpClientFactory.CreateClient("CirurgicoApi");
                foreach (var exame in examesNecessarios)
                {
                    var payload = new { consultaId = consulta.Id, cpf = consulta.CpfPaciente, exame };
                    try
                    {
                        var resp = await clientCirurgico.PostAsJsonAsync("/exames/solicitacoes", payload);
                        string codigo;
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            try
                            {
                                var doc = System.Text.Json.JsonDocument.Parse(json);
                                codigo = doc.RootElement.TryGetProperty("protocolo", out var prop) ? prop.GetString() ?? json : json;
                            }
                            catch { codigo = json; }
                        }
                        else
                        {
                            codigo = "pendente";
                        }
                        resultados.Add(new { mensagem = $"Exame registrado sobre o CPF: {consulta.CpfPaciente}. Por favor agendar o horário.", codigo, tipo = exame });
                    }
                    catch
                    {
                        resultados.Add(new { mensagem = "Falha ao integrar com centro cirúrgico", codigo = "erro-integracao", tipo = exame });
                    }
                }
            }

            return Ok(new
            {
                mensagem = "Consulta atendida com sucesso",
                codigo = consulta.Id,
                doencasSugeridas = doencas.Select(d => new { d.Id, d.Nome }),
                examesSolicitados = resultados
            });
        }

        private static string SugerirTratamento(IEnumerable<string> nomesDoencas)
        {
            var set = nomesDoencas.Select(n => n.ToLowerInvariant()).ToHashSet();
            if (set.Contains("covid") || set.Contains("gripe"))
                return "Repouso, hidratação, antitérmicos; avaliar painel respiratório.";
            if (set.Contains("hemorragia") || set.Contains("fratura") || set.Contains("fraturas internas"))
                return "Imobilização e encaminhamento para imagem (Tomografia/Ressonância).";
            if (set.Contains("enxaqueca"))
                return "Analgésicos, repouso em ambiente silencioso; avaliar imagem se persistente.";
            return "Acompanhar sintomas e realizar exames básicos.";
        }

        private static List<string> DeterminarExames(IEnumerable<string> nomesDoencas)
        {
            var examesNecessarios = new List<string>();
            foreach (var nome in nomesDoencas.Select(n => n.ToLowerInvariant()))
            {
                if (nome.Contains("hemorragia") || nome.Contains("fratura")) examesNecessarios.Add("Tomografia Computadorizada");
                if (nome.Contains("fraturas internas") || nome.Contains("enxaqueca")) examesNecessarios.Add("Ressonância Magnética");
                if (nome.Contains("covid") || nome.Contains("gripe")) examesNecessarios.Add("Painel Respiratório/Laboratório");
            }
            return examesNecessarios.Distinct().ToList();
        }
}
}