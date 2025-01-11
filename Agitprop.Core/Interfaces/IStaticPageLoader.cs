namespace Agitprop.Infrastructure.Interfaces;

public interface IStaticPageLoader
{
    Task<string> Load(string url);
}
