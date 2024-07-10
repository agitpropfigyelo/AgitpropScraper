using Agitprop.Core.Enums;
using Agitprop.Core.Interfaces;

namespace Agitprop.Core.Factories;

public interface IPaginatorFactory
{
    IPaginator GetPaginator(NewsSites source);
}
