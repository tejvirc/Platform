namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attract;

using System;
using System.Linq;
using Contracts;
using Contracts.Events;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Monaco.UI.Common;
using Monaco.UI.Common.Extensions;
using Store;
using Store.Attract;
using Store.Lobby;
using UI.Models;

public sealed class AttractService : IAttractService, IDisposable
{
    private const double RotateTopImageIntervalInSeconds = 10.0;
    private const double RotateTopperImageIntervalInSeconds = 10.0;

    private readonly ILogger<AttractService> _logger;
    private readonly IState<AttractState> _attractState;
    private readonly IState<LobbyState> _lobbyState;
    private readonly IDispatcher _dispatcher;
    private readonly LobbyConfiguration _configuration;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private readonly IAttractConfigurationProvider _attractConfigurationProvider;

    private readonly ITimer _attractTimer;
    private readonly ITimer _rotateTopImageTimer;
    private readonly ITimer _rotateTopperImageTimer;

    //private int _consecutiveAttractCount;
    //private bool _nextAttractModeLanguageIsPrimary = true;
    //private bool _lastInitialAttractModeLanguageIsPrimary = true;

    public AttractService(
        ILogger<AttractService> logger,
        IState<AttractState> attractState,
        IState<LobbyState> lobbyState,
        IDispatcher dispatcher,
        LobbyConfiguration configuration,
        IEventBus eventBus,
        IPropertiesManager properties,
        IAttractConfigurationProvider attractConfigurationProvider)
    {
        _logger = logger;
        _attractState = attractState;
        _lobbyState = lobbyState;
        _dispatcher = dispatcher;
        _configuration = configuration;
        _eventBus = eventBus;
        _properties = properties;
        _attractConfigurationProvider = attractConfigurationProvider;

        _attractTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(_configuration.AttractTimerIntervalInSeconds) };
        _attractTimer.Tick += AttractTimer_Tick;

        _rotateTopImageTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(RotateTopImageIntervalInSeconds) };
        _rotateTopImageTimer.Tick += RotateTopImageTimerTick;

        _rotateTopperImageTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(RotateTopperImageIntervalInSeconds) };
        _rotateTopperImageTimer.Tick += RotateTopperImageTimerTick;
    }

    public void NotifyEntered()
    {
        _eventBus.Publish(new AttractModeEntered());
    }

    public int AdvanceAttractIndex()
    {
        var currentAttractIndex = _attractState.Value.CurrentAttractIndex + 1;

        if (currentAttractIndex >= _attractState.Value.Videos.Count)
        {
            currentAttractIndex = 0;
        }

        return currentAttractIndex;
    }

    public void SetAttractVideoPaths(int currAttractIndex)
    {
        AttractVideoDetails? attract = null;

        if (_attractState.Value.Videos.Count > 0)
        {
            attract = _attractState.Value.Videos.ElementAtOrDefault(currAttractIndex);
        }

        if (_configuration.AlternateAttractModeLanguage)
        {
            var languageIndex = _attractState.Value.IsNextLanguagePrimary ? 0 : 1;

            var topAttractVideoPath =
                attract?.GetTopAttractVideoPathByLocaleCode(_configuration.LocaleCodes[languageIndex]).NullIfEmpty() ??
                _configuration.DefaultTopAttractVideoFilename;

            var topperAttractVideoPath =
                attract?.GetTopperAttractVideoPathByLocaleCode(_configuration.LocaleCodes[languageIndex]).NullIfEmpty() ??
                _configuration.DefaultTopperAttractVideoFilename;

            string? bottomAttractVideoPath = null;

            if (_configuration.BottomAttractVideoEnabled)
            {
                bottomAttractVideoPath =
                    attract?.GetBottomAttractVideoPathByLocaleCode(_configuration.LocaleCodes[languageIndex]).NullIfEmpty() ??
                    _configuration.DefaultTopAttractVideoFilename;
            }

            _dispatcher.Dispatch(
                new AttractUpdateVideosAction
                {
                    TopAttractVideoPath = topAttractVideoPath,
                    TopperAttractVideoPath = topperAttractVideoPath,
                    BottomAttractVideoPath = bottomAttractVideoPath
                });
        }
        else
        {
            var topAttractVideoPath = attract?.TopAttractVideoPath.NullIfEmpty() ?? _configuration.DefaultTopAttractVideoFilename;

            var topperAttractVideoPath =
                attract?.TopperAttractVideoPath.NullIfEmpty() ??
                _configuration.DefaultTopperAttractVideoFilename;

            string? bottomAttractVideoPath = null;

            if (_configuration.BottomAttractVideoEnabled)
            {
                bottomAttractVideoPath =
                    attract?.BottomAttractVideoPath.NullIfEmpty() ?? _configuration.DefaultTopAttractVideoFilename;
            }

            _dispatcher.Dispatch(
                new AttractUpdateVideosAction
                {
                    TopAttractVideoPath = topAttractVideoPath,
                    TopperAttractVideoPath = topperAttractVideoPath,
                    BottomAttractVideoPath = bottomAttractVideoPath
                });
        }
    }

    public void RotateTopImage()
    {
        if (!(_configuration.RotateTopImageAfterAttractVideo is { Length: > 0 }))
        {
            return;
        }

        var newIndex = _attractState.Value.ModeTopImageIndex + 1;

        if (newIndex < 0 || newIndex >= (_configuration.RotateTopImageAfterAttractVideo?.Length ?? 0))
        {
            newIndex = 0;
        }

        _dispatcher.Dispatch(new AttractUpdateTopImageIndexAction { Index = newIndex });
    }

    public void RotateTopperImage()
    {
        if (!(_configuration.RotateTopperImageAfterAttractVideo is { Length: > 0 }))
        {
            return;
        }

        var newIndex = _attractState.Value.ModeTopperImageIndex + 1;

        if (newIndex < 0 || newIndex >= (_configuration.RotateTopperImageAfterAttractVideo?.Length ?? 0))
        {
            newIndex = 0;
        }

        _dispatcher.Dispatch(new AttractUpdateTopperImageIndexAction { Index = newIndex });
    }

    public void Dispose()
    {
        _attractTimer.Stop();
    }

    private void AttractTimer_Tick(object? sender, EventArgs e)
    {
        //if (!_attractConfigurationProvider.IsAttractEnabled ||
        //    !_attractState.Value.HasZeroCredits() ||
        //    _attractState.Value.IsIdleTextScrolling ||
        //    _attractState.Value.IsVoucherNotificationActive ||
        //    _attractState.Value.IsProgressiveGameDisabledNotificationActive ||
        //    _attractState.Value.IsPlayerInfoRequestActive)
        //{
        //    return;
        //}

        _dispatcher.Dispatch(new AttractEnterAction());
    }

    private void RotateTopImageTimerTick(object? sender, EventArgs e)
    {
        _dispatcher.Dispatch(new AttractRotateTopImageAction());
    }

    private void RotateTopperImageTimerTick(object? sender, EventArgs e)
    {
        _dispatcher.Dispatch(new AttractRotateTopperImageAction());
    }
}
