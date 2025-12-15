using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories
{
    public class SintomaRepository : ISintomaRepository
    {
        private readonly ClinicaDbContext _db;

        public SintomaRepository(ClinicaDbContext db)
        {
            _db = db;
        }

        public async Task<Sintoma?> GetByIdAsync(int id)
        {
            return await _db.Sintomas.FindAsync(id);
        }

        public async Task<IEnumerable<Sintoma>> GetAllAsync()
        {
            return await _db.Sintomas.AsNoTracking().ToListAsync();
        }

        public async Task<Sintoma> AddAsync(Sintoma s)
        {
            _db.Sintomas.Add(s);
            await _db.SaveChangesAsync();
            return s;
        }

        public async Task<Sintoma> UpdateAsync(Sintoma s)
        {
            _db.Sintomas.Update(s);
            await _db.SaveChangesAsync();
            return s;
        }

        public async Task<SintomaDoenca?> GetMapaPorSintomaIdAsync(int sintomaId)
        {
            return await _db.SintomaDoencas.AsNoTracking().FirstOrDefaultAsync(x => x.SintomaDoencaId == sintomaId);
        }

        public async Task<SintomaDoenca> AddOrUpdateMapaAsync(SintomaDoenca mapa)
        {
            var existente = await _db.SintomaDoencas.FirstOrDefaultAsync(x => x.SintomaDoencaId == mapa.SintomaDoencaId);
            if (existente == null)
            {
                _db.SintomaDoencas.Add(mapa);
            }
            else
            {
                existente.NomeSintoma = mapa.NomeSintoma;
                existente.DoencasAssociadas = mapa.DoencasAssociadas;
                existente.NivelPrioridade = mapa.NivelPrioridade;
                _db.SintomaDoencas.Update(existente);
            }
            await _db.SaveChangesAsync();
            return existente ?? mapa;
        }
    }
}
