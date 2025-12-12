

using Agendamentos.Domain.Entities;

namespace Agendamentos.API.HATEOAS
{
     public class Link
    {
        public string href { get; set; }
        public string rel { get; set; }
        public string method { get; set; }


        public Link(string href, string rel, string method)
        {
            this.href = href;
            this.rel = rel;
            this.method = method;

        }
    }

     public class AgendamentoContainer
    {
        public Consulta? Consulta { get; set; }
        public Exame? Exame { get; set; }
        public Link[] Links { get; set; }
    }
}