namespace Agitprop.Infrastructure.ProxyService
{
    public record ProxyInfo(
        string Address,            // e.g. "http://45.12.30.181:80" or "socks5://..."
        DateTimeOffset LastValidated,
        int SuccessCount,
        int FailCount,
        bool IsAlive
    )
    {
        public double Score => SuccessCount - FailCount; // simple heuristic
    }
}