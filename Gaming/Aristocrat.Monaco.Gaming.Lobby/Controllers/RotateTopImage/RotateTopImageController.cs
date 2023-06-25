namespace Aristocrat.Monaco.Gaming.Lobby.Controllers.RotateTopImage;

using System;
using Aristocrat.Monaco.UI.Common;
using Controllers;
using global::Fluxor;
using Store;

public sealed class RotateTopImageController : IController, IDisposable
{
    private const double RotateTopImageIntervalInSeconds = 10.0;

    private readonly IDispatcher _dispatcher;

    private readonly ITimer _rotateTopImageTimer;

    public RotateTopImageController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        _rotateTopImageTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(RotateTopImageIntervalInSeconds) };
        _rotateTopImageTimer.Tick += RotateTopImageTimerTick;
    }

    public void Dispose()
    {
        _rotateTopImageTimer.Stop();
    }

    private void RotateTopImageTimerTick(object? sender, EventArgs e)
    {
        ToggleImage();
    }

    private void ToggleImage()
    {
        _dispatcher.Dispatch(new ToggleTopImageAction());
    }
}
