namespace Aristocrat.Monaco.Bingo.UI.ViewModels.GameOverlay
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using CefSharp;
    using Common;
    using Common.Events;
    using Common.GameOverlay;
    using Common.Storage.Model;
    using Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Events;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using Models;
    using Monaco.Common;
    using Monaco.UI.Common.Extensions;
    using MVVM.Model;
    using OverlayServer;
    using OverlayServer.Attributes;
    using OverlayServer.Data.Bingo;
    using Protocol.Common.Storage.Entity;
    using Services;
    using BingoPattern = Common.GameOverlay.BingoPattern;
    using PresentationOverrideTypes = Gaming.Contracts.PresentationOverrideTypes;

    public class BingoHtmlHostOverlayViewModel : BaseNotify, IDisposable
    {
        private const string CloseEvent = "Close";

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
        private readonly ConcurrentDictionary<PresentationOverrideTypes, BingoDisplayConfigurationPresentationOverrideMessageFormat> _configuredOverrideMessageFormats = new();
        private readonly BingoWindow _targetWindow;

        private BingoCard _lastBingoCard;
        private IReadOnlyList<BingoNumber> _lastBallCall = new List<BingoNumber>();
        private IReadOnlyList<BingoPattern> _bingoPatterns = new List<BingoPattern>();
        private IReadOnlyList<BingoPattern> _cyclingPatterns = new List<BingoPattern>();
        private IReadOnlyList<BallCallNumber> _ballCallNumbers = new List<BallCallNumber>();
        private IReadOnlyList<BingoCardNumber> _bingoCardNumbers = new List<BingoCardNumber>();
        private BingoDisplayConfigurationBingoWindowSettings _currentBingoSettings;
        private bool _multipleSpins;
        private double _height;
        private double _width;
        private bool _disposed;
        private OverlayType? _selectedOverlayType;

        private Thickness _helpBoxMargin;
        private bool _isHelpVisible;
        private bool _isInfoVisible;
        private string _bingoHelpAddress;
        private IWebBrowser _bingoHelpWebBrowser;
        private string _bingoInfoAddress;
        private IWebBrowser _bingoInfoWebBrowser;
        private string _standaloneCreditMeterFormat;
        private string _dynamicMessageAddress;
        private string _lastGameScene = string.Empty;
        private string _previousScene = string.Empty;
        private double _helpOpacity;
        private double _infoOpacity;
        private double _gameControlledHeight;
        private double _dynamicMessageOpacity;
        private IGameDetail _lastSelectedGame; 

        public BingoHtmlHostOverlayViewModel(
            IPropertiesManager propertiesManager,
            IDispatcher dispatcher,
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider,
            ILegacyAttractProvider legacyAttractProvider,
            IGameProvider gameProvider,
            IServer overlayServer,
            IPlayerBank playerBank,
            IUnitOfWorkFactory unitOfWorkFactory,
            BingoWindow targetWindow)
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
            _targetWindow = targetWindow;

            _gameControlledHeight = _bingoConfigurationProvider.GetWindow(_targetWindow).Height;

            _overlayServer.ServerStarted += HandleServerStarted;
            _overlayServer.AttractCompleted += AttractCompleted;
            _overlayServer.ClientConnected += OverlayClientConnected;
            _overlayServer.ClientDisconnected += OverlayClientDisconnected;

            // Bingo Info Events
            _eventBus.Subscribe<GameSelectedEvent>(this, Handle);
            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, (_, _) => SetInfoVisibility(true));
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
            _eventBus.Subscribe<WaitingForPlayersCanceledEvent>(this, (_, token) => CancelWaitingForPlayers(token));
            _eventBus.Subscribe<GamePlayDisabledEvent>(this, (_, token) => CancelWaitingForPlayers(token));
            _eventBus.Subscribe<PresentationOverrideDataChangedEvent>(this, Handle);
            _eventBus.Subscribe<ClearBingoDaubsEvent>(this, Handle);
            _eventBus.Subscribe<BingoDisplayConfigurationChangedEvent>(this, (_, _) => HandleBingoDisplayConfigurationChanged());
            _eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, Handle);
            _eventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, Handle);
            _eventBus.Subscribe<GameControlSizeChangedEvent>(this, Handle);

            // Bingo Help Events
            _eventBus.Subscribe<HostConnectedEvent>(this, Handle);
            _eventBus.Subscribe<GameLoadedEvent>(this, (_, _) => SetHelpVisibility(false));
            _eventBus.Subscribe<GameExitedNormalEvent>(this, (_, _) => SetHelpVisibility(false));
            _eventBus.Subscribe<GameFatalErrorEvent>(this, (_, _) => SetHelpVisibility(false));
            _eventBus.Subscribe<GameRequestedPlatformHelpEvent>(this, (e, _) => SetHelpVisibility(e.Visible));
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, Handle);
        }

        public string BingoInfoAddress
        {
            get => _bingoInfoAddress;
            set => SetProperty(ref _bingoInfoAddress, value);
        }

        public string BingoHelpAddress
        {
            get => _bingoHelpAddress;
            set => SetProperty(ref _bingoHelpAddress, value);
        }

        public string DynamicMessageAddress
        {
            get => _dynamicMessageAddress;
            set => SetProperty(ref _dynamicMessageAddress, value);
        }

        public IWebBrowser BingoHelpWebBrowser
        {
            get => _bingoHelpWebBrowser;
            set => SetProperty(ref _bingoHelpWebBrowser, value);
        }

        public IWebBrowser BingoInfoWebBrowser
        {
            get => _bingoInfoWebBrowser;

            set
            {
                var previous = _bingoInfoWebBrowser;
                if (!SetProperty(ref _bingoInfoWebBrowser, value))
                {
                    return;
                }

                if (previous is not null)
                {
                    previous.ConsoleMessage -= BingoInfoWebBrowserOnConsoleMessage;
                }

                if (_bingoInfoWebBrowser is not null)
                {
                    _bingoInfoWebBrowser.ConsoleMessage += BingoInfoWebBrowserOnConsoleMessage;
                }
            }
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public Thickness HelpBoxMargin
        {
            get => _helpBoxMargin;
            private set => SetProperty(ref _helpBoxMargin, value);
        }

        public bool IsHelpVisible
        {
            get => _isHelpVisible;
            set => SetProperty(ref _isHelpVisible, value);
        }

        public bool IsInfoVisible
        {
            get => _isInfoVisible;
            set => SetProperty(ref _isInfoVisible, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void LoadOverlay()
        {
            UpdateAppearance().FireAndForget();
        }

        public double HelpOpacity
        {
            get => _helpOpacity;
            set => SetProperty(ref _helpOpacity, value);
        }

        public double InfoOpacity
        {
            get => _infoOpacity;
            set => SetProperty(ref _infoOpacity, value);
        }

        public double DynamicMessageOpacity
        {
            get => _dynamicMessageOpacity;
            set => SetProperty(ref _dynamicMessageOpacity, value);
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

                if (_bingoInfoWebBrowser is not null)
                {
                    _bingoInfoWebBrowser.ConsoleMessage -= BingoInfoWebBrowserOnConsoleMessage;
                }

                _eventBus.UnsubscribeAll(this);
                _overlayServer.Dispose();
            }

            _disposed = true;
        }

        private static void BingoInfoWebBrowserOnConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Logger.DebugFormat("Bingo Overlay - {0}", e.Message);
        }

        private static double GetVisibleOpacity(bool visible) => visible ? 1.0 : 0.0;

        private static void OverlayClientDisconnected(object sender, ClientDisconnectEventArgs args)
        {
            Logger.Debug($"Overlay client disconnected: {args.Type}, Exception: {(args.Exception == null ? "No Exception": args.Exception)}");
        }

        private static void ReloadBrowser(IWebBrowser browser)
        {
            try
            {
                browser.Reload(true);
            }
            catch (Exception e) // CrefSharp throws Exception and not anything more specific so we must catch a generic exception
            {
                Logger.Error("Failed to reload the web browser", e);
            }
        }

        private void AttractCompleted(object sender, EventArgs e)
        {
            _dispatcher.ExecuteOnUIThread(() =>
            {
                if (BingoInfoAddress is not null && !BingoInfoAddress.Contains(OverlayType.Attract.GetOverlayRoute()))
                {
                    return;
                }

                NavigateToOverlay(OverlayType.BingoOverlay);
            });
        }

        private async Task CancelWaitingForPlayers(CancellationToken token)
        {
            await UpdateOverlay(() => new BingoLiveData { CancelWaitingForGame = true }, token);
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

        public void ExitHelp(object sender, JavascriptMessageReceivedEventArgs args)
        {
            if (args.Message is not CloseEvent)
            {
                return;
            }

            _eventBus.Publish(new ExitHelpEvent());
        }

        private IEnumerable<int> GetBallCallPatternDaubs(int pattern)
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

        private IEnumerable<OverlayServer.Data.Bingo.BingoPattern> GetBingoPatternForOverlay(IEnumerable<BingoPattern> patterns)
        {
            return patterns.Select(x =>
                    new OverlayServer.Data.Bingo.BingoPattern(x.Name, GetBallCallPatternDaubs(x.BitFlags), GetCardPatternDaubs(x.BitFlags))).ToList();
        }

        private IEnumerable<int> GetCardPatternDaubs(int pattern)
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

        private string GetCreditMeterUrl()
        {
            var encodedFormattedBalance = WebUtility.UrlEncode(
                GetFormattedCreditBalance(_standaloneCreditMeterFormat, _playerBank.Balance));
            var formattedQueryString = string.Format(
                OverlayType.CreditMeter.GetFormattedQueryParameters(),
                encodedFormattedBalance);

            var address = new UriBuilder(BingoConstants.BingoOverlayServerUri)
            {
                Path = OverlayType.CreditMeter.GetOverlayRoute(),
                Query = formattedQueryString
            };

            return address.ToString();
        }

        private string GetFormattedCreditBalance(string formatString, long amount)
        {
            var subUnitDigits = CurrencyExtensions.CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits;
            var formattedCredits = amount.MillicentsToDollars().ToString($"C{subUnitDigits}", CurrencyExtensions.CurrencyCultureInfo);
            return string.Format(formatString, formattedCredits);
        }

        private async Task Handle(AttractModeEntered evt, CancellationToken token)
        {
            await _dispatcher.ExecuteAndWaitOnUIThread(() => NavigateToOverlay(OverlayType.Attract));
        }

        private async Task Handle(AttractModeExited evt, CancellationToken token)
        {
            await _dispatcher.ExecuteAndWaitOnUIThread(() =>
            {
                if (BingoInfoAddress is not null && !BingoInfoAddress.Contains(OverlayType.Attract.GetOverlayRoute()))
                {
                    return;
                }

                NavigateToOverlay(OverlayType.BingoOverlay);
            });
        }

        private void Handle(Class2MultipleOutcomeSpinsChangedEvent e)
        {
            Logger.Debug($"multiple spins is {e.Triggered}");
            _multipleSpins = e.Triggered;
        }

        private async Task Handle(GameControlSizeChangedEvent e, CancellationToken token)
        {
            _gameControlledHeight = e.GameControlHeight;
            await UpdateAppearance().ConfigureAwait(false);
        }

        private async Task Handle(GameProcessExitedEvent e, CancellationToken token)
        {
            _configuredOverrideMessageFormats.Clear();

            await SetInfoVisibility(false);
            await _dispatcher.ExecuteAndWaitOnUIThread(() =>
            {
                if (BingoInfoAddress is null || !BingoInfoAddress.Contains(OverlayType.BingoOverlay.GetOverlayRoute()))
                {
                    NavigateToOverlay(OverlayType.BingoOverlay);
                }
            });
        }

        private async Task Handle(GameSelectedEvent e, CancellationToken token)
        {
            _lastSelectedGame = _gameProvider.GetGame(e.GameId);
            await SetInfoVisibility(false);
            await InitializeOverlay(_lastSelectedGame);
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
            _lastGameScene = scene.Scene;
        }

        private async Task Handle(GameDiagnosticsStartedEvent evt, CancellationToken token)
        {
            var windowSettings = _bingoConfigurationProvider.GetSettings(_targetWindow, evt.GameId);
            _previousScene = string.IsNullOrEmpty(_lastGameScene)
                ? windowSettings.InitialScene
                : _lastGameScene;

            if (_overlayServer.IsRunning)
            {
                await UpdateOverlay(() => new BingoLiveData { SceneName = windowSettings.InitialScene }, token);
            }
        }

        private async Task Handle(GameDiagnosticsCompletedEvent evt, CancellationToken token)
        {
            if (string.IsNullOrEmpty(_previousScene))
            {
                return;
            }

            await UpdateOverlay(() => new BingoLiveData { SceneName = _previousScene }, token);
            _lastGameScene = _previousScene;
            _previousScene = string.Empty;
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
#if !(RETAIL)
            _eventBus.Publish(new BingoPatternsInfoEvent(GetBingoPatternForOverlay(patterns)));
#endif
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
                var daubs = GetCardPatternDaubs(outcome.BitFlags);
                var numbers = GetBallCallPatternDaubs(outcome.BitFlags);
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
#if !(RETAIL)
                _eventBus.Publish(new BingoPatternsInfoEvent(GetBingoPatternForOverlay(_bingoPatterns)));
#endif

                _cyclingPatterns = new List<BingoPattern>(_cyclingPatterns.Concat(_bingoPatterns));
                _bingoPatterns = new List<BingoPattern>();
            }
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
            _cyclingPatterns = new List<BingoPattern>();
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

        private async Task Handle(PresentationOverrideDataChangedEvent e, CancellationToken token)
        {
            var data = e.PresentationOverrideData;
            if (data == null || data.Count == 0)
            {
                await _dispatcher.ExecuteAndWaitOnUIThread(() =>
                {
                    DynamicMessageOpacity = GetVisibleOpacity(false);
                    NavigateToDynamicMessage(string.Empty, BingoConstants.DefaultInitialOverlayScene, string.Empty, false, false);
                });
            }
            else
            {
                var overrideType = data.First().Type;
                if (!_configuredOverrideMessageFormats.ContainsKey(overrideType))
                {
                    return;
                }

                var configuration = _configuredOverrideMessageFormats[overrideType];
                var message = string.Format(configuration.MessageFormat, data.First().FormattedAmount ?? string.Empty);
                var meterMessage = GetFormattedCreditBalance(configuration.MeterFormat, _playerBank.Balance);
                var showDynamicMessage = !message.IsEmpty();
                var showMeter = !meterMessage.IsEmpty();
                await _dispatcher.ExecuteAndWaitOnUIThread(() =>
                {
                    NavigateToDynamicMessage(message, configuration.MessageScene, meterMessage, showDynamicMessage, showMeter);
                    DynamicMessageOpacity = GetVisibleOpacity(true);
                });
            }
        }

        private async Task Handle(HostConnectedEvent e, CancellationToken token)
        {
            var helpAddress = _unitOfWorkFactory.GetHelpUri(_gameProvider).ToString();
            await _dispatcher.ExecuteAndWaitOnUIThread(() => BingoHelpAddress = helpAddress);
        }

        private async Task Handle(BankBalanceChangedEvent e, CancellationToken token)
        {
            await _dispatcher.ExecuteAndWaitOnUIThread(
                () =>
                {
                    if (IsHelpVisible)
                    {
                        NavigateToOverlay(OverlayType.CreditMeter);
                    }
                });
        }

        private async Task HandleBingoDisplayConfigurationChanged()
        {
            if (!_overlayServer.IsRunning)
            {
                return;
            }

            var (game, _) = _gameProvider.GetActiveGame();
            if (game != null)
            {
                _lastSelectedGame = game;
            }

            Logger.Debug("Restarting the bingo overlay server as the settings have changed");
            await UpdateAppearance();
            await _overlayServer.StopAsync();
            await InitializeOverlay(_lastSelectedGame);
            await SetInfoVisibility(true);
        }

        private async Task InitializeOverlay(IGameDetail detail)
        {
            await UpdateAppearance();
            LoadPresentationOverrideMessageFormats();

            if (!_overlayServer.IsRunning && detail is not null)
            {
                var windowName = _bingoConfigurationProvider.CurrentWindow;
                _currentBingoSettings = _bingoConfigurationProvider.GetSettings(windowName);
                var attractSettings = _bingoConfigurationProvider.GetAttractSettings();
                _lastGameScene = _currentBingoSettings.InitialScene;

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

                Logger.Debug("Starting overlay server");
                await _overlayServer.StartAsync(detail.Folder, new Uri(BingoConstants.BingoOverlayServerUri), staticData);
            }
        }

        private async Task HandleNoPlayersFound(NoPlayersFoundEvent evt, CancellationToken token)
        {
            await UpdateOverlay(() => new BingoLiveData { StartNoGameFound = true }, token);
        }

        private void HandleServerStarted(object sender, EventArgs e)
        {
            _dispatcher.ExecuteOnUIThread(() =>
            {
                NavigateToOverlay(OverlayType.BingoOverlay);
                ReloadBrowser(BingoInfoWebBrowser);
            });
        }

        private bool IsNewBallCall(IReadOnlyCollection<BingoNumber> incomingBallCall)
        {
            return _lastBallCall.Count > incomingBallCall.Count ||
                !_lastBallCall.SequenceEqual(incomingBallCall.Take(_lastBallCall.Count));
        }

        private void LoadPresentationOverrideMessageFormats()
        {
            var configurations = _bingoConfigurationProvider.GetPresentationOverrideMessageFormats();
            if (configurations == null)
            {
                return;
            }

            foreach (var configuration in configurations.Where(x => !string.IsNullOrEmpty(x.MessageFormat)))
            {
                _configuredOverrideMessageFormats.TryAdd(
                    (PresentationOverrideTypes)configuration.OverrideType,
                    new BingoDisplayConfigurationPresentationOverrideMessageFormat
                    {
                        MessageFormat = configuration.MessageFormat,
                        MeterFormat = configuration.MeterFormat ?? string.Empty,
                        MessageScene = configuration.MessageScene ?? string.Empty
                    });
            }
        }

        private void NavigateToDynamicMessage(string message, string scene, string meterMessage, bool showMessage, bool showMeter)
        {
            var formattedQueryString = string.Format(
                OverlayType.DynamicMessageOverlay.GetFormattedQueryParameters(),
                message, scene, meterMessage, showMessage, showMeter);
            DynamicMessageAddress = new UriBuilder(BingoConstants.BingoOverlayServerUri)
            {
                Path = OverlayType.DynamicMessageOverlay.GetOverlayRoute(),
                Query = formattedQueryString
            }.ToString();
        }

        private void NavigateToOverlay(OverlayType overlayType)
        {
            Logger.Debug($"Navigating the bingo overlay to {overlayType}");
            var isCreditOverlayDisplayed = BingoInfoAddress is not null &&
                                           BingoInfoAddress.Contains(OverlayType.CreditMeter.GetOverlayRoute());

            BingoInfoAddress = overlayType switch
            {
                OverlayType.Attract when !isCreditOverlayDisplayed => GetAttractOverlay(),
                OverlayType.BingoOverlay when _selectedOverlayType != OverlayType.BingoOverlay => GetBingoOverlay(),
                OverlayType.CreditMeter => GetCreditMeterUrl(),
                _ => BingoInfoAddress
            };

            _selectedOverlayType = overlayType;
        }

        private string GetAttractOverlay() =>
            _legacyAttractProvider.GetLegacyAttractUri(_bingoConfigurationProvider.GetAttractSettings())
                ?.ToString() ?? BingoInfoAddress;

        private string GetBingoOverlay() =>
            new UriBuilder(BingoConstants.BingoOverlayServerUri) { Path = OverlayType.BingoOverlay.GetOverlayRoute() }
                .ToString();

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
                    BingoPatterns = GetBingoPatternForOverlay(_cyclingPatterns),
                    SceneName = _lastGameScene
                }).FireAndForget();
