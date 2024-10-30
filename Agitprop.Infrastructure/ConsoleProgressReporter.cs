using System;
using System.Collections.Concurrent;
using System.Text;
using System.Timers;
using Agitprop.Core.Interfaces;
using Agitprop.Core.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agitprop.Infrastructure;

public class ConsoleProgressReporter : IProgressReporter
{
    private int totalJobs = 0;
    private int successfulJobs = 0;
    private int failedJobs = 0;
    private int skippedJobs = 0;
    private int allCompletedJobs = 0;
    private DateTime startTime = DateTime.Now;
    private ConcurrentDictionary<string, string> currentlyRunningJobs = new();
    private readonly ILogger<ConsoleProgressReporter> logger;
    private readonly System.Timers.Timer progressTimer;

    public ConsoleProgressReporter(ILogger<ConsoleProgressReporter> logger, IConfiguration configuration)
    {
        this.logger = logger;
        // Set up a timer to trigger every 3 seconds (3000 milliseconds)
        progressTimer = new System.Timers.Timer(TimeSpan.FromSeconds(1));
        progressTimer.Elapsed += (sender, e) => DisplayProgress();
        progressTimer.AutoReset = true;
        progressTimer.Start();
    }

    public void ReportNewJobsScheduled(int newJobCount)
    {
        Interlocked.Add(ref totalJobs, newJobCount);
    }

    public void ReportJobStarted(string jobUrl)
    {
        currentlyRunningJobs[jobUrl] = "Started";
    }

    public void ReportJobSuccess(string jobUrl)
    {
        Interlocked.Increment(ref successfulJobs);
        Interlocked.Increment(ref allCompletedJobs);
        currentlyRunningJobs.TryRemove(jobUrl, out _);
    }

    public void ReportJobFailed(string jobUrl)
    {
        Interlocked.Increment(ref failedJobs);
        Interlocked.Increment(ref allCompletedJobs);
        currentlyRunningJobs.TryRemove(jobUrl, out _);
    }

    public void ReportJobProgress(string jobUrl, string status)
    {
        currentlyRunningJobs[jobUrl] = status;
    }

    public void ReportJobSkipped(string jobUrl)
    {
        Interlocked.Increment(ref skippedJobs);
        Interlocked.Increment(ref allCompletedJobs);
        currentlyRunningJobs.TryRemove(jobUrl, out _);
    }


    public void Dispose()
    {
        progressTimer?.Dispose();
    }

    private void DisplayProgress()
    {
        logger.LogInformation("Running display update");
        var sb = new StringBuilder();
        sb.AppendLine($"Total Jobs: {totalJobs}/{allCompletedJobs} : Skipped: {skippedJobs} | Successful: {successfulJobs} | Failed: {failedJobs}");
        sb.AppendLine($"Started at: {startTime:yyyy-MM-dd HH:mm:ss} | {DateTime.Now - startTime}");
        sb.AppendLine("Currently Running Jobs:");
        foreach (var job in currentlyRunningJobs.OrderBy(x => x.Value))
        {
            sb.AppendLine($"- [{job.Value}]: {job.Key.Truncate(100)}...");
        }

        Console.Clear();
        Console.Write(sb.ToString());
    }

}
