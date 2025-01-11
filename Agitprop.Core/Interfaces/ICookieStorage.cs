using System.Net;

namespace Agitprop.Core.Interfaces;

public interface ICookiesStorage
{
    Task AddAsync(CookieContainer cookieCollection);
    Task<CookieContainer> GetAsync();
}
