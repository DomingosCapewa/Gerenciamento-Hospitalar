using System.Net;
using System.Net.Http.Json;
using Agendamentos.Application.Interfaces;

namespace Agendamentos.API.Services;

public class AvailabilityHttpService : IAvailabilityService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public AvailabilityHttpService(IHttpClientFactory httpClientFactory) { _httpClientFactory = httpClientFactory; }

    public async Task<(bool Available, object? ConflictPayload)> IsClinicaSpecialtyAvailableAsync(string especialidade, DateTime horario)
    {
        var client = _httpClientFactory.CreateClient("ClinicaApi");
        var url = $"/api/clinica/agenda/disponibilidade?especialidade={Uri.EscapeDataString(especialidade)}&horario={Uri.EscapeDataString(horario.ToString("o"))}";
        try
        {
            var resp = await client.GetAsync(url);
            if (resp.StatusCode == HttpStatusCode.Conflict)
                return (false, await SafeReadAsync(resp));
        }
        catch { }
        return (true, null);
    }

    public async Task<(bool Available, object? ConflictPayload)> IsCirurgicoExamTypeAvailableAsync(string tipoExame, DateTime horario)
    {
        var client = _httpClientFactory.CreateClient("CirurgicoApi");
        var url = $"/api/cirurgico/procedimentos/disponibilidade?tipo={Uri.EscapeDataString(tipoExame)}&horario={Uri.EscapeDataString(horario.ToString("o"))}";
        try
        {
            var resp = await client.GetAsync(url);
            if (resp.StatusCode == HttpStatusCode.Conflict)
                return (false, await SafeReadAsync(resp));
        }
        catch { }
        return (true, null);
    }

    private static async Task<object?> SafeReadAsync(HttpResponseMessage resp)
    {
        try { return await resp.Content.ReadFromJsonAsync<object>(); }
        catch { return null; }
    }
}
