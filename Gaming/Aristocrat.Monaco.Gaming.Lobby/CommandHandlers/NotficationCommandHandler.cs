namespace Aristocrat.Monaco.Gaming.Lobby.CommandHandlers;

using Fluxor;
using LobbyRuntime.V1;

public class NotficationCommandHandler : ICommandHandler<Notification>
{
    private readonly IDispatcher _dispatcher;

    public NotficationCommandHandler(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public void Handle(Notification command)
    {
        switch (command.Code)
        {
            case NotificationCode.GameDisabled:
            case NotificationCode.GameEnabled:
            case NotificationCode.GameInstalled:
            case NotificationCode.GameUninstalled:
            case NotificationCode.GameUpgraded:
                _dispatcher.Dispatch(new LoadGamesAction());
                break;
        }
    }
}
