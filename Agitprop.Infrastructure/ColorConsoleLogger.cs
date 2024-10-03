using Agitprop.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public sealed class ColorConsoleLogger : ILogger, IDisposable
{
    private Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; } = new()
    {
        [LogLevel.Trace] = ConsoleColor.DarkGray,
        [LogLevel.Debug] = ConsoleColor.Gray,
        [LogLevel.Information] = ConsoleColor.Green,
        [LogLevel.Warning] = ConsoleColor.Cyan,
        [LogLevel.Error] = ConsoleColor.Red,
        [LogLevel.Critical] = ConsoleColor.Red,
        [LogLevel.None] = ConsoleColor.Gray
    };

    public IDisposable BeginScope<TState>(TState state)
    {
        return default!;
    }

    public void Dispose()
    {
        Console.ForegroundColor = ConsoleColor.White;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = LogLevelToColorMap[logLevel];
        Console.WriteLine($"[{DateTime.Now:u}][{logLevel}] {formatter(state, exception)}");

        if (exception != null) Console.WriteLine($"{Environment.NewLine}{exception}");

        Console.ForegroundColor = originalColor;
    }
}

public class FileLogger : ILogger
{
    private string filePath;
    private static object _lock = new object();
    private IConfiguration configuration;

    public FileLogger(IConfiguration configuration)
    {
        this.configuration = configuration;
        this.filePath = configuration["LogFile"] ?? throw new MissingConfigurationValueException("LogFile path missing from config");
    }

    public FileLogger(string path)
    {
        filePath = path;
    }
    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        //return logLevel == LogLevel.Trace;
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter != null)
        {
            lock (_lock)
            {
                string fullFilePath = Path.Combine(filePath, DateTime.Now.ToString("yyyy-MM-dd") + "_log.log");
                if (!File.Exists(fullFilePath))
                {
                    File.Create(fullFilePath);
                }
                File.AppendAllText(fullFilePath, $"{Environment.NewLine}[{DateTime.Now:u}][{logLevel}] {formatter(state, exception)}");
                if (exception != null) File.AppendAllText(fullFilePath, $"{Environment.NewLine}{exception}");
            }
        }
    }
}
