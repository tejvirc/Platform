namespace Aristocrat.Monaco.Gaming.Presentation.Services.Attract;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Extensions.Fluxor;
using Accounting.Contracts;
using Application.Contracts.Extensions;
using Contracts.Models;
using Contracts.Progressives;
using Store.Chooser;
using Store.IdleText;
using Gaming.Progressives;
using UI.ViewModels;
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
    private readonly IState<ChooserState> _chooserState;
    private readonly IState<IdleTextState> _idleTextState;
    private readonly IDispatcher _dispatcher;
    private readonly LobbyConfiguration _configuration;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private readonly IBank _bank;
    private readonly IAttractConfigurationProvider _attractConfigurationProvider;

    private readonly Timer _attractTimer;
    private readonly Timer _rotateTopImageTimer;
    private readonly Timer _rotateTopperImageTimer;

    private readonly object _attractLock = new object();

    //private int _consecutiveAttractCount;
    //private bool _nextAttractModeLanguageIsPrimary = true;
    //private bool _lastInitialAttractModeLanguageIsPrimary = true;

    public AttractService(
        ILogger<AttractService> logger,
        IState<AttractState> attractState,
        IState<LobbyState> lobbyState,
        IState<ChooserState> chooserState,
        IState<IdleTextState> idleTextState,
        IDispatcher dispatcher,
        LobbyConfiguration configuration,
        IEventBus eventBus,
        IPropertiesManager properties,
        IAttractConfigurationProvider attractConfigurationProvider,
        IBank bank)
    {
        _logger = logger;
        _attractState = attractState;
        _lobbyState = lobbyState;
        _chooserState = chooserState;
        _idleTextState = idleTextState;
        _dispatcher = dispatcher;
        _configuration = configuration;
        _eventBus = eventBus;
        _properties = properties;
        _bank = bank;
        _attractConfigurationProvider = attractConfigurationProvider;

        _attractTimer = new Timer { Interval = TimeSpan.FromSeconds(_configuration.AttractTimerIntervalInSeconds).TotalMilliseconds};
        _attractTimer.Elapsed += AttractTimer_Tick;

        _rotateTopImageTimer = new Timer { Interval = TimeSpan.FromSeconds(RotateTopImageIntervalInSeconds).TotalMilliseconds};
        _rotateTopImageTimer.Elapsed += RotateTopImageTimerTick;

        _rotateTopperImageTimer = new Timer { Interval = TimeSpan.FromSeconds(RotateTopperImageIntervalInSeconds).TotalMilliseconds};
        _rotateTopperImageTimer.Elapsed += RotateTopperImageTimerTick;

        _dispatcher.DispatchAsync(new AttractSetCanModeStartAction { Bank = bank, Properties = properties });
    }


    public string ActiveLocaleCode => _attractState.Value.IsPrimaryLanguageSelected ? _configuration.LocaleCodes[0] : _configuration.LocaleCodes[1];

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
        IAttractDetails? attract = null;

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
        if(!_attractState.Value.CanAttractModeStart ||
            _idleTextState.Value.IsScrolling )
        //TODO (Future Task) figure out which state should have these fields and implement them
        //    _attractState.Value.IsVoucherNotificationActive ||
        //    _attractState.Value.IsProgressiveGameDisabledNotificationActive ||
        //    _attractState.Value.IsPlayerInfoRequestActive)
        {
            return;
        }

        _dispatcher.DispatchAsync(new AttractEnterAction());
    }

    private void RotateTopImageTimerTick(object? sender, EventArgs e)
    {
        if (_attractState.Value.IsPlaying)
        {
            _dispatcher.Dispatch(new AttractRotateTopImageAction());
        }
    }

    private void RotateTopperImageTimerTick(object? sender, EventArgs e)
    {
        if (_attractState.Value.IsPlaying)
        {
            _dispatcher.Dispatch(new AttractRotateTopperImageAction());
        }
    }

    public void StartAttractTimer()
    {
        if (_attractTimer != null)
        {
            _attractTimer.Stop();

            // When in single game mode, the game is in charge of display attract sequences, not the platform
            if (_lobbyState.Value.AllowSingleGameAutoLaunch)
            {
                return;
            }

            //TODO (Future Task) take a look back into this while implementing the UI incase the idle text scrolling check is unnecessary
            //                   for now just keep it functioning as it was previously in the LobbyViewModel.
            if (_idleTextState.Value.IsScrolling && _attractState.Value.CanAttractModeStart)
            {
                var interval = _attractState.Value.IsActive
                    ? _configuration.AttractSecondaryTimerIntervalInSeconds
                    : _configuration.AttractTimerIntervalInSeconds;

                _attractTimer.Interval = TimeSpan.FromSeconds(interval).TotalMilliseconds;
                _attractTimer.Start();

                _rotateTopImageTimer.Start();
                _rotateTopperImageTimer.Start();
            }
        }
    }

    public void ExitAndResetAttractMode()
    {
        _dispatcher.Dispatch(new AttractExitAction());
        if (_attractTimer != null && _attractTimer.Enabled)
        {
            StartAttractTimer();
        }

        // Don't display Age Warning while the inserting cash dialog is up.
        // TODO (Future Task) When we have a set way to determine if the current state matches with the old LobbyState.CashIn
        // we can add an && check for that after the config.DisplayAgeWarning check to make it match directly with the old functionality.
        if(!_configuration.DisplayAgeWarning && _attractState.Value.IsPlaying)
        {
            _dispatcher.Dispatch(new AttractExitedAction());
        }

        if (_attractState.Value.ResetAttractOnInterruption && _attractState.Value.CurrentAttractIndex != 0)
        {
            _dispatcher.Dispatch(new AttractUpdateIndexAction { AttractIndex = 0 });
            SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
        }
    }

    public bool CheckAndResetAttractIndex()
    {
        lock (_attractLock)
        {
            if (_attractState.Value.CurrentAttractIndex >= _attractState.Value.Videos.Count)
            {
                _dispatcher.Dispatch(new AttractUpdateIndexAction { AttractIndex = 0 });
                return true;
            }

            return false;
        }
    }

    public IEnumerable<GameInfo> GetAttractGameInfoList()
    {
        if (!_attractConfigurationProvider.IsAttractEnabled)
        {
            return new List<GameInfo>();
        }

        IEnumerable<GameInfo> subset = _chooserState.Value.Games
            .Where(g => g.Enabled)
            .DistinctBy(g => g.ThemeId).ToList();

        if (subset.DistinctBy(g => g.GameSubtype).Count() > 1)
            subset = subset.OrderBy(g => g.GameType).ThenBy(g => SubTabInfoViewModel.ConvertToSubTab(g.GameSubtype));

        var attractSequence = _attractConfigurationProvider.GetAttractSequence().Where(ai => ai.IsSelected).ToList();

        var configuredAttractGameInfo =
            (from ai in attractSequence
             join g in subset on new { ai.ThemeId, ai.GameType } equals new { g.ThemeId, g.GameType }
             select g).ToList();

        return configuredAttractGameInfo;
    }

    /// <summary>
    ///     This is called after the game list has been updated
    /// </summary>
    /// <param name="gameList">The updated game list</param>
    public void RefreshAttractGameList()
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

        var listAsInfo = GetAttractGameInfoList();
        var listToAdd = new List<AttractVideoDetails>();
        foreach(var v in listAsInfo)
        {
            if(v != null)
            {
                //TODO look into the LocaleAttractGraphics field on the AttractVideoDetails object.
                AttractVideoDetails details = new AttractVideoDetails { BottomAttractVideoPath = v.BottomAttractVideoPath,
                    TopAttractVideoPath = v.TopAttractVideoPath, TopperAttractVideoPath = v.TopperAttractVideoPath};
                listToAdd.Add(details);
            }
        }
        attractList.AddRange(listToAdd);
        _dispatcher.Dispatch( new AttractSetVideosAction { AttractList = attractList });

        CheckAndResetAttractIndex();
    }

    //TODO (Future Task) See Below:
    //This was pulled from the LobbyStateManager and works in tandem with ChooserIdleText as a check for a permitted state change to attract.
    //As a result this may not be needed but until its implementation is determined as necessary or not it should stay here.
    private bool IsAttractModeIdleTimeout()
    {
        return (_lobbyState.Value.LastUserInteractionTime != DateTime.MinValue
                || _lobbyState.Value.LastUserInteractionTime != DateTime.MaxValue) &&
                _lobbyState.Value.LastUserInteractionTime.AddSeconds(_attractState.Value.AttractModeIdleTimeoutInSeconds) <= DateTime.UtcNow;
    }

    private void OnGameAttractVideoCompleted()
    {
        // Have to run this on a separate thread because we are triggering off an event from the video
        // and we end up making changes to the video control (loading new video).  The Bink Video Control gets very upset
        // if we try to do that on the same thread.

        if (!PlayAdditionalConsecutiveAttractVideo())
        {
            RotateTopImage();
            RotateTopperImage();
            //TODO (Future Task) - Implement this functionality when the Attract View Model is being implemented
            //Task.Run(
            //    () => { MvvmHelper.ExecuteOnUI(() => SendTrigger(LobbyTrigger.AttractVideoComplete)); });
        }
    }

    private bool PlayAdditionalConsecutiveAttractVideo()
    {
        if (!_configuration.HasAttractIntroVideo || _attractState.Value.CurrentAttractIndex != 0 || _attractState.Value.Videos.Count <= 1)
        {
            _dispatcher.Dispatch(new AttractUpdateConsecutiveCount
            {
                ConsecutiveAttractCount = _attractState.Value.ConsecutiveAttractCount  + 1
            });

            _logger.LogDebug($"Consecutive Attract Video count: {_attractState.Value.ConsecutiveAttractCount}");

            if (_attractState.Value.ConsecutiveAttractCount >= _configuration.ConsecutiveAttractVideos ||
                _attractState.Value.ConsecutiveAttractCount >= _chooserState.Value.Games.Count)
            {
                _logger.LogDebug("Stopping attract video sequence");
                return false;
            }

            _logger.LogDebug("Starting another attract video");
        }

        Task.Run(
            () =>
            {
                if (_attractState.Value.Videos.Count <= 1)
                {
                    _dispatcher.Dispatch(new AttractExitAction());
                }

                AdvanceAttractIndex();
                SetAttractVideoPaths(_attractState.Value.CurrentAttractIndex);
            });

        return true;
    }

    public void UserInteracted()
    {
        ExitAndResetAttractMode();
    }
}
