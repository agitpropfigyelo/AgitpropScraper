using Agitprop.Core;
using Agitprop.Core.Enums;

namespace NewsArticleScraper.Tests;

public interface IExpectedJobFactory
{
    ScrapingJob GetPaginationJob(NewsSites source);
}
