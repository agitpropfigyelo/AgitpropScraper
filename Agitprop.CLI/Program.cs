﻿using System.CommandLine;
using Agitprop.CLI.Commands;

class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine($"Debug: Args received: [{string.Join(", ", args)}]");
        Console.WriteLine($"Debug: Args count: {args.Length}");
        
        var rootCommand = new RootCommand
        {
            Description = "Agitprop CLI Tool",
        };

        rootCommand.AddScrapeArticleCommand();
        rootCommand.AddScrapeArchiveCommand();

        await rootCommand.InvokeAsync(args);
    }
}