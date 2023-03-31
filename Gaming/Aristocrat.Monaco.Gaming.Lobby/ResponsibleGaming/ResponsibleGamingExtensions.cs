namespace Aristocrat.Monaco.Gaming.Lobby.ResponsibleGaming;

using SimpleInjector;

public static class ResponsibleGamingExtensions
{
    public static Container RegisterResponsibleGaming(this Container container)
    {
        container.Register<IResponsibleGaming>(() =>
        {
            var config = container.GetInstance<LobbyConfiguration>();

            // TODO Check config to determine if using the default lobby or game-driven lobby

            return container.GetInstance<DefaultResponsibleGamingController>();
        }, Lifestyle.Singleton);

        return container;
    }
}
