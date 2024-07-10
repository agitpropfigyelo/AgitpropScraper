using System.Net;

namespace Agitprop.Core.Interfaces;

public interface IProxyProvider
{
    Task<IWebProxy> GetProxyAsync();
}
