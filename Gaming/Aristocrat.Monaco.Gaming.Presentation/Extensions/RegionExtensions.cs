namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using System.Threading.Tasks;
using Prism.Regions;

public static class RegionExtensions
{
    public static Task RequestNavigateAsync(this IRegion region, string viewName)
    {
        var tcs = new TaskCompletionSource<bool>();

        region.RequestNavigate(viewName, (NavigationResult nr) =>
        {
            if (nr.Result != null && (bool)nr.Result)
            {
                tcs.TrySetResult(true);
                return;
            }

            tcs.TrySetException(new InvalidOperationException($"Navigation failed for {viewName} into {region.Name} with message -- {nr.Error}"));
        });

        return tcs.Task;
    }
}
