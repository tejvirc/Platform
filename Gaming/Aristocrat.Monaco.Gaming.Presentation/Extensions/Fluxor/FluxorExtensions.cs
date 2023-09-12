namespace Aristocrat.Extensions.Fluxor;

using System.Threading.Tasks;
using global::Fluxor;

public static class FluxorExtensions
{
    public static Task DispatchAsync(this IDispatcher dispatcher, object action)
    {
        dispatcher.Dispatch(action);

        return Task.CompletedTask;
    }
}
