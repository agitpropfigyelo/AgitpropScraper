using System.Net;
using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure.InMemory;

public class CookieStorage : ICookiesStorage
{
    private CookieContainer _cookieContainer = new();

    public Task AddAsync(CookieContainer cookieContainer)
    {
        _cookieContainer = cookieContainer;
        return Task.CompletedTask;
    }

    public Task<CookieContainer> GetAsync()
    {
        return Task.FromResult(_cookieContainer);
    }
}