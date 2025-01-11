using PuppeteerSharp;

namespace Agitprop.Core.Interfaces
{
    public interface IBrowserAction
    {
        Task ExecuteAsync(IPage page);

    }
}
