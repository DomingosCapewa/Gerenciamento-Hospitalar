

namespace Agendamentos.API.HATEOAS
{
    public class HATEOAS
    {
        private class ActionTemplate
        {
            public string Rel { get; }
            public string Method { get; }
            public string PathTemplate { get; }

            public ActionTemplate(string rel, string method, string pathTemplate)
            {
                Rel = rel;
                Method = method;
                PathTemplate = pathTemplate;
            }
        }

        private readonly List<ActionTemplate> _actions = new List<ActionTemplate>();

        public HATEOAS()
        {
        }

        public void AddLink(string rel, string method, string pathTemplate)
        {
            _actions.Add(new ActionTemplate(rel, method, pathTemplate));
        }

        public Link[] GetActions(string baseUrl, string id)
        {
            var links = new List<Link>(_actions.Count);
            foreach (var action in _actions)
            {
                var path = action.PathTemplate.Replace("{id}", id);
                var href = baseUrl.TrimEnd('/') + "/" + path.TrimStart('/');
                links.Add(new Link(href, action.Rel, action.Method));
            }
            return links.ToArray();
        }
    }
}