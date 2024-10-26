using System;

namespace Agitprop.Core.Interfaces;

public interface IFailedJobLogger : IDisposable
{
    Task LogFailedJobUrlAsync(string url);
}
