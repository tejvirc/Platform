namespace Aristocrat.Monaco.Gaming.Presentation;

using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Monaco.UI.Common;

public static class ApplicationExtensions
{
    public static TService GetService<TService>(this Application application) where TService : class
    {
        var services = ((MonacoApplication)application).Services ??
                       throw new InvalidOperationException("Services were not set on Application");

        return services.GetRequiredService<TService>();
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
