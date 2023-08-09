namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;

internal static class ContainerExtensions
{
    public static IServiceCollection ServiceCollection(this IContainerRegistry containerRegistry)
    {
        if (containerRegistry is LobbyContainerExtension pce)
        {
            return pce.Services;
        }

        throw new NotImplementedException("IContainerRegistry must be implemented from the concrete type LobbyContainerExtension");
    }

    public static object? GetOrConstructService(this IServiceProvider provider, Type type, params (Type Type, object Instance)[] parameters)
    {
        var instance = provider.GetService(type);
        if (instance is null && !type.IsInterface && !type.IsAbstract)
        {
            var ctor = type.GetConstructors().OrderByDescending(x => x.GetParameters().Length).FirstOrDefault();
            if (ctor is null)
                throw new NullReferenceException($"Could not locate a public constructor for {type.FullName}");

            var ctorParameters = ctor.GetParameters();
            var args = ctor.GetParameters().Select(x =>
            {
                object arg = parameters.FirstOrDefault(p => x.ParameterType.IsAssignableFrom(p.Instance.GetType())).Instance;
                if (arg != null)
                    return arg;

                return provider.GetService(x.ParameterType);
            });
            return ctor.Invoke(args.ToArray());
        }
        return instance;
    }
}
