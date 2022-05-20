namespace Aristocrat.Monaco.Bingo.Common
{
    using System;

    /// <summary>
    ///     Dispatcher interface to allow mocking of the Dispatcher in unit tests.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        ///     Executes the specified <see cref="T:System.Action" /> synchronously on
        ///     the thread the <see cref="T:System.Windows.Threading.Dispatcher" />
        ///     is associated with.
        /// </summary>
        /// <param name="action">A delegate to invoke through the dispatcher.</param>
        void Invoke(Action action);

        /// <summary>
        ///     Executes the specified action asynchronously on the
        ///     thread the <see cref="T:System.Windows.Threading.Dispatcher" />
        ///     is associated with.
        /// </summary>
        /// <param name="action">
        ///     The action to execute, which is pushed onto the
        ///     <see cref="T:System.Windows.Threading.Dispatcher" /> event queue.
        /// </param>
        void BeginInvoke(Action action);

        /// <summary>
        ///     Checks if we're already on the UI thread
        /// </summary>
        /// <returns>true if we're on the UI thread, false otherwise</returns>
        bool CheckAccess();
    }
}
