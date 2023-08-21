namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A <see cref="WaitHandle"/> extensions helper class.
    /// </summary>
    public static class WaitHandleExtensions
    {
        /// <summary>
        ///     Converts a wait object to a task.
        /// </summary>
        /// <param name="handle">Wait object to register.</param>
        /// <param name="cancellation">Propagates the notification that operations should be canceled.</param>
        /// <returns>The wait handle as a task</returns>
        public static Task<bool> AsTask(this WaitHandle handle, CancellationToken cancellation)
        {
            return handle.AsTask(Timeout.InfiniteTimeSpan, cancellation);
        }

        /// <summary>
        ///     Converts a wait object to a task.
        /// </summary>
        /// <param name="handle">Wait object to register.</param>
        /// <param name="timeout">The timeout for wait handle</param>
        /// <param name="cancellation">Propagates the notification that operations should be canceled.</param>
        /// <returns>The wait handle as a task</returns>
        public static Task<bool> AsTask(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellation)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            var tcs = new TaskCompletionSource<bool>(cancellation);

            var waitRegistration = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),
                tcs,
                timeout,
                true);

            var tokenRegistration = cancellation.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs);

            tcs.Task.ContinueWith(
                (task, state) =>
                {
                    waitRegistration.Unregister(null);
                    tokenRegistration.Dispose();
                }, TaskScheduler.Default, CancellationToken.None);

            return tcs.Task;
        }
    }
}
