

namespace Clinica.Application.DTOs
{
    public class AtenderConsultaPorHorario
    {
        public string PacienteCPF { get; set; } = string.Empty;
        public DateTime? Horario { get; set; }
        public List<SintomaInputDto> Sintomas { get; set; } = new();
    }
}