namespace Aristocrat.Mgam.Client.Options
{
    using System;

    /// <summary>
    ///     Used for notifications when TOptions instances change.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IOptionsMonitor<out TOptions>
        where TOptions : class
    {
        /// <summary>
        ///     Gets the <typeparamref name="TOptions"/> instance.
        /// </summary>
        TOptions CurrentValue { get; }

        /// <summary>
        ///     Registers a listener to be called whenever a named <typeparamref name="TOptions"/> changes.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="filter">Notification filter.</param>
        /// <returns><see cref="IDisposable"/> which should be disposed to stop listening for changes.</returns>
        IDisposable OnChange(Action<TOptions, string> listener, Predicate<string> filter = null);
    }
}
