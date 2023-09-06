namespace Aristocrat.Monaco.Gaming.Presentation.Services.Chooser;

using System;
using Fluxor;
using Gaming.Contracts.Models;
using Monaco.UI.Common;
using Store;

public sealed class GameFilterService : IGameFilterService, IDisposable
{
    private const double IdleTimerIntervalSeconds = 15.0;

    private readonly IDispatcher _dispatcher;

    private readonly ITimer _idleTimer;

    public GameFilterService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        _idleTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(IdleTimerIntervalSeconds) };
        _idleTimer.Tick += IdleTimer_Tick;
    }

    public void ResetIdleTimer()
    {
        _idleTimer.Stop();
        _idleTimer.Start();
    }

    public void Dispose()
    {
        _idleTimer.Stop();
    }

    private void IdleTimer_Tick(object? sender, EventArgs e)
    {
        _dispatcher.Dispatch(new ChooserUpdateDenomFilterAction(-1));
        _dispatcher.Dispatch(new ChooserUpdateGameFilterAction(GameType.Undefined));
    }
}
