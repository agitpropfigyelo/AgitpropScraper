using PuppeteerSharp;

namespace Agitprop.Infrastructure.Interfaces
{
    public interface IBrowserAction
    {
        Task ExecuteAsync(IPage page);

    }
}
