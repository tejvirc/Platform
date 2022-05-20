// ReSharper disable once CheckNamespace
namespace Aristocrat.Mgam.Client
{
    using System;

    /// <summary>
    ///     Extension methods to support Reactive library
    /// </summary>
    public static class ReactiveExtensions
    {
        /// <summary>
        ///     Disposes the subscription.
        /// </summary>
        /// <param name="subscription"></param>
        public static void Unsubscribe(this IDisposable subscription)
        {
            subscription?.Dispose();
        }
    }
}
