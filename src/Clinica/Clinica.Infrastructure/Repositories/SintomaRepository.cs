using Clinica.Application.Interfaces;
using Clinica.Domain.Entities;
using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Repositories
{
    public class SintomaRepository(ClinicaDbContext db) : ISintomaRepository
    {
        private readonly ClinicaDbContext _db = db;

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        public async Task<Sintoma?> GetByIdAsync(int id)
        {
            return await _db.Sintomas.FindAsync(id);
        }

        public async Task<Sintoma?> GetByNomeAsync(string nome)
        {
            var target = NormalizeName(nome).ToLowerInvariant();
            var itens = await _db.Sintomas.AsNoTracking().ToListAsync();
            return itens.FirstOrDefault(s => NormalizeName(s.Nome).ToLowerInvariant() == target);
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
            return await _db.SintomaDoencas.AsNoTracking().FirstOrDefaultAsync(x => x.SintomaId == sintomaId);
        }

        public async Task<SintomaDoenca> AddOrUpdateMapaAsync(SintomaDoenca mapa)
        {
            var existente = await _db.SintomaDoencas.FirstOrDefaultAsync(x => x.SintomaId == mapa.SintomaId);
            if (existente == null)
            {
                _db.SintomaDoencas.Add(mapa);
            }
            else
            {
                existente.SintomaId = mapa.SintomaId;
                existente.DoencaId = mapa.DoencaId;
                _db.SintomaDoencas.Update(existente);
            }
            await _db.SaveChangesAsync();
            return existente ?? mapa;
        }
        
        public async Task<SintomaDoenca> AssociarSintomaDoencaAsync(int sintomaId, int doencaId)
        {
            var sintomaExiste = await _db.Sintomas.AnyAsync(s => s.Id == sintomaId);
            var doencaExiste = await _db.Doencas.AnyAsync(d => d.Id == doencaId);

            if (!sintomaExiste || !doencaExiste)
                throw new InvalidOperationException("Sintoma ou doença não encontrada.");

            var existente = await _db.SintomaDoencas
                .FirstOrDefaultAsync(sd => sd.SintomaId == sintomaId && sd.DoencaId == doencaId);

            if (existente != null)
                return existente;

            var novo = new SintomaDoenca
            {
                SintomaId = sintomaId,
                DoencaId = doencaId
            };

            _db.SintomaDoencas.Add(novo);
            await _db.SaveChangesAsync();

            // Carrega navegações para retorno completo (opcional)
            await _db.Entry(novo).Reference(sd => sd.Sintoma).LoadAsync();
            await _db.Entry(novo).Reference(sd => sd.Doenca).LoadAsync();

            return novo;
        }

        public async Task<IEnumerable<Doenca>> SugerirDoencasPorSintomasAsync(IEnumerable<int> sintomaIds)
        {
            var ids = sintomaIds.Distinct().ToList();
            if (ids.Count == 0) return Enumerable.Empty<Doenca>();

            var doencas = await _db.SintomaDoencas
                .Where(sd => ids.Contains(sd.SintomaId))
                .Include(sd => sd.Doenca)
                .Select(sd => sd.Doenca)
                .Distinct()
                .AsNoTracking()
                .ToListAsync();

            return doencas;
        }
    }
}
