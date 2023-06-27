namespace Aristocrat.Monaco.Gaming.Lobby.Runtime.Server;

using Aristocrat.LobbyRuntime.V1;
using CommandHandlers;

public class LobbyService : ILobbyService
{
    private readonly ICommandHandlerFactory _handlerFactory;

    public LobbyService(
        ICommandHandlerFactory handlerFactory)
    {
        _handlerFactory = handlerFactory;
    }

    public GetGamesResponse GetGames(Empty request)
    {
        var command = new GetGames();

        _handlerFactory.Create<GetGames>().Handle(command);

        return new GetGamesResponse { Games = command.Games };
    }
}
