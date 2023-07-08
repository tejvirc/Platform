﻿namespace Aristocrat.Monaco.Gaming.Lobby.Platform.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Fluxor;
using Store;

public class GameUninstalledConsumer : Consumes<GameUninstalledEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameUninstalledConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameUninstalledEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameUninstalledAction());
    }
}
