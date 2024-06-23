using PuppeteerSharp;

namespace Agitprop.Infrastructure;

public interface IBrowserAction
{
    Task ExecuteAsync(IPage page);

}
