namespace webscraper;

public class NerResponse
{
    public List<string> MISC { get; set; }
    public List<string> ORG { get; set; }
    public List<string> PER { get; set; }

    public override string ToString()
    {
        return $"\t{string.Join(", ", MISC)}\n\t{string.Join(", ", ORG)}\n\t{string.Join(", ", PER)}";
    }
}
