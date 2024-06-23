using System.Net;

namespace Agitprop.Infrastructure;

public interface IProxyProvider
{
    Task<IWebProxy> GetProxyAsync();
}