#if !(RETAIL)
            _eventBus.Publish(new BingoPatternsInfoEvent(GetBingoPatternForOverlay(_cyclingPatterns)));
#endif
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

        private async Task SetHelpVisibility(bool visible)
        {
            var helpAddress = _unitOfWorkFactory.GetHelpUri(_gameProvider).ToString();
            await _dispatcher.ExecuteAndWaitOnUIThread(
                () =>
                {
                    IsHelpVisible = visible;
                    BingoHelpAddress = helpAddress;
                    NavigateToOverlay(visible ? OverlayType.CreditMeter : OverlayType.BingoOverlay);
                    HelpOpacity = GetVisibleOpacity(visible);
                });
        }

        private async Task SetInfoVisibility(bool visible)
        {
            await _dispatcher.ExecuteAndWaitOnUIThread(() =>
            {
                IsInfoVisible = visible;
                InfoOpacity = GetVisibleOpacity(visible);
            });
        }

        private async Task UpdateAppearance()
        {
            var window = _bingoConfigurationProvider.GetWindow(_targetWindow);
            var helpAppearance = _bingoConfigurationProvider.GetHelpAppearance();
            _standaloneCreditMeterFormat = helpAppearance.CreditMeterFormat ?? string.Empty;

            await _dispatcher.ExecuteAndWaitOnUIThread(
                () =>
                {
                    Width = window.Width;
                    Height = _gameControlledHeight;
                    HelpBoxMargin = new Thickness(
                        helpAppearance.HelpBox.Left * window.Width,
                        helpAppearance.HelpBox.Top * window.Height,
                        helpAppearance.HelpBox.Right * window.Width,
                        helpAppearance.HelpBox.Bottom * window.Height);
                });
        }

        private async Task UpdateOverlay(Func<BingoLiveData> updaterFunction, CancellationToken token = default)
        {
            var updates = updaterFunction();
            await _overlayServer.UpdateData(updates, token);
        }
    }
}
