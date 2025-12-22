namespace Shared.Contracts.Hateoas;
public class LinkDto
{
    public string Href { get; set; } = string.Empty;
    public string Rel { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public LinkDto() {}
    public LinkDto(string href, string rel, string method) { Href = href; Rel = rel; Method = method; }
}
