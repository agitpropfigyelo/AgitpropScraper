namespace webscraper;

public class NerResponse
{
    public List<string>? MISC { get; set; }
    public List<string>? ORG { get; set; }
    public List<string>? PER { get; set; }

    public override string ToString()
    {
        string miscString= MISC is not null ? string.Join(", ", MISC) : "";
        string orgString= ORG is not null ? string.Join(", ", ORG) : "";
        string perString= PER is not null ? string.Join(", ", PER) : "";
        return $"MISC:\n\t{miscString}\nORG:\n\t{orgString}\nPER:\n\t{perString}";
    }
}
