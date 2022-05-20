namespace Aristocrat.Monaco.Bingo.Common
{
    using System;

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
                dispatcher.BeginInvoke(action);
            }
        }
    }
}