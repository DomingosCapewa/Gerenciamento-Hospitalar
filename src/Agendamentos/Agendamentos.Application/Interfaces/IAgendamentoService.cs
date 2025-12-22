using Agendamentos.Application.DTOs;
using Agendamentos.Domain.Entities;

namespace Agendamentos.Application.Interfaces;

public interface IAgendamentoService
{
    Task<Consulta> CriarConsultaAsync(CadastroConsultaDto dto);
    Task<Consulta?> ObterConsultaPorIdAsync(int id);
    Task<IEnumerable<Consulta>> ObterConsultasPorCpfAsync(string cpf);
    Task AtualizarConsultaAsync(int id, AtualizaConsultaDto dto);
    Task ExcluirConsultaAsync(int id);

    Task<Exame> CriarExameAsync(CadastroExameDto dto);
    Task<Exame?> ObterExamePorIdAsync(int id);
    Task AtualizarExameAsync(int id, AtualizaExameDto dto);
    Task ExcluirExameAsync(int id);
}