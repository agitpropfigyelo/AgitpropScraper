using Agitprop.Core;
using Agitprop.Core.Interfaces;
using Agitprop.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public class AgitpropSink : ISink
{

    INamedEntityRecognizer NerService;
    IAgitpropDataBaseService DataBase;
    ILogger Logger;

    public AgitpropSink(INamedEntityRecognizer nerService, IAgitpropDataBaseService dataBase, ILogger logger)
    {
        NerService = nerService;
        DataBase = dataBase;
        Logger = logger;
    }

    public async Task EmitAsync(string url, List<ContentParserResult> data, CancellationToken cancellationToken = default)
    {
        //var tasks=data.Select
        foreach (var article in data)
        {
            var entities = await NerService.AnalyzeSingleAsync(article.Text);
            await DataBase.CreateMentionsAsync(url, article, entities);
        }
    }
}
