namespace Aristocrat.Monaco.Hhr.Client.Extensions
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    ///     Extension function for Reactive Subjects
    /// </summary>
    public static class SubscribeExtensions
    {
        /// <summary>
        ///     Subscribe with async action
        /// </summary>
        /// <param name="source">Observable</param>
        /// <param name="asyncAction">Async action to perform OnNext</param>
        /// <param name="handler">Optional handler to call.</param>
        /// <typeparam name="T">Type of subscription</typeparam>
        /// <returns></returns>
        public static IDisposable SubscribeAsync<T>(
            this IObservable<T> source,
            Func<T, Task> asyncAction,
            Action<Exception> handler = null)
        {
            async Task<Unit> Wrapped(T t)
            {
                await asyncAction(t);
                return Unit.Default;
            }

            return handler == null
                ? source.SelectMany(Wrapped).Subscribe(_ => { })
                : source.SelectMany(Wrapped).Subscribe(_ => { }, handler);
        }
    }
}