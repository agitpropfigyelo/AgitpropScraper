using System.Net;

namespace Agitprop.Infrastructure;

public interface ICookiesStorage
{
    Task AddAsync(CookieContainer cookieCollection);
    Task<CookieContainer> GetAsync();
}
