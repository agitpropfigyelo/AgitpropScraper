namespace webscraper;

public class ProgressHandler : ProgressBar, IProgress<int>
{
    private int num;
    private int counter = 0;
    public ProgressHandler(int numIn) : base()
    {
        num = numIn;
    }


    public void Report(int value)
    {
        Interlocked.Increment(ref counter);
        double currentProgress = (double)counter / num;
        base.Report(currentProgress);
    }
}
