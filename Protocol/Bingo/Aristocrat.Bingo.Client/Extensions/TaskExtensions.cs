namespace Aristocrat.Bingo.Client.Extensions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        public static void RunAndForget(this Task task, Func<AggregateException, Task> onError = null)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.ContinueWith(
                async t =>
                {
                    if (onError is not null)
                    {
                        await onError(t.Exception).ConfigureAwait(false);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Current);
        }
    }
}