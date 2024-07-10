namespace Agitprop.Core;

using Polly;
using Polly.Retry;

public static class Executor
{
    private static AsyncRetryPolicy AsyncPolicy { get; } = Polly.Policy.Handle<Exception>().WaitAndRetryAsync(6, i => TimeSpan.FromSeconds(i * 1));
    private static RetryPolicy Policy { get; } = Polly.Policy.Handle<Exception>().Retry(3);

    public static async Task<T> RetryAsync<T>(Func<Task<T>> func) => await AsyncPolicy.ExecuteAsync(func);

    public static void Retry<T>(Action action) => Policy.Execute(action);
}