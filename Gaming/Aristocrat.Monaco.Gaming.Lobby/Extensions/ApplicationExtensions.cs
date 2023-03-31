namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using System.Collections.Generic;
using System.Windows;
using Aristocrat.Monaco.UI.Common;
using Microsoft.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static TService GetService<TService>(this Application application) where TService : class
    {
        var services = ((MonacoApplication)application).Services ??
                       throw new InvalidOperationException("Services were not set on Application");

        return services.GetRequiredService<TService>();
    }

    public static object GetService(this Application application, Type serviceType)
    {
        var services = ((MonacoApplication)application).Services ??
                       throw new InvalidOperationException("Services were not set on Application");

        return services.GetRequiredService(serviceType);
    }

    public static IEnumerable<TService> GetServices<TService>(this Application application) where TService : class
    {
        var services = ((MonacoApplication)application).Services ??
                       throw new InvalidOperationException("Services were not set on Application");

        return services.GetRequiredService<IEnumerable<TService>>();
    }
}
