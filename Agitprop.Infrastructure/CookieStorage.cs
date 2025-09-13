using System.Net;

using Agitprop.Core.Interfaces;

namespace Agitprop.Infrastructure;

/// <summary>
/// Provides storage for managing cookies using a <see cref="CookieContainer"/>.
/// </summary>
public class CookieStorage : ICookiesStorage
{
    // Internal cookie container for storing cookies.
    private CookieContainer _cookieContainer = new();

    /// <summary>
    /// Adds a new cookie container to the storage.
    /// </summary>
    /// <param name="cookieContainer">The cookie container to add.</param>
    /// <returns>A completed task.</returns>
    public Task AddAsync(CookieContainer cookieContainer)
    {
        _cookieContainer = cookieContainer;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the current cookie container from the storage.
    /// </summary>
    /// <returns>The current <see cref="CookieContainer"/>.</returns>
    public Task<CookieContainer> GetAsync()
    {
        return Task.FromResult(_cookieContainer);
    }
}
