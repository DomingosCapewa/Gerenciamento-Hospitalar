using System.Collections.Generic;

namespace Shared.Contracts.Hateoas;

public class HateoasBuilder
{
    private class ActionTemplate
    {
        public string Rel { get; }
        public string Method { get; }
        public string PathTemplate { get; }
        public ActionTemplate(string rel, string method, string pathTemplate)
        {
            Rel = rel; Method = method; PathTemplate = pathTemplate;
        }
    }

    private readonly List<ActionTemplate> _actions = new();

    public HateoasBuilder Add(string rel, string method, string pathTemplate)
    {
        _actions.Add(new ActionTemplate(rel, method, pathTemplate));
        return this;
    }

    public LinkDto[] Build(string baseUrl, IDictionary<string,string>? tokens = null)
    {
        var links = new List<LinkDto>(_actions.Count);
        foreach (var a in _actions)
        {
            var path = a.PathTemplate;
            if (tokens != null)
            {
                foreach (var kv in tokens)
                {
                    path = path.Replace("{" + kv.Key + "}", kv.Value ?? string.Empty);
                }
            }
            var href = (baseUrl ?? string.Empty).TrimEnd('/') + "/" + path.TrimStart('/');
            links.Add(new LinkDto(href, a.Rel, a.Method));
        }
        return links.ToArray();
    }
}
