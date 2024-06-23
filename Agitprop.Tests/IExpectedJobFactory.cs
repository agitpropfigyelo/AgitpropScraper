using Agitprop.Core;
using Agitprop.Infrastructure;

namespace NewsArticleScraper.Tests;

public interface IExpectedJobFactory
{
    ScrapingJob GetPaginationJob(NewsSites source);
}
