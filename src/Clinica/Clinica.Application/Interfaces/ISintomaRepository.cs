using Clinica.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clinica.Application.Interfaces
{
    public interface ISintomaRepository
    {
        Task<Sintoma?> GetByIdAsync(int id);
        Task<Sintoma?> GetByNomeAsync(string nome);
        Task<IEnumerable<Sintoma>> GetAllAsync();
        Task<Sintoma> AddAsync(Sintoma s);
        Task<Sintoma> UpdateAsync(Sintoma s);

        Task<SintomaDoenca?> GetMapaPorSintomaIdAsync(int sintomaId);
        Task<SintomaDoenca> AddOrUpdateMapaAsync(SintomaDoenca mapa);
        Task<SintomaDoenca> AssociarSintomaDoencaAsync(int sintomaId, int doencaId);
        Task<IEnumerable<Doenca>> SugerirDoencasPorSintomasAsync(IEnumerable<int> sintomaIds);
    }
}
