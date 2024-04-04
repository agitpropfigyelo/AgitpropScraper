namespace webscraper;

public interface IEntityDbService
{
    void WriteArticleToDb(Article articleIn);

    void CreateEntity(string entityIn);

    void CreateRelation(string sourceIn, string relation, string entityIdIn);
}
