namespace Agitprop.Core.Interfaces;

public interface IBrowserPageLoader
{
    Task<string> Load(string url, object pageActions, bool headless);
}
