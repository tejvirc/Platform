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
using Gaming.Contracts.Models;
using Gaming.Contracts.Progressives;
using Store.Chooser;
using Store.IdleText;
using Gaming.Progressives;
using UI.ViewModels;
using Contracts;
using Options;
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
using LobbyState = Store.Lobby.LobbyState;

public sealed class AttractService : IAttractService, IDisposable
{
    private readonly ILogger<AttractService> _logger;
    private readonly IState<AttractState> _attractState;
    private readonly IState<LobbyState> _lobbyState;
    private readonly IState<ChooserState> _chooserState;
    private readonly IState<IdleTextState> _idleTextState;
    private readonly IDispatcher _dispatcher;
    private readonly AttractOptions _attractOptions;
    private readonly TranslateOptions _translateOptions;
    private readonly IEventBus _eventBus;
    private readonly IAttractConfigurationProvider _attractConfigurationProvider;
    private readonly ITopImageRotationService _topImageRotationService;
    private readonly ITopperImageRotationService _topperImageRotationService;

    private readonly Timer _attractTimer;

    public AttractService(
        ILogger<AttractService> logger,
        IState<AttractState> attractState,
        IState<LobbyState> lobbyState,
        IState<ChooserState> chooserState,
        IState<IdleTextState> idleTextState,
        IDispatcher dispatcher,
        IOptions<AttractOptions> attractOptions,
        IOptions<TranslateOptions> translateOptions,
        IEventBus eventBus,
        IAttractConfigurationProvider attractConfigurationProvider)
    {
        _logger = logger;
        _attractState = attractState;
        _lobbyState = lobbyState;
        _chooserState = chooserState;
        _idleTextState = idleTextState;
        _dispatcher = dispatcher;
        _attractOptions = attractOptions.Value;
        _translateOptions = translateOptions.Value;
        _eventBus = eventBus;
        _attractConfigurationProvider = attractConfigurationProvider;

        _attractTimer = new Timer { Interval = TimeSpan.FromSeconds(_attractOptions.TimerIntervalInSeconds).TotalMilliseconds };
        _attractTimer.Elapsed += AttractTimer_Tick;
    }

    public void NotifyEntered()
    {
        _eventBus.Publish(new AttractModeEntered());
    }

    public void NotifyExited()
    {
        _eventBus.Publish(new AttractModeExited());
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
                    ? _attractOptions.SecondaryTimerIntervalInSeconds
                    : _attractOptions.TimerIntervalInSeconds;

                _attractTimer.Interval = TimeSpan.FromSeconds(interval).TotalMilliseconds;
                _attractTimer.Start();
            }
        }
    }

    public void CheckAndStartAttractTimer()
    {
        if (_attractTimer != null && _attractTimer.Enabled)
        {
            StartAttractTimer();
        }
    }

    public bool CheckAndResetAttractIndex()
    {
        if (_attractState.Value.CurrentAttractIndex >= _attractState.Value.Videos.Count)
        {
            _dispatcher.Dispatch(new AttractUpdateIndexAction { AttractIndex = 0 });
            return true;
        }
        
        return false;
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

        if (_attractOptions.HasIntroVideo)
        {
            attractList.Add(new AttractVideoDetails
            {
                BottomAttractVideoPath = _attractOptions.BottomIntroVideoFilename,
                TopAttractVideoPath = _attractOptions.TopIntroVideoFilename,
                TopperAttractVideoPath = _attractOptions.TopperIntroVideoFilename
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
}
