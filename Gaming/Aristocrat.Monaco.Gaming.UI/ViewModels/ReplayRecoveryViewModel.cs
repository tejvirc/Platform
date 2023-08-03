namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Timers;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Commands;
    using Contracts;
    using Diagnostics;
    using Hardware.Contracts.Button;
    using Kernel;
    using Localization.Properties;
    using MVVM.Command;
    using MVVM.ViewModel;

    /// <summary>
    ///     Helper class to handle replay/recovering screen of the lobby ViewModel.
    /// </summary>
    public class ReplayRecoveryViewModel : BaseEntityViewModel, IDisposable
    {
        private const double CashOutMessagesCycleIntervalInSeconds = 3.0;

        private readonly IEventBus _eventBus;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IPropertiesManager _properties;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        private double _backgroundOpacity = 0.2;
        private string _cashoutText;

        private bool _disposed;
        private bool _isReplayNavigationVisible;
        private string _label;
        private string _messageText;
        private string _replayGameName;
        private string _replayPauseMessageText;
        private long? _replaySequence;
        private DateTime? _replayStartTime;
        private long _replayEndCredits;
        private Timer _cashoutMessageTimer;
        private int _currentCashoutMessageIndex;
        private readonly List<string> _cashOutTexts = new List<string>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReplayRecoveryViewModel" /> class
        /// </summary>
        /// <param name="eventBus">Event bus.</param>
        /// <param name="gameDiagnostics">Game diagnostics interface.</param>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="handlerFactory">The command handler factory</param>
        public ReplayRecoveryViewModel(
            IEventBus eventBus,
            IGameDiagnostics gameDiagnostics,
            IPropertiesManager propertiesManager,
            ICommandHandlerFactory handlerFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameDiagnostics = gameDiagnostics ?? throw new ArgumentNullException(nameof(gameDiagnostics));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _commandHandlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));

            ExitCommand = new ActionCommand<object>(ExitButtonPressed);
            ContinueCommand = new ActionCommand<object>(ContinueButtonPressed);
            _cashoutMessageTimer = new Timer(TimeSpan.FromSeconds(CashOutMessagesCycleIntervalInSeconds).TotalMilliseconds);
            _cashoutMessageTimer.Elapsed += CashOutMessageCycleTimerTick;

            SubscribeToEvents();
        }

        /// <summary>
        ///     Gets the exit command.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        ///     Gets the continue command.
        /// </summary>
        public ICommand ContinueCommand { get; }

        /// <summary>
        ///     Gets or sets a value indicating whether the replay navigation is visible.
        /// </summary>
        public bool IsReplayNavigationVisible
        {
            get => _isReplayNavigationVisible;

            set
            {
                if (_isReplayNavigationVisible == value)
                {
                    return;
                }

                _isReplayNavigationVisible = value;
                RaisePropertyChanged(nameof(IsReplayNavigationVisible));
            }
        }

        /// <summary>
        ///     Gets or sets the background opacity.
        /// </summary>
        public double BackgroundOpacity
        {
            get => _backgroundOpacity;

            set
            {
                if (!(Math.Abs(_backgroundOpacity - value) >= 0.001))
                {
                    return;
                }

                _backgroundOpacity = value;
                RaisePropertyChanged(nameof(BackgroundOpacity));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the replay navigation is visible.
        /// </summary>
        public string MessageText
        {
            get => _messageText;

            set
            {
                if (_messageText == value)
                {
                    return;
                }

                _messageText = value;
                RaisePropertyChanged(nameof(MessageText));
            }
        }

        public long? ReplaySequence
        {
            get => _replaySequence;

            set
            {
                if (_replaySequence == value)
                {
                    return;
                }

                _replaySequence = value;
                RaisePropertyChanged(nameof(ReplaySequence));
            }
        }

        public string ReplayGameName
        {
            get => _replayGameName;

            set
            {
                if (_replayGameName == value)
                {
                    return;
                }

                _replayGameName = value;
                RaisePropertyChanged(nameof(ReplayGameName));
            }
        }

        public DateTime? ReplayStartTime
        {
            get => _replayStartTime;

            set
            {
                if (_replayStartTime == value)
                {
                    return;
                }

                _replayStartTime = value;
                RaisePropertyChanged(nameof(ReplayStartTime));
            }
        }

        /// <summary>
        ///     Gets or sets a label for the free game being replayed
        /// </summary>
        public string Label
        {
            get => _label;

            set
            {
                if (_label == value)
                {
                    return;
                }

                _label = value;
                RaisePropertyChanged(nameof(Label));
            }
        }

        public string CashoutText
        {
            get => _cashoutText;

            set
            {
                if (_cashoutText == value)
                {
                    return;
                }

                _cashoutText = value;
                RaisePropertyChanged(nameof(CashoutText));
            }
        }

        public string ReplayPauseMessageText
        {
            get => _replayPauseMessageText;

            set
            {
                if (_replayPauseMessageText == value)
                {
                    return;
                }

                _replayPauseMessageText = value;
                RaisePropertyChanged(nameof(ReplayPauseMessageText));
                RaisePropertyChanged(nameof(IsReplayPauseMessageVisible));
                RaisePropertyChanged(nameof(CanReplayContinue));
            }
        }

        public bool IsReplayPauseMessageVisible => !string.IsNullOrEmpty(_replayPauseMessageText);

        public bool CanReplayContinue =>
            !string.IsNullOrEmpty(_replayPauseMessageText) && _replayPauseMessageText != CompletionText;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameReplayPauseInputEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameReplayCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuExitingEvent>(this, HandleEvent);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);

                if (_cashoutMessageTimer != null)
                {
                    _cashoutMessageTimer.Elapsed -= CashOutMessageCycleTimerTick;
                    _cashoutMessageTimer.Dispose();
                }
            }

            _cashoutMessageTimer = null;
            _disposed = true;
        }

        private string CompletionText => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReplayCompletedText);

        private void ExitButtonPressed(object parameter)
        {
            ExitDiagnostics();
        }

        private void ContinueButtonPressed(object parameter)
        {
            ReplayPauseMessageText = string.Empty;
            _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Play));
        }

        private void HandleEvent(GameDiagnosticsStartedEvent @event)
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();

            ReplayPauseMessageText = string.Empty;
            CashoutText = string.Empty;

            var game = properties.GetValues<IGameDetail>(GamingConstants.AllGames).FirstOrDefault(g => g.Id == @event.GameId);
            if (game == null)
            {
                return;
            }

            ReplayGameName = game.ThemeName;
            if (!string.IsNullOrEmpty(game.VariationId))
            {
                ReplayGameName = $"{ReplayGameName} ({game.VariationId})";
            }

            Label = @event.Label;

            GetEventData(@event.Context as dynamic);
        }

        private void HandleEvent(GameReplayPauseInputEvent @event)
        {
            if (_properties.GetValue(GamingConstants.ReplayPauseActive, true))
            {
                // The ReplayPause can be sent after the game-end Game Event 
                if (ReplayPauseMessageText != CompletionText)
                {
                    ReplayPauseMessageText = @event.ReplayPauseState ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReplayPauseInputText) : string.Empty;
                }

                return;
            }

            // Let the replay continue if the pause is requested but it is disabled 
            if (@event.ReplayPauseState)
            {
                _eventBus.Publish(new DownEvent((int)ButtonLogicalId.Play));
            }
        }

        private void HandleEvent(GameReplayCompletedEvent @event)
        {
            _commandHandlerFactory.Create<ReplayGameEnded>().Handle(new ReplayGameEnded(_replayEndCredits.MillicentsToCents()));
            ReplayPauseMessageText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReplayCompletedText);
        }

        private void HandleEvent(GameDiagnosticsCompletedEvent @event)
        {
            ReplaySequence = null;
            ReplayGameName = string.Empty;
            ReplayStartTime = null;
            Label = string.Empty;
            CashoutText = string.Empty;
            MessageText = string.Empty;
            ReplayPauseMessageText = string.Empty;
        }

        private static void GetEventData(IDiagnosticContext<object> context)
        {
        }

        private void HandleEvent(OperatorMenuExitingEvent @event)
        {
            ExitDiagnostics();
        }

        private void ExitDiagnostics()
        {
            ReplayPauseMessageText = string.Empty;
            _gameDiagnostics.End();
            _cashoutMessageTimer?.Stop();
        }

        private void GetEventData(ReplayContext context)
        {
            var time = ServiceManager.GetInstance().GetService<ITime>();

            ReplaySequence = context.Arguments.LogSequence;

            ReplayStartTime = context.GameIndex == -1 || context.GameIndex == 0
                ? time.GetLocationTime(context.Arguments.StartDateTime)
                : time.GetLocationTime(
                    context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.StartDateTime ?? DateTime.MinValue);

            _replayEndCredits = context.Arguments.EndCredits;

            _cashOutTexts.Clear();
            AppendGameWinBonuses(context);
            AppendVoucherInfo(context);
            AppendHardMeterOutInfo(context);
            AppendHandpayInfo(context);
            DisplayCashoutInfo();
        }

        private void AppendGameWinBonuses(ReplayContext context)
        {
            if (context.Arguments.GameWinBonus <= 0)
            {
                return;
            }

            var gameWinBonusText = Localizer.For(CultureFor.Operator).FormatString(
                ResourceKeys.ReplayGameWinBonusAwarded,
                context.Arguments.GameWinBonus.CentsToDollars().FormattedCurrencyString());
            _cashOutTexts.Add(gameWinBonusText);
        }

        private void DisplayCashoutInfo()
        {
            if (!_cashOutTexts.Any())
            {
                return;
            }

            CashoutText = _cashOutTexts.First();

            if (_cashOutTexts.Count > 1)
            {
                _currentCashoutMessageIndex = 0;
                _cashoutMessageTimer.Start();
            }
        }

        private void AppendHardMeterOutInfo(ReplayContext context)
        {
            var amountOut = context.GameIndex == -1 || context.GameIndex == 0
                ? context.Arguments.Transactions
                    .Where(t => t.TransactionType == typeof(HardMeterOutTransaction))
                    .Sum(t => t.Amount)
                : context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.AmountOut ?? 0;

            if (amountOut > 0)
            {
                _cashOutTexts.Add(
                    Localizer.For(CultureFor.Operator)
                        .FormatString(ResourceKeys.ReplayTicketPrinted) + " " + amountOut.MillicentsToDollars().FormattedCurrencyString());
            }
        }

        private void AppendVoucherInfo(ReplayContext context)
        {
            var amountOut = context.GameIndex == -1 || context.GameIndex == 0
                ? context.Arguments.Transactions
                    .Where(t => t.TransactionType == typeof(VoucherOutTransaction))
                    .Sum(t => t.Amount)
                : context.Arguments.FreeGames.ElementAt(context.GameIndex - 1)?.AmountOut ?? 0;

            if (amountOut > 0)
            {
                _cashOutTexts.Add(
                    Localizer.For(CultureFor.Operator)
                        .FormatString(ResourceKeys.ReplayTicketPrinted) + " " +
                    (Convert.ToDecimal(amountOut / GamingConstants.Millicents) /
                     CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit).FormattedCurrencyString());
            }
        }

        private void AppendHandpayInfo(ReplayContext context)
        {
            // If we had a handpay key-off during the game round that was just replayed, display the hand paid text as well
            if (!(context.GameIndex == -1 || context.GameIndex == 0))
            {
                return;
            }

            var transactionInfoList = context.Arguments.Transactions
                .Where(
                    t => t.TransactionType == typeof(HandpayTransaction) && t.Amount > 0 && t.HandpayType != null)
                .ToList();
            if (!transactionInfoList.Any())
            {
                return;
            }

            long bonusOrGameWinToCredits = 0;
            long bonusOrGameWinToHandpay = 0;
            long cancelledCredits = 0;

            foreach (var transactionInfo in transactionInfoList)
            {
                switch (transactionInfo.HandpayType)
                {
                    case HandpayType.GameWin:
                    case HandpayType.BonusPay:
                        switch (transactionInfo.KeyOffType)
                        {
                            case KeyOffType.LocalCredit:
                            case KeyOffType.RemoteCredit:
                                bonusOrGameWinToCredits += transactionInfo.Amount;
                                break;
                            case KeyOffType.LocalHandpay:
                            case KeyOffType.RemoteHandpay:
                                bonusOrGameWinToHandpay += transactionInfo.Amount;
                                break;
                        }

                        break;
                    case HandpayType.CancelCredit:
                        cancelledCredits += transactionInfo.Amount;
                        break;
                }
            }

            if (bonusOrGameWinToCredits > 0)
            {
                _cashOutTexts.Add(
                    string.Format(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.JackpotToCreditsKeyedOff),
                        bonusOrGameWinToCredits.MillicentsToDollars().FormattedCurrencyString()));
            }

            if (bonusOrGameWinToHandpay > 0)
            {
                _cashOutTexts.Add(
                    string.Format(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.JackpotHandpayKeyedOff),
                        bonusOrGameWinToHandpay.MillicentsToDollars().FormattedCurrencyString()));
            }

            if (cancelledCredits > 0)
            {
                _cashOutTexts.Add(
                    string.Format(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CashOutHandpayKeyedOff),
                        cancelledCredits.MillicentsToDollars().FormattedCurrencyString()));
            }
        }

        private void CashOutMessageCycleTimerTick(object source, ElapsedEventArgs e)
        {
            _currentCashoutMessageIndex = (_currentCashoutMessageIndex + 1) % _cashOutTexts.Count;
            CashoutText = _cashOutTexts.ElementAt(_currentCashoutMessageIndex);
        }
    }
}