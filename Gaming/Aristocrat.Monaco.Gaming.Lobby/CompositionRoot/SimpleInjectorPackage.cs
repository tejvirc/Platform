namespace Aristocrat.Monaco.Gaming.Lobby.CompositionRoot;

using Contracts.Lobby;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Toolkit.Mvvm.Extensions;
using Views;

public class SimpleInjectorPackage : IPackage
{
    public SimpleInjectorPackage(IServiceCollection services)
    {
    }
}
