using System;

namespace Agitprop.Core.Interfaces;

public interface IProgressReporter : IDisposable
{
    public void ReportNewJobsScheduled(int newJobCount);
    public void ReportJobStarted(string jobUrl);
    public void ReportJobSuccess(string jobUrl);
    public void ReportJobFailed(string jobUrl);
    public void ReportJobSkipped(string jobUrl);
    public void ReportJobProgress(string jobUrl, string status);

}

