namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Events;
    using Contracts.Lobby;
    using Contracts.Models;
    using Contracts.PlayerInfoDisplay;
    using Events;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Localization.Properties;
    using log4net;
    using MVVM;
    using MVVM.ViewModel;
    using Utils;

    public class MessageOverlayViewModel : BaseEntityViewModel
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private const string HandPayDisplayKey = "HandPayImage";
        private const string CashoutDisplayKey = "CashOutImage";

        private readonly IOverlayMessageStrategyController _overlayMessageStrategyController;
        private readonly IEventBus _eventBus;
        private readonly IBank _bank;
        private readonly IPropertiesManager _properties;
        private readonly ITransferOutHandler _transferOutHandler;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGameRecovery _gameRecovery;
        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly PlayerMenuPopupViewModel _playerMenuPopup;
        private readonly IPlayerInfoDisplayManager _playerInfoDisplayManager;

        private MessageOverlayState _messageOverlayState;
        private bool _isLockupMessageVisible;
        private bool _isReplayRecoveryDlgVisible;
        private bool _isAgeWarningDlgVisible;
        private bool _isResponsibleGamingInfoOverlayDlgVisible;
        private bool _isOverlayWindowVisible;

        /// <summary>
        ///     Gets a value indicating whether the Non Cash Overlay Dlg is visible
        /// </summary>
        private bool IsNonCashOverlayDlgVisible => _lobbyStateManager.ContainsAnyState(LobbyState.CashOutFailure);

        public MessageOverlayViewModel(PlayerMenuPopupViewModel playerMenuPopup, IPlayerInfoDisplayManager playerInfoDisplayManager)
            : this(ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IBank>(),
                ServiceManager.GetInstance().TryGetService<IContainerService>(),
                ServiceManager.GetInstance().TryGetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<ITransferOutHandler>(),
                ServiceManager.GetInstance().TryGetService<ISystemDisableManager>())
        {
            _playerMenuPopup = playerMenuPopup;
            _playerInfoDisplayManager = playerInfoDisplayManager;
        }

        private MessageOverlayViewModel(
            IEventBus eventBus,
            IBank bank,
            IContainerService containerService,
            IPropertiesManager properties,
            ITransferOutHandler transferOutHandler,
            ISystemDisableManager systemDisableManager)
        {
            if (containerService == null)
            {
                throw new ArgumentNullException(nameof(containerService));
            }

            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _transferOutHandler = transferOutHandler ?? throw new ArgumentNullException(nameof(transferOutHandler));
            _systemDisableManager = systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            ReserveOverlayViewModel = new ReserveOverlayViewModel();

            _lobbyStateManager = _lobbyStateManager = containerService.Container.GetInstance<ILobbyStateManager>();
            _overlayMessageStrategyController = containerService.Container.GetInstance<IOverlayMessageStrategyController>();
            MessageOverlayData = containerService.Container.GetInstance<IMessageOverlayData>();
            _gameDiagnostics = containerService.Container.GetInstance<IGameDiagnostics>();
            _gameRecovery = containerService.Container.GetInstance<IGameRecovery>();
        }

        public IMessageOverlayData MessageOverlayData { get; set; }

        public ReserveOverlayViewModel ReserveOverlayViewModel { get; }

        public bool ShowVoucherNotification { get; set; }

        public bool ShowProgressiveGameDisabledNotification { get; set; }

        public bool CustomMainViewElementVisible { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether we need to show a lockup message.
        /// </summary>
        public bool IsLockupMessageVisible
        {
            get => _isLockupMessageVisible;
            set => SetProperty(ref _isLockupMessageVisible, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the replay/recovery dialog is visible
        /// </summary>
        public bool IsReplayRecoveryDlgVisible
        {
            get => _isReplayRecoveryDlgVisible;
            set => SetProperty(ref _isReplayRecoveryDlgVisible, value);
        }

        public bool IsAgeWarningDlgVisible
        {
            get => _isAgeWarningDlgVisible;
            set => SetProperty(ref _isAgeWarningDlgVisible, value);
        }

        public bool IsResponsibleGamingInfoOverlayDlgVisible
        {
            get => _isResponsibleGamingInfoOverlayDlgVisible;
            set => SetProperty(ref _isResponsibleGamingInfoOverlayDlgVisible, value);
        }

        /// <summary>
        ///     Gets a value indicating whether we are cashing out.
        /// </summary>
        public bool IsCashingOutDlgVisible
        {
            get
            {
                if (_lobbyStateManager.CurrentState == LobbyState.Disabled)
                {
                    return _lobbyStateManager.ContainsAnyState(LobbyState.CashOut) && _transferOutHandler.InProgress &&
                           !_lobbyStateManager.ContainsAnyState(LobbyState.CashIn, LobbyState.CashOutFailure);
                }

                return _lobbyStateManager.ContainsAnyState(LobbyState.CashOut, LobbyState.PrintHelpline) &&
                       !_lobbyStateManager.ContainsAnyState(LobbyState.CashIn, LobbyState.CashOutFailure);
            }
        }

        public bool IsSelectPayModeVisible
        {
            get => _isSelectPayModeVisible;
            set => SetProperty(ref _isSelectPayModeVisible, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the overlay window is visible.
        /// </summary>
        public bool IsOverlayWindowVisible
        {
            get => _isOverlayWindowVisible;

            private set
            {
                if (_isOverlayWindowVisible != value)
                {
                    _isOverlayWindowVisible = value;
                    _eventBus.Publish(new OverlayWindowVisibilityChangedEvent(_isOverlayWindowVisible));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether we should show the cashing in dialog
        /// </summary>
        public bool IsCashingInDlgVisible { get; set; }

        public bool LastCashOutForcedByMaxBank;

        public bool ForceBuildLockupText { get; set; }

        public CashInType CashInType { get; set; }

        public bool ShowPaidMeterForAutoCashout { get; set; }

        public readonly ConcurrentDictionary<string, DisplayableMessage> HardErrorMessages =
            new ConcurrentDictionary<string, DisplayableMessage>();

        private bool _isSelectPayModeVisible;

        public void UpdateCashoutButtonState(bool state)
        {
            _overlayMessageStrategyController.OverlayStrategy.CashOutButtonPressed = state;
            _overlayMessageStrategyController.FallBackStrategy.CashOutButtonPressed = state;
        }

        public void AddHardErrorMessage(DisplayableMessage displayableMessage)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (HardErrorMessages.ContainsKey(displayableMessage.Message))
                    {
                        return;
                    }

                    HardErrorMessages.TryAdd(displayableMessage.Message, displayableMessage);
                    ForceBuildLockupText = HardErrorMessages.Count > 1 && !_systemDisableManager.CurrentDisableKeys.Contains(ApplicationConstants.OperatorMenuLauncherDisableGuid);
                    MessageOverlayData.IsDialogFadingOut = false;
                    HandleMessageOverlayText(displayableMessage.Message);
                });
        }

        public void RemoveHardErrorMessage(DisplayableMessage displayableMessage)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (!HardErrorMessages.TryRemove(displayableMessage.Message, out _))
                    {
                        Logger.Warn($"RemoveMessage failed to remove message {displayableMessage.Message}");
                    }

                    ForceBuildLockupText = HardErrorMessages.Count > 1;

                    HandleMessageOverlayText(string.Empty);
                });
        }

        public void HandleMessageOverlayText(string message)
        {
            MessageOverlayData.Clear();
            Logger.Debug("MessageOverlayData cleared. " +
                         $"MessageOverlayState={_messageOverlayState}, " +
                         $"CashOutState={_lobbyStateManager.CashOutState}, " +
                         $"IsDialogVisible={MessageOverlayData.IsDialogVisible}");

            _messageOverlayState = GetMessageOverlayState();
            MessageOverlayData.DisplayImageResourceKey = GetDisplayImageKey();

            var messageSent = false;
            switch (_messageOverlayState)
            {
                case MessageOverlayState.VoucherNotification:
                    MessageOverlayData.DisplayForPopUp = true;
                    MessageOverlayData.Text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherNotification).Replace("\\r\\n", Environment.NewLine);
                    break;
                case MessageOverlayState.CashOut:
                    if (LastCashOutForcedByMaxBank && _lobbyStateManager.CashOutState == LobbyCashOutState.Voucher)
                    {
                        ShowPaidMeterForAutoCashout = true;
                    }

                    MessageOverlayData = _overlayMessageStrategyController.OverlayStrategy.HandleMessageOverlayCashOut(MessageOverlayData, LastCashOutForcedByMaxBank, _lobbyStateManager.CashOutState);
                    messageSent = true;
                    break;
                case MessageOverlayState.PrintHelpline:
                    MessageOverlayData.Text = Localizer.For(CultureFor.Player).GetString(ResourceKeys.PrintingTicket);
                    break;
                case MessageOverlayState.CashOutFailure:
                    MessageOverlayData.DisplayForPopUp = true;
                    var localeCode = _properties.GetValue(GamingConstants.SelectedLocaleCode, GamingConstants.EnglishCultureCode)
                        .ToUpperInvariant();
                    MessageOverlayData.Text = string.Format(
                            new CultureInfo(localeCode),
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NonCashableCreditsFailedMessage),
                            OverlayMessageUtils.ToCredits(_bank.QueryBalance(AccountType.NonCash)).FormattedCurrencyString())
                        .Replace("\\r\\n", Environment.NewLine);
                    break;
                case MessageOverlayState.CashIn:
                    var stateContainsCashOut = _lobbyStateManager.ContainsAnyState(LobbyState.CashOut);
                    MessageOverlayData = _overlayMessageStrategyController.OverlayStrategy.HandleMessageOverlayCashIn(MessageOverlayData, CashInType, stateContainsCashOut, _lobbyStateManager.CashOutState);
                    messageSent = true;
                    break;
                case MessageOverlayState.Diagnostics:
                    MessageOverlayData.ReplayText = _gameDiagnostics.IsActive && _gameDiagnostics.Context is IDiagnosticContext<IGameHistoryLog>
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReplayText)
                        : string.Empty;
                    break;
                case MessageOverlayState.Handpay:
                    if (_overlayMessageStrategyController.OverlayStrategy.LastHandpayType is null || ForceBuildLockupText)
                    {
                        MessageOverlayData.Text = BuildLockupMessageText();
                    }
                    else
                    {
                        MessageOverlayData = _overlayMessageStrategyController.OverlayStrategy.HandleMessageOverlayHandPay(MessageOverlayData, message);
                        messageSent = true;
                    }
                    break;
                case MessageOverlayState.Disabled:
                    if (_lobbyStateManager.ContainsAnyState(LobbyState.CashIn))
                    {
                        _lobbyStateManager.RemoveStackableState(LobbyState.CashIn);
                    }
                    if (MessageOverlayData.IsDialogFadingOut && !MessageOverlayData.DisplayForEvents && !MessageOverlayData.DisplayForPopUp)
                    {
                        MessageOverlayData.Text = MessageOverlayData.Text;
                        MessageOverlayData.SubText = MessageOverlayData.SubText;
                        MessageOverlayData.SubText2 = MessageOverlayData.SubText2;
                        MessageOverlayData.IsSubText2Visible = MessageOverlayData.IsSubText2Visible;
                    }
                    else
                    {
                        MessageOverlayData.Text = BuildLockupMessageText();
                    }

                    var printer = ServiceManager.GetInstance().TryGetService<IPrinter>();
                    if (_lobbyStateManager.CashOutState == LobbyCashOutState.Voucher && printer?.LogicalState == PrinterLogicalState.Printing)
                    {
                        // Include the Printing text as well if we are disabled but still printing,
                        // otherwise the printing message may be delayed until after printing is finished
                        var cashOutData = new MessageOverlayData();
                        _overlayMessageStrategyController.OverlayStrategy.HandleMessageOverlayCashOut(MessageOverlayData, LastCashOutForcedByMaxBank, _lobbyStateManager.CashOutState);
                        messageSent = true;

                        if (!MessageOverlayData.Text.Contains(cashOutData.Text))
                        {
                            MessageOverlayData.Text = string.IsNullOrWhiteSpace(MessageOverlayData.Text)
                                ? cashOutData.Text
                                : $"{MessageOverlayData.Text}\n{cashOutData.Text}";
                            MessageOverlayData.SubText = string.IsNullOrWhiteSpace(MessageOverlayData.SubText)
                                ? cashOutData.SubText
                                : $"{MessageOverlayData.SubText}\n{cashOutData.SubText}";
                            MessageOverlayData.SubText2 = string.IsNullOrWhiteSpace(MessageOverlayData.SubText2)
                                ? cashOutData.SubText2
                                : $"{MessageOverlayData.SubText2}\n{cashOutData.SubText2}";
                            MessageOverlayData.IsSubText2Visible |= cashOutData.IsSubText2Visible;
                        }
                    }
                    break;
                case MessageOverlayState.ProgressiveGameDisabledNotification:
                    MessageOverlayData.DisplayForPopUp = true;
                    MessageOverlayData.Text = Localizer.For(CultureFor.Player).GetString(ResourceKeys.GameDisabledProgressiveError)
                        .Replace("\\r\\n", Environment.NewLine);
                    break;
            }

            ClearPresentationIfComplete(messageSent);

            Logger.Debug(MessageOverlayData.GenerateLogText());

            if (_eventBus is null)
            {
                // We're in process of shutting down.
                return;
            }

