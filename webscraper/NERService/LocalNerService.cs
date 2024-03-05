
namespace webscraper;

public class LocalNerService : INerService
{
    private readonly Uri uri=new("localhost");
    
    public Dictionary<string, List<string>> GetNamedEntities(Article articleIn)
    {
        throw new NotImplementedException();
    }
}
