using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Core.Factories;

public interface ILinkParserFactory
{
    ILinkParser GetLinkParser(NewsSites siteIn);
}
