namespace Aristocrat.Monaco.Gaming.Lobby.Controllers.RotateTopperImage;

using System;
using System.Threading.Tasks;
using Aristocrat.Monaco.UI.Common;
using global::Fluxor;
using Microsoft.Extensions.Logging;
using Store;
using Store.Lobby;

public sealed class RotateTopperImageController : IController, IDisposable
{
    private const double RotateTopperImageIntervalInSeconds = 10.0;

    private readonly ILogger<RotateTopperImageController> _logger;
    private readonly IDispatcher _dispatcher;
    private readonly IState<LobbyState> _state;
    private readonly LobbyConfiguration _configuration;

    private readonly ITimer _rotateTopperImageTimer;

    public RotateTopperImageController(ILogger<RotateTopperImageController> logger, IDispatcher dispatcher, IState<LobbyState> state, LobbyConfiguration configuration)
    {
        _logger = logger;
        _dispatcher = dispatcher;
        _state = state;
        _configuration = configuration;

        _rotateTopperImageTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(RotateTopperImageIntervalInSeconds) };
        _rotateTopperImageTimer.Tick += RotateTopperImageTimerTick;
    }

    public void Dispose()
    {
        _rotateTopperImageTimer.Stop();
    }

    private void RotateTopperImageTimerTick(object? sender, EventArgs e)
    {
        ToggleImage();
    }

    private void ToggleImage()
    {
        _dispatcher.Dispatch(new ToggleTopperImageAction());
    }
}
