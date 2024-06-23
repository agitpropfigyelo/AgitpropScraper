using Agitprop.Infrastructure;

namespace Agitprop.Core.Interfaces;

public interface IPaginatorFactory
{
    IPaginator GetPaginator(NewsSites source);
}
