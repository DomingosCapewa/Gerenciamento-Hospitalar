

using Clinica.Domain.Entities;

namespace Clinica.Application.DTOs
{

        public class AtenderConsultaRequestDto
        {
            public string PacienteCPF { get; set; } = string.Empty;
            public DateTime? DataHora { get; set; }      
            public int? ConsultaId { get; set; }
            public List<SintomaInputDto> Sintomas { get; set; } = new();
            public string Diagnostico { get; set; } = string.Empty;
            public string TratamentoSugerido { get; set; } = string.Empty;
        }

        public class SintomaInputDto
        {
            public int SintomaId { get; set; }   
            public PrioridadeSintoma? Prioridade { get; set; }
            public string? Observacao { get; set; } = string.Empty;
        }

    }