#if !(RETAIL)
            _eventBus.Publish(new MessageOverlayDataEvent(MessageOverlayData));
#endif

            // Wait until after message data has been re-set instead of updating visibility immediately after clearing it
            // This will prevent message flickering
            HandleOverlayWindowDialogVisibility();
            RaisePropertyChanged(nameof(MessageOverlayData));
        }

        public void HandleOverlayWindowDialogVisibility()
        {
            IsLockupMessageVisible = _lobbyStateManager.IsInState(LobbyState.Disabled);

            IsReplayRecoveryDlgVisible = _lobbyStateManager.CurrentState == LobbyState.GameLoadingForDiagnostics ||
                                         _lobbyStateManager.CurrentState == LobbyState.GameDiagnostics ||
                                         _lobbyStateManager.CurrentState == LobbyState.GameLoading && _gameRecovery.IsRecovering;

            IsAgeWarningDlgVisible = _lobbyStateManager.ContainsAnyState(LobbyState.AgeWarningDialog);

            IsResponsibleGamingInfoOverlayDlgVisible = _lobbyStateManager.ContainsAnyState(LobbyState.ResponsibleGamingInfoLayeredLobby, LobbyState.ResponsibleGamingInfoLayeredGame);

            if (IsLockupMessageVisible &&
                _systemDisableManager.CurrentDisableKeys.Contains(ApplicationConstants.ReserveDisableKey) &&
                HardErrorMessages.Count == 1)
            {
                ReserveOverlayViewModel.IsDialogVisible = true;
            }

            // Keep message overlay dialog visible when handpay PAID banner is up
            var isHandpayPaidDialogVisible = _lobbyStateManager.CashOutState == LobbyCashOutState.HandPay &&
                                         _messageOverlayState == MessageOverlayState.CashOut &&
                                         !string.IsNullOrEmpty(MessageOverlayData.Text);

            var hasHardLockup = HardErrorMessages.Count > 1 ||
                                !_systemDisableManager.CurrentDisableKeys.Contains(ApplicationConstants.ReserveDisableKey) && HardErrorMessages.Count >= 1;

            var isPresentationOverridden = IsPresentationOverridden();

            MessageOverlayData.IsDialogVisible = !isPresentationOverridden &&
                                                 IsLockupMessageVisible && hasHardLockup ||
                                                 isHandpayPaidDialogVisible ||
                                                 IsCashingOutDlgVisible ||
                                                 IsCashingInDlgVisible ||
                                                 IsNonCashOverlayDlgVisible ||
                                                 ShowProgressiveGameDisabledNotification ||
                                                 ShowVoucherNotification;

            if (MessageOverlayData.IsDialogVisible)
            {
                ReserveOverlayViewModel.IsDialogVisible = false;
            }

            IsOverlayWindowVisible = IsReplayRecoveryDlgVisible ||
                                     IsAgeWarningDlgVisible ||
                                     IsSelectPayModeVisible ||
                                     IsResponsibleGamingInfoOverlayDlgVisible ||
                                     MessageOverlayData.IsDialogVisible ||
                                     ReserveOverlayViewModel.IsDialogVisible ||
                                     _playerMenuPopup.IsMenuVisible ||
                                     _playerInfoDisplayManager.IsActive() ||
                                     CustomMainViewElementVisible;

            Logger.Debug("HandleOverlayWindowDialogVisibility: " +
                         $"CurrentState={_lobbyStateManager.CurrentState}, " +
                         $"IsLockupMessageVisible={IsLockupMessageVisible}, " +
                         $"HardErrorMessages={string.Join("; ", HardErrorMessages.Keys)}, " +
                         $"CurrentDisableKeys={string.Join("; ", _systemDisableManager.CurrentDisableKeys)}, " +
                         $"hasHardLockup={hasHardLockup}, " +
                         $"isHandpayPaidDialogVisible={isHandpayPaidDialogVisible}, " +
                         $"IsCashingOutDlgVisible={IsCashingOutDlgVisible}, " +
                         $"IsCashingInDlgVisible={IsCashingInDlgVisible}, " +
                         $"IsNonCashOverlayDlgVisible={IsNonCashOverlayDlgVisible}, " +
                         $"ShowProgressiveGameDisabledNotification={ShowProgressiveGameDisabledNotification}, " +
                         $"ShowVoucherNotification={ShowVoucherNotification}, " +
                         $"MessageOverlayData.IsDialogVisible={MessageOverlayData.IsDialogVisible}, " +
                         $"IsPresentationOverridden={isPresentationOverridden}" +
                         $"IsOverlayWindowVisible={IsOverlayWindowVisible}, ");
        }

        private string BuildLockupMessageText()
        {
            var overlayMsg = new StringBuilder();

            var messages = HardErrorMessages.ToArray();

            foreach (var message in messages.Select(o => o.Value.Message).Distinct())
            {
                overlayMsg.AppendLine(message);
            }

            return overlayMsg.ToString();
        }

        private MessageOverlayState GetMessageOverlayState()
        {
            var state = MessageOverlayState.Disabled;
            if (!_lobbyStateManager.IsInState(LobbyState.Disabled))
            {
                if (ShowProgressiveGameDisabledNotification)
                {
                    state = MessageOverlayState.ProgressiveGameDisabledNotification;
                }
                else if (_lobbyStateManager.ContainsAnyState(LobbyState.CashOut))
                {
                    state = MessageOverlayState.CashOut;
                }
                else if (_lobbyStateManager.ContainsAnyState(LobbyState.CashIn))
                {
                    state = MessageOverlayState.CashIn;
                }
                else if (_lobbyStateManager.ContainsAnyState(LobbyState.CashOutFailure))
                {
                    state = MessageOverlayState.CashOutFailure;
                }
                else if (ShowVoucherNotification)
                {
                    state = MessageOverlayState.VoucherNotification;
                }
                else if (_lobbyStateManager.ContainsAnyState(LobbyState.PrintHelpline))
                {
                    state = MessageOverlayState.PrintHelpline;
                }
            }
            else
            {
                if (_lobbyStateManager.ContainsAnyState(LobbyState.CashOutFailure))
                {
                    state = MessageOverlayState.CashOutFailure;
                }
                else if (_lobbyStateManager.ContainsAnyState(LobbyState.GameLoadingForDiagnostics, LobbyState.GameDiagnostics))
                {
                    state = MessageOverlayState.Diagnostics;
                }
                else if (_systemDisableManager.CurrentDisableKeys.Contains(ApplicationConstants.HandpayPendingDisableKey) &&
                         HardErrorMessages.Count == 1 && !_overlayMessageStrategyController.OverlayStrategy.IsBasic)
                {
                    state = MessageOverlayState.Handpay;
                }
                // When we are in lockup, we can give a cashout button on screen, and if that's pressed
                // handle the enhanced display
                else if (_lobbyStateManager.ContainsAnyState(LobbyState.CashOut) &&
                         _lobbyStateManager.CashOutState != LobbyCashOutState.Undefined &&
                         (!_systemDisableManager.DisableImmediately ||
                          _systemDisableManager.CurrentDisableKeys.All(
                              x => x == ApplicationConstants.LiveAuthenticationDisableKey) &&
                          _lobbyStateManager.CashOutState == LobbyCashOutState.HandPay))
                {
                    state = MessageOverlayState.CashOut;
                }
                else if (ShowVoucherNotification)
                {
                    state = MessageOverlayState.VoucherNotification;
                }
            }

            Logger.Debug($"Setting MessageOverlayState={state} (LobbyState={_lobbyStateManager.CurrentState}, BaseState={_lobbyStateManager.BaseState}, CashOutState={_lobbyStateManager.CashOutState})");
            return state;
        }

        private string GetDisplayImageKey()
        {
            var displayImageResourceKey = string.Empty;

            if (IsCashingOutDlgVisible)
            {
                switch (_lobbyStateManager.CashOutState)
                {
                    case LobbyCashOutState.Voucher:
                        if (_overlayMessageStrategyController.OverlayStrategy.CashOutButtonPressed)
                        {
                            displayImageResourceKey = CashoutDisplayKey;
                        }
                        break;
                    case LobbyCashOutState.HandPay:
                        displayImageResourceKey = HandPayDisplayKey;
                        break;
                }
            }

            return displayImageResourceKey;
        }

        private bool IsPresentationOverridden()
        {
            var isCashoutOverridden = _overlayMessageStrategyController.RegisteredPresentations.Contains(
                                          PresentationOverrideTypes.PrintingCashoutTicket) &&
                                      IsCashingOutDlgVisible && _messageOverlayState == MessageOverlayState.CashOut &&
                                      _lobbyStateManager.CashOutState == LobbyCashOutState.Voucher && !LastCashOutForcedByMaxBank;

            var isCashWinOverridden = _overlayMessageStrategyController.RegisteredPresentations.Contains(
                                          PresentationOverrideTypes.PrintingCashwinTicket) &&
                                      IsCashingOutDlgVisible &&
                                      _messageOverlayState == MessageOverlayState.CashOut && LastCashOutForcedByMaxBank;

            var isTransferOutOverridden = _overlayMessageStrategyController.RegisteredPresentations.Contains(
                                              PresentationOverrideTypes.TransferingOutCredits) &&
                                          IsCashingOutDlgVisible && _messageOverlayState == MessageOverlayState.CashOut &&
                                          _lobbyStateManager.CashOutState == LobbyCashOutState.Wat;

            var isHandpayOverridden = _overlayMessageStrategyController.RegisteredPresentations.Contains(
                                            PresentationOverrideTypes.JackpotHandpay) &&
                                      _overlayMessageStrategyController.OverlayStrategy.LastHandpayType == HandpayType.GameWin &&
                                       IsCashingOutDlgVisible &&
                                       (_messageOverlayState == MessageOverlayState.Handpay || _lobbyStateManager.CashOutState == LobbyCashOutState.HandPay);

            var isBonusHandpayOverridden = _overlayMessageStrategyController.RegisteredPresentations.Contains(
                                               PresentationOverrideTypes.BonusJackpot) &&
                                           _overlayMessageStrategyController.OverlayStrategy.LastHandpayType == HandpayType.BonusPay &&
                                           IsCashingOutDlgVisible &&
                                           (_messageOverlayState == MessageOverlayState.Handpay || _lobbyStateManager.CashOutState == LobbyCashOutState.HandPay);

            var isCancelledCreditHandpayOverridden = _overlayMessageStrategyController.RegisteredPresentations.Contains(
                                                         PresentationOverrideTypes.CancelledCreditsHandpay) &&
                                                     _overlayMessageStrategyController.OverlayStrategy.LastHandpayType == HandpayType.CancelCredit &&
                                                     IsCashingOutDlgVisible &&
                                                     (_messageOverlayState == MessageOverlayState.Handpay ||
                                                      _lobbyStateManager.CashOutState == LobbyCashOutState.HandPay);

            var isCashInOverridden = _overlayMessageStrategyController.RegisteredPresentations.Contains(
                                          PresentationOverrideTypes.TransferingInCredits) &&
                                      IsCashingInDlgVisible && _messageOverlayState == MessageOverlayState.CashIn;

            return (_overlayMessageStrategyController.GameRegistered && !ForceBuildLockupText) &&
                   (isCashoutOverridden ||
                    isCashWinOverridden ||
                    isTransferOutOverridden ||
                    isHandpayOverridden ||
                    isBonusHandpayOverridden ||
                    isCancelledCreditHandpayOverridden ||
                    isCashInOverridden ||
                    IsCashingOutDlgVisible && _lobbyStateManager.CashOutState == LobbyCashOutState.Undefined);
        }
        
        public void HandpayCancelled()
        {
            _overlayMessageStrategyController.SetLastCashOutAmount(0);
        }

        public void HandpayKeyedOff(HandpayKeyedOffEvent evt)
        {
            var cashOutState = LobbyCashOutState.Undefined;

            if (!evt.Transaction.IsCreditType())
            {
                cashOutState = LobbyCashOutState.HandPay;
            }

            if (evt.Transaction.HandpayType == HandpayType.GameWin)
            {
                var forcedKeyOff = _properties.GetValue(AccountingConstants.HandpayLargeWinForcedKeyOff, false);
                if (forcedKeyOff)
                {
                    cashOutState = LobbyCashOutState.Voucher;
                }
            }

            _lobbyStateManager.CashOutState = cashOutState;

            _overlayMessageStrategyController.SetLastCashOutAmount(
                evt.Transaction.CashableAmount + evt.Transaction.NonCashAmount + evt.Transaction.PromoAmount);

            HandleMessageOverlayText(string.Empty);

            Logger.Debug("Detected HandpayKeyedOffEvent. " +
                         $"Amount: {_overlayMessageStrategyController.OverlayStrategy.LastCashOutAmount} " +
                         $"CashOutState: {_lobbyStateManager.CashOutState}");
        }

        public void HandpayStarted(HandpayStartedEvent evt)
        {
            var forcedKeyOff = _properties.GetValue(AccountingConstants.HandpayLargeWinForcedKeyOff, false);
            var jurisdictionLargeWinKeyOffType = _properties.GetValue(AccountingConstants.HandpayLargeWinKeyOffStrategy, KeyOffType.LocalHandpay);

            if (evt.Handpay == HandpayType.GameWin && forcedKeyOff && jurisdictionLargeWinKeyOffType == KeyOffType.LocalHandpay)
            {
                return;
            }

            _lobbyStateManager.CashOutState = LobbyCashOutState.HandPay;

            _overlayMessageStrategyController.SetHandpayAmountAndType(evt.CashableAmount + evt.NonCashAmount + evt.PromoAmount, evt.Handpay, evt.WagerAmount);

            Logger.Debug("Detected HandpayStartedEvent. " +
                         $"HandpayAmount: {_overlayMessageStrategyController.OverlayStrategy.HandpayAmount} " +
                         $"CashOutState: {_lobbyStateManager.CashOutState}");
        }

        public void TransferOutFailed(TransferOutFailedEvent evt)
        {
            var config = _properties.GetValue<LobbyConfiguration>(GamingConstants.LobbyConfig, null);
            if (config == null || !config.NonCashCashoutFailureMessageEnabled || evt.NonCashableAmount <= 0)
            {
                return;
            }

            _overlayMessageStrategyController.SetLastCashOutAmount(evt.NonCashableAmount);
        }

        public void VoucherOutStarted(VoucherOutStartedEvent evt)
        {
            _lobbyStateManager.CashOutState = LobbyCashOutState.Voucher;

            _overlayMessageStrategyController.SetLastCashOutAmount(evt.Amount);

            Logger.Debug("Detected VoucherOutStartedEvent. " +
                         $"Amount: {_overlayMessageStrategyController.OverlayStrategy.LastCashOutAmount} " +
                         $"CashOutState: {_lobbyStateManager.CashOutState}");

            HandleMessageOverlayText(string.Empty);
        }

        public void WatTransferInitiated(WatTransferInitiatedEvent evt)
        {
            _lobbyStateManager.CashOutState = LobbyCashOutState.Wat;

            _overlayMessageStrategyController.SetLastCashOutAmount(evt.Transaction.CashableAmount + evt.Transaction.NonCashAmount + evt.Transaction.PromoAmount);

            Logger.Debug("Detected WatTransferInitiatedEvent. " +
                         $"Amount: {_overlayMessageStrategyController.OverlayStrategy.LastCashOutAmount} " +
                         $"CashOutState: {_lobbyStateManager.CashOutState}");

            HandleMessageOverlayText(string.Empty);
        }

        private void ClearPresentationIfComplete(bool messageSentToOverlay)
        {
            var shouldClearPresentation = !messageSentToOverlay && _overlayMessageStrategyController.GameRegistered ||
                                          !MessageOverlayData.GameHandlesHandPayPresentation;

            if (!shouldClearPresentation)
            {
                return;
            }

            Logger.Debug("Sending PresentOverriddenPresentation Clear");
            _overlayMessageStrategyController.ClearGameDrivenPresentation();
        }
        private static void HandleEvent(IEvent evt)
        {
            // no implementation intentionally
        }
    }
}
