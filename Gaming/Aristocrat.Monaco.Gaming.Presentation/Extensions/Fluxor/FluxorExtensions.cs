namespace Aristocrat.Extensions.Fluxor;

using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using global::Fluxor;
using Dispatcher = System.Windows.Threading.Dispatcher;

public static class FluxorExtensions
{
    public static Task DispatchAsync(this IDispatcher dispatcher, object action)
    {
        var tcs = new TaskCompletionSource<bool>();

        Dispatcher.CurrentDispatcher.Invoke(() =>
        {
            try
            {
                dispatcher.Dispatch(action);
                tcs.TrySetResult(true);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        return tcs.Task;
    }
}
