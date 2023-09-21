﻿namespace Aristocrat.Monaco.Gaming.Presentation.Consumers;

using System.Threading;
using System.Threading.Tasks;
using Gaming.Contracts;
using Extensions.Fluxor;
using Fluxor;
using Store;

public class GameDisabledConsumer : Consumes<GameDisabledEvent>
{
    private readonly IDispatcher _dispatcher;

    public GameDisabledConsumer(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async Task ConsumeAsync(GameDisabledEvent theEvent, CancellationToken cancellationToken)
    {
        await _dispatcher.DispatchAsync(new GameDisabledAction());
    }
}