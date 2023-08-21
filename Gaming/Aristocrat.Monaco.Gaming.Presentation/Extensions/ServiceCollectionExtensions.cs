namespace Aristocrat.Monaco.Gaming.Presentation;

using Kernel;
using Microsoft.Extensions.DependencyInjection;
using SimpleInjector;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformService<TService>(this IServiceCollection services)
        where TService : class
    {
        return services.AddSingleton(provider => provider.GetRequiredService<Container>().GetInstance<TService>());
    }

    public static IServiceCollection AddLocatedPlatformService<TService>(this IServiceCollection services)
        where TService : class
    {
        return services.AddSingleton(_ => ServiceManager.GetInstance().GetService<TService>());
    }
}
