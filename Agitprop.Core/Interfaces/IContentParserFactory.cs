using Agitprop.Infrastructure;

namespace Agitprop.Core.Interfaces;

public interface IContentParserFactory
{
    IContentParser GetContentParser(NewsSites siteIn);
}
