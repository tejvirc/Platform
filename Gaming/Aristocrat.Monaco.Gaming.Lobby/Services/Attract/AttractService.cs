namespace Aristocrat.Monaco.Gaming.Lobby.Services.Attract;

using System;
using System.Threading.Tasks;
using Aristocrat.Monaco.Gaming.Lobby;
using Contracts;
using Contracts.Events;
using Fluxor;
using Kernel;
using Microsoft.Extensions.Logging;
using Models;
using Store;
using Store.Attract;
using Store.Lobby;
using UI.Common;
using UI.Common.Extensions;

public sealed class AttractService : IAttractService, IDisposable
{
    private readonly ILogger<AttractService> _logger;
    private readonly IState<AttractState> _attractState;
    private readonly IDispatcher _dispatcher;
    private readonly LobbyConfiguration _configuration;
    private readonly IEventBus _eventBus;
    private readonly IPropertiesManager _properties;
    private readonly IAttractConfigurationProvider _attractConfigurationProvider;

    private readonly ITimer _attractTimer;

    private int _consecutiveAttractCount;
    private bool _nextAttractModeLanguageIsPrimary = true;
    private bool _lastInitialAttractModeLanguageIsPrimary = true;

    public AttractService(
        ILogger<AttractService> logger,
        IState<AttractState> attractState,
        IDispatcher dispatcher,
        LobbyConfiguration configuration,
        IEventBus eventBus,
        IPropertiesManager properties,
        IAttractConfigurationProvider attractConfigurationProvider)
    {
        _logger = logger;
        _attractState = attractState;
        _dispatcher = dispatcher;
        _configuration = configuration;
        _eventBus = eventBus;
        _properties = properties;
        _attractConfigurationProvider = attractConfigurationProvider;

        _attractTimer = new DispatcherTimerAdapter { Interval = TimeSpan.FromSeconds(_configuration.AttractTimerIntervalInSeconds) };
        _attractTimer.Tick += AttractTimer_Tick;
    }

    public void NotifyEntered()
    {
        _eventBus.Publish(new AttractModeEntered());
    }

    public void ResetConsecutiveAttractCount()
    {
        _consecutiveAttractCount = 0;
    }

    public void SetLanguageFlags()
    {
        if (_configuration.AlternateAttractModeLanguage)
        {
            _nextAttractModeLanguageIsPrimary = !_nextAttractModeLanguageIsPrimary;
        }

        if (currAttractIndex == 0 && _configuration.AlternateAttractModeLanguage)
        {
            _nextAttractModeLanguageIsPrimary = !_lastInitialAttractModeLanguageIsPrimary;
            _lastInitialAttractModeLanguageIsPrimary = _nextAttractModeLanguageIsPrimary;
        }
    }

    public int AdvanceAttractIndex()
    {
        var currentAttractIndex = _attractState.Value.CurrentAttractIndex + 1;

        if (currentAttractIndex >= _attractState.Value.AttractList.Count)
        {
            currentAttractIndex = 0;
        }

        return currentAttractIndex;
    }

    public AttractVideoPathsResult SetAttractVideoPaths(int currAttractIndex)
    {
        AttractVideoInfo? attract = null;

        if (_attractState.Value.AttractList.Count > 0)
        {
            attract = _attractState.Value.AttractList[currAttractIndex];
        }

        if (_configuration.AlternateAttractModeLanguage)
        {
            var languageIndex = _nextAttractModeLanguageIsPrimary ? 0 : 1;

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

            return new AttractVideoPathsResult
            {
                TopAttractVideoPath = topAttractVideoPath,
                TopperAttractVideoPath = topperAttractVideoPath,
                BottomAttractVideoPath = bottomAttractVideoPath
            };
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

            return new AttractVideoPathsResult
            {
                TopAttractVideoPath = topAttractVideoPath,
                TopperAttractVideoPath = topperAttractVideoPath,
                BottomAttractVideoPath = bottomAttractVideoPath
            };
        }
    }

    public bool PlayAdditionalConsecutiveVideo()
    {
        if (!_configuration.HasAttractIntroVideo || _attractState.Value.CurrentAttractIndex != 0 || _attractState.Value.AttractList.Count <= 1)
        {
            _consecutiveAttractCount++;

            if (_consecutiveAttractCount >= _configuration.ConsecutiveAttractVideos ||
                _consecutiveAttractCount >= _attractState.Value.Games.Count)
            {
                return false;
            }
        }

        return true;
    }

    public void Dispose()
    {
        _attractTimer.Stop();
    }

    private void AttractTimer_Tick(object? sender, EventArgs e)
    {
        if (!_attractConfigurationProvider.IsAttractEnabled ||
            !_attractState.Value.HasZeroCredits() ||
            _attractState.Value.IsIdleTextScrolling ||
            _attractState.Value.IsVoucherNotificationActive ||
            _attractState.Value.IsProgressiveGameDisabledNotificationActive ||
            _attractState.Value.IsPlayerInfoRequestActive)
        {
            return;
        }

        _dispatcher.Dispatch(new AttractEnterAction());
    }
}
