using Agitprop.Infrastructure.Interfaces;

namespace Agitprop.Core.Interfaces;

public interface ILinkParserFactory
{
    ILinkParser GetLinkParser(NewsSites siteIn);
}
