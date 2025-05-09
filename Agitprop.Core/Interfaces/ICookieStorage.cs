using System.Net;

namespace Agitprop.Core.Interfaces;

/// <summary>
/// Defines the contract for managing cookie storage.
/// </summary>
public interface ICookiesStorage
{
    /// <summary>
    /// Adds a new cookie container to the storage.
    /// </summary>
    /// <param name="cookieCollection">The cookie container to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddAsync(CookieContainer cookieCollection);

    /// <summary>
    /// Retrieves the current cookie container from the storage.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current <see cref="CookieContainer"/>.</returns>
    Task<CookieContainer> GetAsync();
}
