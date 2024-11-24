using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Useful asynchronous extensions.
/// </summary>
public static class AsyncExtensions
{
    /// <summary>
    /// This extension method provides the ability to cancel tasks that have no cancellation facilities.
    /// </summary>
    /// <typeparam name="T">Task result</typeparam>
    /// <param name="task">Task to add cancellation to</param>
    /// <param name="cancellationToken">Task cancellation token</param>
    /// <returns>Task<T></returns>
    /// <exception cref="OperationCanceledException">Operation has been cancelled</exception>
    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>();
        using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
        {
            if (task != await Task.WhenAny(task, tcs.Task))
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        return task.Result;
    }
}