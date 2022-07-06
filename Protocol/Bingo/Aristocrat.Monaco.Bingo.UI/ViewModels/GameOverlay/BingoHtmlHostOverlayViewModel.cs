﻿namespace Aristocrat.Monaco.Bingo.UI.ViewModels.GameOverlay
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using CefSharp;
    using Common;
    using Common.Events;
    using Common.GameOverlay;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Events;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Monaco.Common;
    using MVVM.Model;
    using OverlayServer;
    using OverlayServer.Attributes;
    using OverlayServer.Data.Bingo;
    using Protocol.Common.Storage.Entity;
    using Services;
    using BingoPattern = Common.GameOverlay.BingoPattern;

    public class BingoHtmlHostOverlayViewModel : BaseNotify, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IPropertiesManager _propertiesManager;
        private readonly IDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly IBingoDisplayConfigurationProvider _bingoConfigurationProvider;
        private readonly ILegacyAttractProvider _legacyAttractProvider;
        private readonly IGameProvider _gameProvider;
        private readonly IServer _overlayServer;
        private readonly IPlayerBank _playerBank;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly Stopwatch _stopwatch = new();
        private readonly ConcurrentDictionary<PresentationOverrideTypes, (string, string)> _configuredOverrideMessageFormats = new();
        private BingoCard _lastBingoCard;
        private IReadOnlyList<BingoNumber> _lastBallCall = new List<BingoNumber>();
        private IReadOnlyList<BingoPattern> _bingoPatterns = new List<BingoPattern>();
        private IReadOnlyList<BingoPattern> _cyclingPatterns = new List<BingoPattern>();
        private IReadOnlyList<BallCallNumber> _ballCallNumbers = new List<BallCallNumber>();
        private IReadOnlyList<BingoCardNumber> _bingoCardNumbers = new List<BingoCardNumber>();
        private BingoDisplayConfigurationBingoWindowSettings _currentBingoSettings;
        private bool _multipleSpins;
        private bool _previouslyVisible;

        private bool _disposed;
        private string _address;
        private bool _visible;
        private IWebBrowser _webBrowser;

        public BingoHtmlHostOverlayViewModel(
            IPropertiesManager propertiesManager,
            IDispatcher dispatcher,
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider,
            ILegacyAttractProvider legacyAttractProvider,
            IGameProvider gameProvider,
            IServer overlayServer,
            IPlayerBank playerBank,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bingoConfigurationProvider = bingoConfigurationProvider ?? throw new ArgumentNullException(nameof(bingoConfigurationProvider));
            _legacyAttractProvider = legacyAttractProvider ?? throw new ArgumentNullException(nameof(legacyAttractProvider));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _overlayServer = overlayServer ?? throw new ArgumentNullException(nameof(overlayServer));
            _playerBank = playerBank ?? throw new ArgumentNullException(nameof(playerBank));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _overlayServer.ServerStarted += HandleServerStarted;
            _overlayServer.AttractCompleted += AttractCompleted;
            _overlayServer.ClientConnected += OverlayClientConnected;
            _overlayServer.ClientDisconnected += OverlayClientDisconnected;

            _eventBus.Subscribe<GameConnectedEvent>(this, (_, _) => HandleGameLoaded());
            _eventBus.Subscribe<GameProcessExitedEvent>(this, Handle);
            _eventBus.Subscribe<BingoGameBallCallEvent>(this, Handle);
            _eventBus.Subscribe<BingoGameNewCardEvent>(this, Handle);
            _eventBus.Subscribe<SceneChangedEvent>(this, Handle);
            _eventBus.Subscribe<GamePlayInitiatedEvent>(this, Handle);
            _eventBus.Subscribe<BingoGamePatternEvent>(this, Handle);
            _eventBus.Subscribe<GamePresentationEndedEvent>(
                this,
                Handle,
                _ => _currentBingoSettings is null or { PatternDaubTime: BingoDaubTime.PresentationEnd });
            _eventBus.Subscribe<GamePresentationStartedEvent>(
                this,
                Handle,
                _ => _currentBingoSettings is { PatternDaubTime: BingoDaubTime.PresentationStart });
            _eventBus.Subscribe<GameWinPresentationStartedEvent>(
                this,
                Handle,
                _ => _currentBingoSettings is { PatternDaubTime: BingoDaubTime.WinPresentationStart });
            _eventBus.Subscribe<Class2MultipleOutcomeSpinsChangedEvent>(this, Handle);
            _eventBus.Subscribe<AttractModeEntered>(this, Handle);
            _eventBus.Subscribe<AttractModeExited>(this, Handle);
            _eventBus.Subscribe<WaitingForPlayersEvent>(this, Handle);
            _eventBus.Subscribe<NoPlayersFoundEvent>(this, HandleNoPlayersFound);
            _eventBus.Subscribe<PlayersFoundEvent>(this, (_, token) => CancelWaitingForPlayers(token));
            _eventBus.Subscribe<GamePlayDisabledEvent>(this, (_, token) => CancelWaitingForPlayers(token));
            _eventBus.Subscribe<PresentationOverrideDataChangedEvent>(this, Handle);
            _eventBus.Subscribe<GameRequestedPlatformHelpEvent>(this, Handle);
            _eventBus.Subscribe<ClearBingoDaubsEvent>(this, Handle);
        }

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public IWebBrowser WebBrowser
        {
            get => _webBrowser;
            set => SetProperty(ref _webBrowser, value);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _overlayServer.ServerStarted -= HandleServerStarted;
                _overlayServer.AttractCompleted -= AttractCompleted;
                _overlayServer.ClientConnected -= OverlayClientConnected;
                _overlayServer.ClientDisconnected -= OverlayClientDisconnected;
                _overlayServer.Dispose();
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void AttractCompleted(object sender, EventArgs e)
        {
            if (Address is not null && !Address.Contains(OverlayType.Attract.GetOverlayRoute()))
            {
                return;
            }

            HandleServerStarted(this, null);
        }

        private void Handle(GameRequestedPlatformHelpEvent evt)
        {
            if (evt.Visible)
            {
                var currentVisibility = Visible;
                SetVisibility(false);
                _previouslyVisible = currentVisibility;
            }
            else
            {
                SetVisibility(_previouslyVisible);
            }
        }

        private void Handle(AttractModeEntered evt)
        {
            var attractUri =
                _legacyAttractProvider.GetLegacyAttractUri(_bingoConfigurationProvider.GetAttractSettings());
            if (attractUri is null)
            {
                return;
            }

            Address = attractUri.ToString();
        }

        private void Handle(AttractModeExited evt)
        {
            if (Address is not null && !Address.Contains(OverlayType.Attract.GetOverlayRoute()))
            {
                return;
            }

            HandleServerStarted(this, null);
        }

        private async Task HandleGameLoaded()
        {
            LoadPresentationOverrideMessageFormats();
            if (!_overlayServer.IsRunning)
            {
                var windowName = _bingoConfigurationProvider.CurrentWindow;
                _currentBingoSettings = _bingoConfigurationProvider.GetSettings(windowName);
                var attractSettings = _bingoConfigurationProvider.GetAttractSettings();

                var staticData = new BingoStaticData
                {
                    AttractScene = attractSettings.OverlayScene,
                    BallCallTitle = _currentBingoSettings.BallCallTitle,
                    BingoCardTitle = _currentBingoSettings.CardTitle,
                    DisclaimerMessages = _currentBingoSettings.DisclaimerText,
                    GameCssFile = _currentBingoSettings.CssPath,
                    InitialScene = _currentBingoSettings.InitialScene,
                    InitialBallCallNumbers = ConvertToBallCallNumber(_lastBallCall).ToList(),
                    InitialBingoCardNumbers =
                        _lastBingoCard is null
                            ? Enumerable.Empty<BingoCardNumber>()
                            : ConvertBingoCardNumberArrayToList(_lastBingoCard.Numbers).ToList(),
                    PatternCycleTime = _currentBingoSettings.PatternCyclePeriod
                };

                var currentGame = _gameProvider.GetGame(_propertiesManager.GetValue(GamingConstants.SelectedGameId, 0));
                Logger.Debug("Starting overlay server");
                await _overlayServer.StartAsync(currentGame.Folder, new Uri(BingoConstants.BingoOverlayServerUri), staticData);
            }

            SetVisibility(true);
        }

        private async Task Handle(BingoGameBallCallEvent e, CancellationToken token)
        {
            if (IsNewBallCall(e.BallCall.Numbers))
            {
                Logger.Debug("Clearing ball call on overlay for a new game");
                await UpdateOverlay(() => new BingoLiveData { ClearBallCall = true }, token);
            }

            var diff = (_currentBingoSettings?.MinimumPreDaubedTimeMs ?? 0) - _stopwatch.ElapsedMilliseconds;
            if (diff > 0 && !e.IsRecovery)
            {
                Logger.Debug($"Adding artificial daub delay to the bingo card and ball call: {diff}ms");
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(BingoConstants.MaxPreDaubedTimeMs, diff)), token);
            }

            await UpdateOverlay(
                () =>
                {
                    _lastBallCall = e.BallCall.Numbers;
                    _ballCallNumbers = ConvertToBallCallNumber(e.BallCall.Numbers).ToList();
                    var daubs = Convert.ToString(e.Daubs, 2).PadLeft(32, '0');
                    Logger.Debug($"Daubing bingo card with {daubs}");
                    DaubBingoCard(e.Daubs);
                    return new BingoLiveData
                    {
                        BallCallNumbers = _ballCallNumbers,
                        BingoCardNumbers = _bingoCardNumbers
                    };
                },
                token);
        }

        private IEnumerable<BallCallNumber> ConvertToBallCallNumber(IEnumerable<BingoNumber> numbers)
        {
            var index = 0;
            var allow0PaddingBallCall = _currentBingoSettings?.Allow0PaddingBallCall ?? false;
            foreach (var ball in numbers)
            {
                yield return new BallCallNumber(
                    ++index,
                    allow0PaddingBallCall ? ball.Number.ToString("D2") : ball.Number.ToString(),
                    ball.State == BingoNumberState.BallCallInitial
                        ? BallCallNumber.DropState.Initial
                        : BallCallNumber.DropState.LateBall);
            }
        }

        private async Task Handle(BingoGameNewCardEvent card, CancellationToken token)
        {
            await UpdateOverlay(
                () =>
                {
                    _ballCallNumbers = new List<BallCallNumber>();
                    _lastBingoCard = card.BingoCard;
                    _bingoCardNumbers = new List<BingoCardNumber>();
                    _bingoPatterns = new List<BingoPattern>();
                    _cyclingPatterns = new List<BingoPattern>();
                    return new BingoLiveData { ClearBingoCard = true };
                },
                token);

            await UpdateOverlay(
                () =>
                {
                    _bingoCardNumbers = ConvertBingoCardNumberArrayToList(card.BingoCard.Numbers).ToList();
                    Logger.Debug("Sending new bingo card numbers to overlay");
                    return new BingoLiveData { BingoCardNumbers = _bingoCardNumbers };
                },
                token);

            SaveDaubState(true);
            _stopwatch.Restart();
        }

        private async Task Handle(SceneChangedEvent scene, CancellationToken token)
        {
            Logger.Debug($"Sending scene changed to '{scene.Scene}' to overlay");
            await UpdateOverlay(() => new BingoLiveData { SceneName = scene.Scene }, token);
        }

        private async Task Handle(GamePlayInitiatedEvent e, CancellationToken token)
        {
            Logger.Debug("Clearing bingo card on overlay for a new game");
            await UpdateOverlay(
                () =>
                {
                    _bingoCardNumbers = new List<BingoCardNumber>();
                    return new BingoLiveData { ClearBingoCard = true };
                },
                token);
        }

        private async Task Handle(BingoGamePatternEvent e, CancellationToken token)
        {
            Logger.Debug($"Got {e.Patterns.Count} patterns with start pattern cycle {e.StartPatternCycle}");

            if (e.Patterns.Count == 0) // no wins
            {
                _cyclingPatterns = new List<BingoPattern>();
                _bingoPatterns = new List<BingoPattern>();
                return;
            }

            // get patterns that aren't the game end win pattern
            var patterns = e.Patterns.Where(x => !x.IsGameEndWin).ToList();
            if (!e.StartPatternCycle)
            {
                _cyclingPatterns = new List<BingoPattern>();
                _bingoPatterns = new List<BingoPattern>(patterns);
                return;
            }

            _bingoPatterns = new List<BingoPattern>();
            _cyclingPatterns = new List<BingoPattern>(patterns);
            await _overlayServer.UpdateData(
                new BingoLiveData { BingoPatterns = GetBingoPatternForOverlay(patterns) },
                token);
        }

        private async Task Handle(BaseGameEvent e, CancellationToken token)
        {
            Logger.Debug($"GamePresentationEndedEvent with {e.Log.Outcomes.Count()} outcomes.");

            // we get this event after each reel spin completes for an outcome.
            // It contains all the outcomes, even if we haven't shown the player the outcome yet.
            // if there are multiple spins, take one outcome from the beginning of the patterns list and
            // convert it to the format the overlay needs
            // if wins are coalesced then send all the wins

            if (_bingoPatterns.Count == 0)
            {
                return;
            }

            if (_multipleSpins)
            {
                Logger.Debug("Sending single outcome");
                var outcome = _bingoPatterns.First();

                Logger.Debug(
                    $"Name={outcome.Name} daub bits={outcome.BitFlags} win={outcome.WinAmount} gew={outcome.IsGameEndWin}");
                var daubs = CardPatternDaubs(outcome.BitFlags);
                var numbers = BallCallPatternDaubs(outcome.BitFlags);
                await _overlayServer.UpdateData(
                    new BingoLiveData
                    {
                        BingoPatterns = new List<OverlayServer.Data.Bingo.BingoPattern>
                        {
                            new(outcome.Name, numbers, daubs)
                        }
                    }, token);
                _bingoPatterns = _bingoPatterns.Where(x => !Equals(x, outcome)).ToList();
                _cyclingPatterns = new List<BingoPattern>(_cyclingPatterns.Append(outcome));
            }
            else
            {
                Logger.Debug("sending all the outcomes");

                await _overlayServer.UpdateData(
                    new BingoLiveData { BingoPatterns = GetBingoPatternForOverlay(_bingoPatterns) }, token);

                _cyclingPatterns = new List<BingoPattern>(_cyclingPatterns.Concat(_bingoPatterns));
                _bingoPatterns = new List<BingoPattern>();
            }
        }

        private void Handle(Class2MultipleOutcomeSpinsChangedEvent e)
        {
            Logger.Debug($"multiple spins is {e.Triggered}");
            _multipleSpins = e.Triggered;
        }

        private async Task Handle(WaitingForPlayersEvent e, CancellationToken token)
        {
            var waitSettings = new WaitForGameSettings
            {
                WaitTimeSeconds = e.WaitingDuration.Seconds,
                StartDateTimeUtc = e.StartTimeUtc,
                WaitTimeDelaySeconds = _currentBingoSettings?.WaitingForGameDelaySeconds ?? BingoConstants.DefaultDelayStartWaitingForPlayersSeconds,
                FailureTimeSeconds = _currentBingoSettings?.WaitingForGameTimeoutDisplaySeconds ?? BingoConstants.DefaultNoPlayersFoundSeconds,
                WaitMessage = _currentBingoSettings?.WaitingForGameMessage ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.WaitingForPlayersText),
                FailureMessage = _currentBingoSettings?.WaitingForGameTimeoutMessage ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.NoPlayersFoundText)
            };

            await UpdateOverlay(() => new BingoLiveData { WaitForGameSettings = waitSettings }, token);
        }

        private async Task Handle(ClearBingoDaubsEvent evt, CancellationToken token)
        {
            if (!_currentBingoSettings.ClearDaubsOnBetChange)
            {
                return;
            }

            _bingoPatterns = new List<BingoPattern>();
            _bingoCardNumbers = _bingoCardNumbers.Select(
                x => new BingoCardNumber(x.Position, x.Value, BingoCardNumber.DaubState.NoDaub)).ToList();
            await UpdateOverlay(
                () => new BingoLiveData
                {
                    ClearBingoPatterns = true,
                    ClearBallCall = true,
                    ClearBingoCard = true,
                    BingoCardNumbers = _bingoCardNumbers,
                    BallCallNumbers = _ballCallNumbers
                },
                token);

            SaveDaubState(false);
        }

        private void SaveDaubState(bool state)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();
            var repository = unitOfWork.Repository<BingoDaubsModel>();
            var daubsModel = repository.Queryable().SingleOrDefault() ?? new BingoDaubsModel();
            daubsModel.CardIsDaubed = state;
            repository.AddOrUpdate(daubsModel);
            unitOfWork.SaveChanges();
        }

        private async Task Handle(PresentationOverrideDataChangedEvent e, CancellationToken token)
        {
            var data = e.PresentationOverrideData;
            if (data == null || data.Count == 0)
            {
                await UpdateOverlay(() => new BingoLiveData { HideDynamicMessage = true, HideMeterValue = true }, token);
            }
            else
            {
                var overrideType = data.First().Type;
                if (!_configuredOverrideMessageFormats.ContainsKey(overrideType))
                {
                    return;
                }

                var messageFormat = _configuredOverrideMessageFormats[overrideType];
                var message = string.Format(messageFormat.Item1, data.First().FormattedAmount ?? string.Empty);
                var subUnitDigits = CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits;
                var meterMessage = string.Format(
                    messageFormat.Item2,
                    _playerBank.Balance.MillicentsToDollars().ToString(
                        $"C{subUnitDigits}",
                        CurrencyExtensions.CurrencyCultureInfo));

                await UpdateOverlay(() => new BingoLiveData { DynamicMessage = message, MeterValue = meterMessage }, token);
            }
        }

        private async Task UpdateOverlay(Func<BingoLiveData> updaterFunction, CancellationToken token = default)
        {
            var updates = updaterFunction();
            await _overlayServer.UpdateData(updates, token);
        }

        private void Handle(GameProcessExitedEvent e)
        {
            if (Address is not null && !Address.Contains(OverlayType.Attract.GetOverlayRoute()))
            {
                return;
            }

            _configuredOverrideMessageFormats.Clear();
            SetVisibility(false);

            HandleServerStarted(this, null);
        }

        private async Task HandleNoPlayersFound(NoPlayersFoundEvent evt, CancellationToken token)
        {
            await UpdateOverlay(() => new BingoLiveData { StartNoGameFound = true }, token);
        }

        private async Task CancelWaitingForPlayers(CancellationToken token)
        {
            await UpdateOverlay(() => new BingoLiveData { CancelWaitingForGame = true }, token);
        }

        private IEnumerable<OverlayServer.Data.Bingo.BingoPattern> GetBingoPatternForOverlay(IEnumerable<BingoPattern> patterns)
        {
            return patterns.Select(x =>
                    new OverlayServer.Data.Bingo.BingoPattern(x.Name, BallCallPatternDaubs(x.BitFlags), CardPatternDaubs(x.BitFlags))).ToList();
        }

        private void DaubBingoCard(int daubs)
        {
            // daub bingo card numbers based on daub pattern encoded in an integer.
            var daubed = new BitArray(new[] { daubs });
            for (var i = 0; i < _bingoCardNumbers.Count; i++)
            {
                _bingoCardNumbers[i].State =
                    daubed[i] ? BingoCardNumber.DaubState.NonPatternDaub : BingoCardNumber.DaubState.NoDaub;
            }
        }

        /// <summary>
        ///     Gets a list of positions of numbers on the bingo card in a pattern
        /// </summary>
        /// <param name="pattern">The pattern</param>
        /// <returns>a list of positions of the numbers on the card</returns>
        private IEnumerable<int> CardPatternDaubs(int pattern)
        {
            var binary = Convert.ToString(pattern, 2).PadLeft(32, '0');
            Logger.Debug($"card daub pattern is 0b{binary}");
            var result = new List<int>();
            var daubed = new BitArray(new[] { pattern });
            for (var i = 0; i < _bingoCardNumbers.Count; i++)
            {
                if (!daubed[i])
                {
                    continue;
                }

                Logger.Debug($"CardPatternDaubs adding position {i} for number {_bingoCardNumbers[i].Value}");
                result.Add(i + 1);
            }

            return result;
        }

        /// <summary>
        ///     Gets the position of numbers in the ball call to highlight for a pattern
        /// </summary>
        /// <param name="pattern">The pattern</param>
        /// <returns>A list of positions of numbers to highlight</returns>
        private IEnumerable<int> BallCallPatternDaubs(int pattern)
        {
            var binary = Convert.ToString(pattern, 2).PadLeft(32, '0');
            Logger.Debug($"daub pattern is 0b{binary}");
            var result = new List<int>();
            var daubed = new BitArray(new[] { pattern });
            for (var i = 0; i < _bingoCardNumbers.Count; i++)
            {
                if (!daubed[i])
                {
                    continue;
                }

                var number = _bingoCardNumbers[i].Value;
                var position = _ballCallNumbers.FirstOrDefault(x => x.Value == number)?.Position;
                if (position.HasValue)
                {
                    Logger.Debug($"BallCallPatternDaubs adding position {position.Value} for number {number}");
                    result.Add(position.Value);
                }
                else
                {
                    Logger.Debug("position is unknown");
                }
            }

            return result;
        }

        private IEnumerable<BingoCardNumber> ConvertBingoCardNumberArrayToList(BingoNumber[,] numbers)
        {
            var position = 0;
            for (var row = 0; row < BingoConstants.BingoCardDimension; row++)
            {
                for (var col = 0; col < BingoConstants.BingoCardDimension; col++)
                {
                    // check for free space position and override symbol
                    var number = _currentBingoSettings?.Allow0PaddingBingoCard ?? false ?
                            numbers[row, col].Number.ToString("D2") : numbers[row, col].Number.ToString();
                    if (position == BingoConstants.FreeSpaceIndex)
                    {
                        number = _currentBingoSettings?.FreeSpaceCharacter ?? BingoConstants.DefaultFreeSpaceCharacter;
                    }

                    position++;
                    yield return new BingoCardNumber(position, number, BingoCardNumber.DaubState.NoDaub);
                }
            }
        }

        private void HandleServerStarted(object sender, EventArgs e)
        {
            Address = new UriBuilder(BingoConstants.BingoOverlayServerUri)
            {
                Path = OverlayType.BingoOverlay.GetOverlayRoute()
            }.ToString();
        }

        private void OverlayClientConnected(object sender, OverlayType overlayType)
        {
            Logger.Debug($"Overlay client connected: {overlayType}");
            if (overlayType is not OverlayType.BingoOverlay)
            {
                return;
            }

            UpdateOverlay(
                () => new BingoLiveData
                {
                    BallCallNumbers = _ballCallNumbers,
                    BingoCardNumbers = _bingoCardNumbers,
                    BingoPatterns = GetBingoPatternForOverlay(_cyclingPatterns)
                }).FireAndForget();
        }

        private void OverlayClientDisconnected(object sender, OverlayType overlayType)
        {
            Logger.Debug($"Overlay client disconnected: {overlayType}");
            if (Address is not null && !Address.Contains(overlayType.GetOverlayRoute()))
            {
                return;
            }

            _dispatcher.Invoke(ReloadPage);
        }

        private void ReloadPage()
        {
            if (!Visible)
            {
                return;
            }

            try
            {
                WebBrowser?.Reload();
            }
            catch (Exception ex)
            {
                // CefSharp throws a general exception for things so we must catch Exception
                Logger.Warn("Failed to reload the browser", ex);
            }
        }

        private void SetVisibility(bool visible)
        {
            _dispatcher.ExecuteOnUIThread(() => Visible = visible);
            _previouslyVisible = visible;
        }

        private bool IsNewBallCall(IReadOnlyCollection<BingoNumber> incomingBallCall)
        {
            return _lastBallCall.Count > incomingBallCall.Count ||
                !_lastBallCall.SequenceEqual(incomingBallCall.Take(_lastBallCall.Count));
        }

        private void LoadPresentationOverrideMessageFormats()
        {
            var messageFormats = _bingoConfigurationProvider.GetPresentationOverrideMessageFormats();
            if (messageFormats == null)
            {
                return;
            }

            foreach (var messageFormat in messageFormats.Where(
                         messageFormat => !string.IsNullOrEmpty(messageFormat.MessageFormat)))
            {
                _configuredOverrideMessageFormats.TryAdd(
                    (PresentationOverrideTypes)messageFormat.OverrideType,
                    (messageFormat.MessageFormat, messageFormat.MeterFormat ?? string.Empty));
            }
        }
    }
}
