using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Core.Factories
{
    public interface IContentParserFactory
    {
        IContentParser GetContentParser(NewsSites siteIn);
    }
}
