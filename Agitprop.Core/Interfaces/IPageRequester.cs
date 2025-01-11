using System.Net;

namespace Agitprop.Core.Interfaces;

public interface IPageRequester
{
    public CookieContainer CookieContainer { get; set; }
    Task<HttpResponseMessage> GetAsync(string url);
}
