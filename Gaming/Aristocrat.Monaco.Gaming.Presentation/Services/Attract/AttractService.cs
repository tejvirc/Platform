namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attract;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Aristocrat.Monaco.Gaming.Contracts.Models;
using Aristocrat.Monaco.Gaming.UI.ViewModels;
//using Aristocrat.Monaco.Gaming.UI.ViewModels;
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
using LobbyState = Store.Lobby.LobbyState;

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

    private readonly object _attractLock = new object();

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

    public void StartAttractTimer()
    {
        if (_attractTimer != null)
        {
            _attractTimer.Stop();

            // When in single game mode, the game is in charge of display attract sequences, not the platform
            //TODO look to see if there is a better place to put this property
            if (_lobbyState.Value.AllowSingleGameAutoLaunch)
            {
                return;
            }

            //TODO look into idle text scrolling since we will want to make sure that we dont allow attract while it should be disabled.
            //There is a chance this check may not be needed at all since this is all in the backend and that is a frontend thing
            if (/*!IsIdleTextScrolling &&*/ _attractState.Value.CanAttractModeStart)
            {
                var interval = _attractState.Value.IsActive
                    ? _configuration.AttractSecondaryTimerIntervalInSeconds
                    : _configuration.AttractTimerIntervalInSeconds;

                _attractTimer.Interval = TimeSpan.FromSeconds(interval);
                _attractTimer.Start();
            }
        }
    }

    public void ExitAndResetAttractMode(/*AgeWarningTimer AgeWarningTimer (May add this in place of using the configuration so that it works exactly how it would in the LobbyViewModel)*/)
    {
        _dispatcher.Dispatch(new AttractExitAction());
        if (_attractTimer != null && _attractTimer.IsEnabled)
        {
            StartAttractTimer();
        }

        // Don't display Age Warning while the inserting cash dialog is up.
        //if (_ageWarningTimer.CheckForAgeWarning() == AgeWarningCheckResult.False && CurrentState == LobbyState.Attract)
        //TODO double check to make sure these fields correlate with the above if statement.
        if(!_configuration.DisplayAgeWarning && _attractState.Value.IsPlaying)
        {
            _dispatcher.Dispatch(new AttractExitedAction());
        }

        if (_attractState.Value.ResetAttractOnInterruption && _attractState.Value.CurrentAttractIndex != 0)
        {
            //TODO Not certain if this is the expected solution
            _dispatcher.Dispatch(new GameUninstalledAction());
            SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
        }
    }

    public bool CheckAndResetAttractIndex()
    {
        lock (_attractLock)
        {
            if (_attractState.Value.CurrentAttractIndex >= _attractState.Value.Videos.Count)
            {
                //TODO Not certain if this is the expected solution
                _dispatcher.Dispatch(new GameUninstalledAction());
                return true;
            }

            return false;
        }
    }

    public IEnumerable<GameInfo> GetAttractGameInfoList(ObservableCollection<GameInfo> gameList)
    {
        if (!_attractConfigurationProvider.IsAttractEnabled)
        {
            return new List<GameInfo>();
        }

        IEnumerable<GameInfo> subset = gameList
            .Where(g => g.Enabled)
            .DistinctBy(g => g.ThemeId).ToList();

        //TODO figure out a fix for this ConvertToSubTab issue
        //For now we just duplicated the code from the SubTabInfoViewModel.cs class
        //That code is below and will need to be deleted but for now this works around the error.
        if (subset.DistinctBy(g => g.GameSubtype).Count() > 1)
            subset = subset.OrderBy(g => g.GameType).ThenBy(g => SubTabInfoViewModel.ConvertToSubTab(g.GameSubtype));

        var attractSequence = _attractConfigurationProvider.GetAttractSequence().Where(ai => ai.IsSelected).ToList();

        var configuredAttractGameInfo =
            (from ai in attractSequence
             join g in subset on new { ai.ThemeId, ai.GameType } equals new { g.ThemeId, g.GameType }
             select g).ToList();

        return configuredAttractGameInfo;
    }

    public void RefreshAttractGameList(ObservableCollection<GameInfo> gameList)
    {
        List<AttractVideoDetails> attractList = new List<AttractVideoDetails>();

        if (_configuration.HasAttractIntroVideo)
        {
            attractList.Add(new AttractVideoDetails
            {
                BottomAttractVideoPath = _configuration.BottomAttractIntroVideoFilename,
                TopAttractVideoPath = _configuration.TopAttractIntroVideoFilename,
                TopperAttractVideoPath = _configuration.TopperAttractIntroVideoFilename
            });
        }

        //TODO do something to add the collection of attract game info to the attract list
        attractList.AddRange((List <AttractVideoDetails>)GetAttractGameInfoList(gameList));
        _dispatcher.Dispatch(
            new AttractAddVideosAction
                {
                    AttractList = attractList as System.Collections.Immutable.IImmutableList<AttractVideoDetails>
                });

        CheckAndResetAttractIndex();
    }

    //TODO had to rework the initial check on this value but this seems correct
    private bool IsAttractModeIdleTimeout()
    {
        return (_lobbyState.Value.LastUserInteractionTime != DateTime.MinValue
                || _lobbyState.Value.LastUserInteractionTime != DateTime.MaxValue) &&
                _lobbyState.Value.LastUserInteractionTime.AddSeconds(_attractState.Value.AttractModeIdleTimeoutInSeconds) <= DateTime.UtcNow;
    }
}
