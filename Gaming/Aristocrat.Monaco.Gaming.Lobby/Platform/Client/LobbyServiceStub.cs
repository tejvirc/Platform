namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Client;

using CommandHandlers;
using LobbyRuntime.V1;

public class LobbyServiceStub
{
    private readonly ICommandHandlerFactory _commandHandlerFactory;

    public LobbyServiceStub(ICommandHandlerFactory commandHandlerFactory)
    {
        _commandHandlerFactory = commandHandlerFactory;
    }

    public Empty SendEvent(SendNotificationRequest request)
    {
        //_commandHandlerFactory.Create<Notification>()
        //    .Handle(new Notification { Code = request.EventCode });

        return new Empty();
    }
}
