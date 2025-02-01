using System.CommandLine;

using Agitprop.ConsoleToolKit;

class Program
{

    public static async Task Main(string[] args)
    {
        // Define command-line options
        var rootCommand = new RootCommand();
        rootCommand.Description = "Agitprop Console ToolKit";

        // Add commands
        rootCommand.AddQueueCommand();
        rootCommand.AddScrapeCommand();

        await rootCommand.InvokeAsync(args);
    }


}
