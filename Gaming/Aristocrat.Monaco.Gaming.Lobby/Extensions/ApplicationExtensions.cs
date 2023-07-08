namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using System.Collections.Generic;
using System.Windows;
using Aristocrat.Monaco.UI.Common;
using Microsoft.Extensions.DependencyInjection;
using Services;

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

    public static T GetObject<T>(this Application application) where T : class
    {
        return (T)application.GetObject(typeof(T));
    }

    public static object GetObject(this Application application, Type objecType)
    {
        var services = ((MonacoApplication)application).Services ??
                       throw new InvalidOperationException("Services were not set on Application");

        if (objecType.IsAbstract || objecType.IsInterface)
        {
            throw new InvalidOperationException($"{objecType} is not a concrete type");
        }

        return ActivatorUtilities.CreateInstance(services, objecType);
    }
}
