namespace Aristocrat.Monaco.Gaming.Lobby.Services.ResponsibleGaming;

using SimpleInjector;

public static class ResponsibleGamingExtensions
{
    public static Container RegisterResponsibleGaming(this Container container)
    {
        container.Register<IResponsibleGamingController>(
            () =>
            {
                var config = container.GetInstance<LobbyConfiguration>();

                return config.ResponsibleGamingTimeLimitEnabled
                    ? container.GetInstance<NoResponsibleGamingController>()
                    : container.GetInstance<DefaultResponsibleGamingController>();
            },
            Lifestyle.Singleton);

        return container;
    }
}
