

namespace Clinica.Application.DTOs
{
    public class AtenderConsultaPorCodigo
    {
        public string PacienteCPF { get; set; } = string.Empty;
        public int? ConsultaId { get; set; }
        public List<string> Sintomas { get; set; } = new();
    }
}