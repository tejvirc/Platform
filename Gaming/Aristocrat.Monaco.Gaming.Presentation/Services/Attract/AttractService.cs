namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attract;

using System;
using System.Linq;
using Fluxor;
using Gaming.Contracts;
using Gaming.Contracts.Events;
using Kernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly AttractOptions _attractOptions;
    private readonly TranslateOptions _translateOptions;
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
        IOptions<AttractOptions> attractOptions,
        IOptions<TranslateOptions> translateOptions,
        IEventBus eventBus,
        IPropertiesManager properties,
        IAttractConfigurationProvider attractConfigurationProvider)
    {
        _logger = logger;
        _attractState = attractState;
        _lobbyState = lobbyState;
        _dispatcher = dispatcher;
        _attractOptions = attractOptions.Value;
        _translateOptions = translateOptions.Value;
        _eventBus = eventBus;
        _properties = properties;
        _attractConfigurationProvider = attractConfigurationProvider;

        _attractTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(_attractOptions.TimerIntervalInSeconds) };
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

        if (_attractOptions.AlternateLanguage)
        {
            var languageIndex = _attractState.Value.IsNextLanguagePrimary ? 0 : 1;

            var topAttractVideoPath =
                attract?.GetTopAttractVideoPathByLocaleCode(_translateOptions.LocaleCodes[languageIndex]).NullIfEmpty() ??
                _attractOptions.DefaultTopVideoFilename;

            var topperAttractVideoPath =
                attract?.GetTopperAttractVideoPathByLocaleCode(_translateOptions.LocaleCodes[languageIndex]).NullIfEmpty() ??
                _attractOptions.DefaultTopperVideoFilename;

            string? bottomAttractVideoPath = null;

            if (_attractOptions.BottomVideo)
            {
                bottomAttractVideoPath =
                    attract?.GetBottomAttractVideoPathByLocaleCode(_translateOptions.LocaleCodes[languageIndex]).NullIfEmpty() ??
                    _attractOptions.DefaultTopVideoFilename;
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
            var topAttractVideoPath = attract?.TopAttractVideoPath.NullIfEmpty() ?? _attractOptions.DefaultTopVideoFilename;

            var topperAttractVideoPath =
                attract?.TopperAttractVideoPath.NullIfEmpty() ??
                _attractOptions.DefaultTopperVideoFilename;

            string? bottomAttractVideoPath = null;

            if (_attractOptions.BottomVideo)
            {
                bottomAttractVideoPath =
                    attract?.BottomAttractVideoPath.NullIfEmpty() ?? _attractOptions.DefaultTopVideoFilename;
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
        if (!(_attractOptions.TopImageRotation is { Count: > 0 }))
        {
            return;
        }

        var newIndex = _attractState.Value.ModeTopImageIndex + 1;

        if (newIndex < 0 || newIndex >= _attractOptions.TopImageRotation.Count)
        {
            newIndex = 0;
        }

        _dispatcher.Dispatch(new AttractUpdateTopImageIndexAction { Index = newIndex });
    }

    public void RotateTopperImage()
    {
        if (!(_attractOptions.TopperImageRotation is { Count: > 0 }))
        {
            return;
        }

        var newIndex = _attractState.Value.ModeTopperImageIndex + 1;

        if (newIndex < 0 || newIndex >= _attractOptions.TopperImageRotation.Count)
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
