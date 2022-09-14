namespace Aristocrat.Monaco.Bingo.Common
{
    using System;
    using System.Threading.Tasks;
    using Monaco.Common;

    /// <summary>
    ///     A container for Dispatcher extension methods
    /// </summary>
    public static class DispatcherExtensions
    {
        /// <summary>
        ///     Safely dispatch an action on the UI thread
        /// </summary>
        /// <param name="dispatcher">
        ///     Dispatcher that executes the specified method
        /// </param>
        /// <param name="action">The action to run</param>
        public static void ExecuteOnUIThread(this IDispatcher dispatcher, Action action)
        {
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action).FireAndForget();
            }
        }

        /// <summary>
        ///     Safely dispatch an action on the UI thread
        /// </summary>
        /// <param name="dispatcher">
        ///     Dispatcher that executes the specified method
        /// </param>
        /// <param name="action">The action to run</param>
        public static async Task ExecuteAndWaitOnUIThread(this IDispatcher dispatcher, Action action)
        {
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                await dispatcher.BeginInvoke(action).ConfigureAwait(false);
            }
        }
    }
}