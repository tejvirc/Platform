namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System;
using Kernel;

public sealed class GameLauncher : IGameLauncher, IDisposable
{
    private readonly IEventBus _eventBus;

    public GameLauncher(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void Dispose()
    {
    }
}
