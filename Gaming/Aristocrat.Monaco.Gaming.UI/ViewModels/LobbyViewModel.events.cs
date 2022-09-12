namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Media;
    using Cabinet.Contracts;
    using Contracts;
    using Contracts.Bonus;
    using Contracts.Events;
    using Contracts.InfoBar;
    using Contracts.Models;
    using Contracts.Progressives;
    using Events;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.ButtonDeck;
    using Hardware.Contracts.Display;
    using Hardware.Contracts.IdReader;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common.Models;
    using MVVM;
    using Utils;
    using PayMethod = Contracts.Bonus.PayMethod;
#if !(RETAIL)
    using RobotController.Contracts;
    using Vgt.Client12.Testing.Tools;
#endif

    /// <summary>
    ///     Events parts of LobbyViewModel
    /// </summary>
    public partial class LobbyViewModel
    {

        /// <summary>
        ///     Raised when the displays have changed
        /// </summary>
        public event EventHandler DisplayChanged;

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<SystemDownEvent>(this, HandleEvent);
            _eventBus.Subscribe<UpEvent>(this, HandleEvent);
#if !(RETAIL)
            _eventBus.Subscribe<GameLoadRequestedEvent>(this, HandleEvent);
            _eventBus.Subscribe<DebugCurrencyAcceptedEvent>(this, HandleEvent);
#endif
            _eventBus.Subscribe<GameInitializationCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameExitedNormalEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameProcessExitedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameFatalErrorEvent>(this, HandleEvent);
            _eventBus.Subscribe<GamePlayDisabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<GamePlayEnabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<ViewResizeEvent>(this, HandleEvent);
            _eventBus.Subscribe<PrimaryOverlayMediaPlayerEvent>(this, HandleEvent);
            _eventBus.Subscribe<CashOutStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CashOutAbortedEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandpayCanceledEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutFailedEvent>(this, HandleEvent);
            _eventBus.Subscribe<WatTransferInitiatedEvent>(this, HandleEvent);
            _eventBus.Subscribe<WatTransferCommittedEvent>(this, HandleEvent);
            _eventBus.Subscribe<VoucherOutStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandpayStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandpayKeyedOffEvent>(this, HandleEvent);
            _eventBus.Subscribe<VoucherRedemptionRequestedEvent>(this, HandleEvent);
            _eventBus.Subscribe<VoucherRedeemedEvent>(this, HandleEvent);
            _eventBus.Subscribe<VoucherRejectedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CurrencyInStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CurrencyInCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<WatOnStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<WatOnCompleteEvent>(this, HandleEvent);
            _eventBus.Subscribe<BankBalanceChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameAddedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameRemovedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameEnabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameDisabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameUninstalledEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameUpgradedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameOrderChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameTagsChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameRequestedLobbyEvent>(this, HandleEvent);
            _eventBus.Subscribe<PropertyChangedEvent>(this, HandleEvent, evt => evt.PropertyName == GamingConstants.IdleText);
            _eventBus.Subscribe<GameIdleEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this, HandleEvent);
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PrimaryGameStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameEndedEvent>(this, HandleEvent);
            _eventBus.Subscribe<DisableCountdownTimerEvent>(this, HandleEvent);
            _eventBus.Subscribe<DisplayingTimeRemainingChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<ReprintTicketEvent>(this, HandleEvent);
            _eventBus.Subscribe<PrintCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<FieldOfInterestEvent>(this, HandleEvent);
            _eventBus.Subscribe<SetValidationEvent>(this, HandleEvent);
            _eventBus.Subscribe<TimeUpdatedEvent>(this, HandleEvent);
            _eventBus.Subscribe<ShowServiceConfirmationEvent>(this, HandleEvent);
            _eventBus.Subscribe<MissedStartupEvent>(this, HandleEvent);
            _eventBus.Subscribe<BonusStartedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PartialBonusPaidEvent>(this, HandleEvent, evt => evt.Transaction.Mode != BonusMode.GameWin);
            _eventBus.Subscribe<BonusAwardedEvent>(this, HandleEvent, evt => evt.Transaction.Mode != BonusMode.GameWin);
            _eventBus.Subscribe<BonusFailedEvent>(this, HandleEvent, evt => evt.Transaction.Mode != BonusMode.GameWin);
            _eventBus.Subscribe<DisplayConnectedEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandpayKeyOffPendingEvent>(this, HandleEvent);
            _eventBus.Subscribe<HandpayCompletedEvent>(this, HandleEvent);
            _eventBus.Subscribe<SessionInfoEvent>(this, HandleEvent);
            _eventBus.Subscribe<CultureChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<LobbySettingsChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CashoutNotificationEvent>(this, HandleEvent);
            _eventBus.Subscribe<CurrencyCultureChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameDenomChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CashOutButtonPressedEvent>(this, HandleEvent);
            _eventBus.Subscribe<AttractConfigurationChangedEvent>(this, HandleEvent);
            _eventBus.Subscribe<ProgressiveGameDisabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<ProgressiveGameEnabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<InfoBarDisplayTransientMessageEvent>(this, HandleEvent);
            _eventBus.Subscribe<InfoBarDisplayStaticMessageEvent>(this, HandleEvent);
            _eventBus.Subscribe<InfoBarCloseEvent>(this, HandleEvent);
            _eventBus.Subscribe<SystemDisableRemovedEvent>(this, HandleEvent);
            _eventBus.Subscribe<DenominationSelectedEvent>(this, HandleEvent);
            _eventBus.Subscribe<CallAttendantButtonOffEvent>(this, HandleEvent);
            _eventBus.Subscribe<ReserveButtonPressedEvent>(this, HandleEvent);
            _eventBus.Subscribe<ViewInjectionEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferEnableOnOverlayEvent>(this, HandleEvent);
            _eventBus.Subscribe<PlayerMenuButtonPressedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PlayerInfoDisplayExitedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PlayerInfoDisplayEnteredEvent>(this, HandleEvent);
            _eventBus.Subscribe<GambleFeatureActiveEvent>(this, HandleEvent);
        }

        public delegate void CustomViewChangedEventHandler(ViewInjectionEvent ev);

        public event CustomViewChangedEventHandler CustomEventViewChangedEvent;

        private void HandleEvent(ViewInjectionEvent evt)
        {
            Logger.Debug($"ViewInjectionEvent: Role: {evt.DisplayRole}, Action: {evt.Action}, Element: {evt.Element?.GetType().FullName}/{evt.Element?.GetHashCode()}");

            if (evt.DisplayRole == DisplayRole.Main && !MessageOverlayDisplay.CustomMainViewElementVisible)
            {
                MessageOverlayDisplay.CustomMainViewElementVisible = evt.Action == ViewInjectionEvent.ViewAction.Add;
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    HandleMessageOverlayText();
                    OnCustomEventViewChangedEvent(evt);
                });
        }

        private void HandleEvent(CallAttendantButtonOffEvent evt)
        {
            RaisePropertiesChanged();
        }

        private void HandleEvent(ReserveButtonPressedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    //Check if reserve is supported
                    if (!_reserveService.CanReserveMachine)
                    {
                        return;
                    }

                    MessageOverlayDisplay.ReserveOverlayViewModel.IsDialogVisible = true;
                    MvvmHelper.ExecuteOnUI(HandleMessageOverlayText);
                });
        }

        private void HandleEvent(ProgressiveGameDisabledEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (!_gameRecovery.IsRecovering && _gameState.UncommittedState == PlayState.Idle &&
                        _gameService.Running &&
                        _selectedGame?.Denomination == evt.Denom && _selectedGame?.GameId == evt.GameId &&
                        (string.IsNullOrEmpty(evt.BetOption) || _selectedGame?.BetOption == evt.BetOption))
                    {
                        // Only display this notification if idle we will lockup when not idle elsewhere
                        MessageOverlayDisplay.ShowProgressiveGameDisabledNotification = true;
                        UpdateUI();
                    }
                });
        }

        private void HandleEvent(ProgressiveGameEnabledEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_selectedGame?.Denomination == evt.Denom && _selectedGame?.GameId == evt.GameId &&
                        (string.IsNullOrEmpty(evt.BetOption) || _selectedGame?.BetOption == evt.BetOption))
                    {
                        MessageOverlayDisplay.ShowProgressiveGameDisabledNotification = false;
                        UpdateUI();
                    }
                });
        }

        private void HandleEvent(CashoutNotificationEvent evt)
        {
            MessageOverlayDisplay.ShowVoucherNotification = evt.PaperIsInChute;
            if (evt.PaperIsInChute && !_systemDisableManager.IsDisabled)
            {
                PlayLoopingAlert(Sound.PaperInChute, -1);
            }
            else if (!evt.PaperIsInChute)
            {
                StopSound(Sound.PaperInChute);
            }

            if (!evt.IsResending)
            {
                MvvmHelper.ExecuteOnUI(UpdateUI);
            }
        }

        private void HandleEvent(CashOutButtonPressedEvent evt)
        {
            MessageOverlayDisplay.UpdateCashoutButtonState(true);
        }

        private void HandleEvent(DisplayConnectedEvent evt)
        {
            DisplayChanged?.Invoke(this, EventArgs.Empty);
        }

        private void HandleEvent(GameUninstalledEvent gameUninstalledEvent)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    CurrentAttractIndex = 0;
                    SetAttractVideos();
                });
        }

        private void HandleEvent(GameUpgradedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    OnUserInteraction();
                    LoadGameInfo();

                    CurrentAttractIndex = 0;
                    SetAttractVideos();
                });
        }

        private void HandleEvent(SystemDownEvent platformEvent)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (PlayerMenuPopupViewModel.IsMenuVisible)
                    {
                        PlayerMenuPopupViewModel.IsMenuVisible = false;
                        HandleMessageOverlayVisibility();
                    }

                    if (_systemDisableManager.IsDisabled)
                    {
                        if (!MessageOverlayDisplay.ShowProgressiveGameDisabledNotification && ContainsAnyState(LobbyState.CashOutFailure))
                        {
                            _lobbyStateManager.RemoveFlagState(LobbyState.CashOutFailure);
                        }

                        return;
                    }

                    if (IsLobbyVisible && platformEvent.Enabled)
                    {
                        if (!IsResponsibleGamingInfoFullScreen)
                        {
                            ExitResponsibleGamingInfoDialog();
                        }

                        if (IsInState(LobbyState.Chooser))
                        {
                            if (Enum.IsDefined(typeof(LcdButtonDeckLobby), platformEvent.LogicalId))
                            {
                                HandleLcdButtonDeckButtonPress((LcdButtonDeckLobby)platformEvent.LogicalId);
                            }
                        }

                        OnUserInteraction();
                    }

                    if (MessageOverlayDisplay.ShowProgressiveGameDisabledNotification)
                    {
                        MessageOverlayDisplay.ShowProgressiveGameDisabledNotification = false;
                        InitiateGameShutdown();
                        UpdateUI();
                    }
                    else if (ContainsAnyState(LobbyState.CashOutFailure))
                    {
                        _lobbyStateManager.RemoveFlagState(LobbyState.CashOutFailure);
                    }
                });
        }

        private void HandleEvent(UpEvent platformEvent)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (IsLobbyVisible && !_systemDisableManager.IsDisabled)
                    {
                        OnUserInteraction();
                    }

                    if (PlayerMenuPopupViewModel.IsMenuVisible)
                    {
                        PlayerMenuPopupViewModel.IsMenuVisible = false;
                        HandleMessageOverlayVisibility();
                    }
                });
        }

        private void HandleEvent(GameInitializationCompletedEvent platformEvent)
        {
            // Why is this here instead of in OnGameLoaded()?  Because OnGameLoaded actually gets called
            // any time we reenter the Game State, NOT just the first time the game is loaded.
            // It could be called multiple times if say, lockups and caused and cleared during recovery.
            // We ONLY want to call OnGamePlayEnabled one time when the game first is loaded.
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_gameRecovery.IsRecovering)
                    {
                        // Start the Responsible Game Timer since we have completed the Recovery Game Load
                        _responsibleGaming?.OnGamePlayEnabled();
                    }

                    if (Config.DisplaySessionTimeInClock)
                    {
                        SetupTimeRemainingDataForRuntime();
                    }

                    if (_broadcastDisableCountdownMessagePending)
                    {
                        // In case 'DisableCountdownWindow' is still visible, we need to remove it as the message is going to be shown
                        // by broadcasting the 'Disable Countdown' message to the game
                        LobbyView.CloseDisableCountdownWindow();
                        BroadcastInitialDisableCountdownMessage();
                        _broadcastDisableCountdownMessagePending = false;
                    }

                    SendTrigger(LobbyTrigger.GameLoaded);

                    if (_gameRecovery.IsRecovering)
                    {
                        // runtime crash while printing causes the message to get displayed but not cleared
                        if (ContainsAnyState(LobbyState.CashOut))
                        {
                            _lobbyStateManager.RemoveFlagState(LobbyState.CashOut, true);
                        }
                    }
                });
        }

        private void HandleEvent(GameExitedNormalEvent platformEvent)
        {
            _normalGameExitReceived = true;
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Logger.Debug("GameExitedNormalEvent received.");
                    Logger.Debug("Sending Game Normal Exit Trigger");
                    SendTrigger(LobbyTrigger.GameNormalExit);
                });
        }

        private void HandleEvent(GameProcessExitedEvent platformEvent)
        {
            // Checking to make sure we got a normal exit first because
            // We once got a GameProcessExitedEvent with Unexpected = false
            // without ever getting a GameExitedNormalEvent
            // so we didn't fire off any Triggers and the box hung.
            var unexpected = platformEvent.Unexpected || !_normalGameExitReceived;
            _normalGameExitReceived = false;

            // If we are trying to load a game and we got an "expected" exit, it was probably just
            // us killing the previously loading game. If we initiate recovery again then we'll end
            // up stuck in a loop forever. See TXM-5429 for a more detailed explanation.
            if (CurrentState == LobbyState.GameLoading && !platformEvent.Unexpected)
            {
                Logger.Warn("Game recovery loop detected, ignoring GameProcessExitedEvent!");
                return;
            }

            MvvmHelper.ExecuteOnUI(() =>
            {
                Logger.Debug($"GameProcessExitedEvent received.  Unexpected: {platformEvent.Unexpected}");

                // Moving check for recovery outside of check for unexpected.  We sometimes shut
                // down the game process ourselves and get an "expected" game process exited event,
                // but still need to do recovery.

                // 1) Added IsDisabled check for VLT-2112.  If the process is killed while we are
                // locked up, then do not recover now.  We will recover upon coming out of lockup.
                if (_gameHistory.IsRecoveryNeeded && !_systemDisableManager.DisableImmediately)
                {
                    Logger.Debug("Sending InitiateRecovery Trigger");
                    SendTrigger(
                        LobbyTrigger.InitiateRecovery,
                        CurrentState == LobbyState.Game &&
                        unexpected); //only check with runtime if we get an unexpected exit during game state.
                }
                else if (IsSingleGameMode && BaseState == LobbyState.Chooser && !IsInOperatorMenu)
                {
                    Logger.Debug("Trying to relaunch game after exit");
                    TryLaunchSingleGame();
                }
                else
                {
                    //checking to make sure we got a normal exit first because
                    //We once got a GameProcessExitedEvent with Unexpected = false
                    //without ever getting a GameExitedNormalEvent
                    //so we didn't fire off any Triggers and the box hung.

                    // If a game crashes, it is critical that we fire this trigger.
                    // Otherwise we end up in a bad state and the box can be locked.
                    if (unexpected)
                    {
                        Logger.Debug("Sending GameUnexpectedExit Trigger");
                        SendTrigger(LobbyTrigger.GameUnexpectedExit);
                    }
                }
            });
        }

        private void HandleEvent(BankBalanceChangedEvent platformEvent)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    Credits = OverlayMessageUtils.ToCredits(platformEvent.NewBalance);
                    CheckForExitGame();
                    OnUserInteraction();

                    if (ContainsAnyState(LobbyState.CashOutFailure) &&
                        _bank.QueryBalance(AccountType.NonCash) == 0)
                    {
                        _lobbyStateManager.RemoveFlagState(LobbyState.CashOutFailure);
                    }

                    HandleMessageOverlayText();
                });
        }

        private void HandleEvent(CurrencyInStartedEvent platformEvent)
        {
            Logger.Debug("Detected CurrencyInStartedEvent");
            if (CurrentState != LobbyState.Disabled)
            {
                CashInStarted(CashInType.Currency);
            }
        }

        private void HandleEvent(VoucherRedemptionRequestedEvent platformEvent)
        {
            Logger.Debug("Detected VoucherRedemptionRequestedEvent");
            if (CurrentState != LobbyState.Disabled)
            {
                CashInStarted(CashInType.Voucher);
            }
        }

        private void HandleEvent(VoucherRedeemedEvent platformEvent)
        {
            Logger.Debug("Detected VoucherRedeemedEvent");
            if (!Config.RemoveIdlePaidMessageOnSessionStart)
            {
                UpdatePaidMeterValue(0);
            }
            HandleCompletedMoneyIn(platformEvent.Transaction.Amount);
        }

        private void HandleEvent(VoucherRejectedEvent platformEvent)
        {
            Logger.Debug("Detected VoucherRejectedEvent");
            CashInFinished();
        }

        private void HandleEvent(CurrencyInCompletedEvent platformEvent)
        {
            Logger.Debug($"Detected CurrencyInCompletedEvent.  Amount: {platformEvent.Amount}");
            _disableDebugCurrency = false;
            _debugCurrencyTimer?.Stop();
            HandleCompletedMoneyIn(platformEvent.Amount, platformEvent.Amount > 0);
        }

        private void HandleEvent(WatOnStartedEvent watOnEvent)
        {
            Logger.Debug("Detected WatOnStartedEvent.");
            if (CurrentState != LobbyState.Disabled)
            {
                CashInStarted(CashInType.Wat, false);
            }
        }

        private void HandleEvent(WatOnCompleteEvent watOnEvent)
        {
            Logger.Debug($"Detected WatOnCompleteEvent.  Amount: {watOnEvent.Transaction.TransactionAmount}");
            _disableDebugCurrency = false;
            _debugCurrencyTimer?.Stop();
            HandleCompletedMoneyIn(watOnEvent.Transaction.TransactionAmount);
        }

        private void HandleEvent(BonusStartedEvent bonusEvent)
        {
            Logger.Debug("Detected BonusStartedEvent.");
            if (CurrentState != LobbyState.Disabled &&
                bonusEvent.Transaction.Mode != BonusMode.GameWin)
            {
                CashInStarted(CashInType.Wat);
            }
            
            if (bonusEvent.Transaction.Mode == BonusMode.GameWin &&
                bonusEvent.Transaction.PayMethod == PayMethod.Voucher)
            {
                _forcedCashOutData.Enqueue(true);
            }
        }

        private void HandleEvent(PartialBonusPaidEvent bonusEvent)
        {
            Logger.Debug($"Detected PartialBonusPaidEvent.  Amount: {bonusEvent.Transaction.PaidAmount}");
            HandleCompletedMoneyIn(bonusEvent.Transaction.PaidAmount, false);
        }

        private void HandleEvent(BonusAwardedEvent bonusEvent)
        {
            Logger.Debug($"Detected BonusAwardedEvent.  Amount: {bonusEvent.Transaction.PaidAmount}");
            HandleCompletedMoneyIn(bonusEvent.Transaction.PaidAmount, false);
        }

        private void HandleEvent(BonusFailedEvent bonusEvent)
        {
            Logger.Debug($"Detected BonusFailedEvent.  Transaction: {bonusEvent.Transaction}");
            HandleCompletedMoneyIn(bonusEvent.Transaction.PaidAmount, false);
        }

        private void HandleCompletedMoneyIn(long amount, bool playSound = true)
        {
            Logger.Debug($"HandleCompletedMoneyIn.  Amount: {amount}");
            if (playSound)
            {
                PlayAudioFile(Sound.CoinIn);
            }

            CashInFinished();
        }

        private void HandleEvent(TransferOutCompletedEvent platformEvent)
        {
            Logger.Debug("Detected TransferOutCompletedEvent");
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (HasZeroCredits && _gameState.Idle) // VLT-5401: Handle Manitoba partial cash-out
                    {
                        _responsibleGamingCashOutInProgress = false;
                        _responsibleGaming?.EndResponsibleGamingSession();
                    }

                    switch (CashOutDialogState)
                    {
                        case LobbyCashOutDialogState.Visible:
                            CashOutDialogState = LobbyCashOutDialogState.VisiblePendingTimeout;
                            break;
                        case LobbyCashOutDialogState.VisiblePendingCompletedEvent:
                            ClearCashOutDialog(true);
                            break;
                    }

                    // Wait until CashOut state is exited to set CashOutState to Undefined
                });
        }

        private void HandleEvent(TransferOutFailedEvent platformEvent)
        {
            Logger.Debug("Detected TransferOutFailedEvent");
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    // If Responsible Gaming is running and we failed to cash out
                    // then we need to redisplay the Responsible Gaming Dialog
                    // Also check for now that the bank isn't zeroed out, since this error can occur
                    // even though a cash out has been handled and stored in the system for the operator to reprint the ticket
                    // The bank part can probably be removed once future changes to the Transfer Out code occur
                    if ((_responsibleGamingSessionState == ResponsibleGamingSessionState.Started ||
                         _responsibleGamingSessionState == ResponsibleGamingSessionState.Paused)
                        && _responsibleGamingCashOutInProgress && !HasZeroCredits &&
                        !(_responsibleGaming?.IsTimeLimitDialogVisible ?? false))
                    {
                        Logger.Debug("RG Dialog was up before cash out--Bring the RG Dialog back up");
                        _responsibleGaming?.ResetDialog(false);
                    }

                    _responsibleGamingCashOutInProgress = false;

                    ClearCashOutDialog(false);

                    _lobbyStateManager.RemoveFlagState(LobbyState.CashOut, false);

                    if (Config.NonCashCashoutFailureMessageEnabled && platformEvent.NonCashableAmount > 0)
                    {
                        _lobbyStateManager.AddFlagState(LobbyState.CashOutFailure);
                    }

                    MessageOverlayDisplay.LastCashOutForcedByMaxBank = false;
                });
        }

        private void HandleEvent(TransferOutStartedEvent platformEvent)
        {
            Logger.Debug(
                $"Detected TransferOutStartedEvent.  Amount: {platformEvent.Total}  ResponsibleGamingCashOutInProgress: {_responsibleGamingCashOutInProgress}");

            if (!_responsibleGamingCashOutInProgress)
            {
                // If we haven't already determined that this was a Responsible Gaming Related Cashout
                // check to see if the RG Dialog is up.
                _responsibleGamingCashOutInProgress = IsTimeLimitDlgVisible;
                Logger.Debug($"Setting ResponsibleGamingCashOutInProgress to {_responsibleGamingCashOutInProgress}");
            }

            _lobbyStateManager.CashOutState = LobbyCashOutState.Undefined;

            if (_forcedCashOutData.TryDequeue(out var forcedByMaxBank))
            {
                if (_bank.QueryBalance().MillicentsToCents() + platformEvent.Total.MillicentsToCents() >
                    ((long)_properties.GetProperty(AccountingConstants.MaxCreditMeter, long.MaxValue))
                    .MillicentsToCents())
                {
                    MessageOverlayDisplay.LastCashOutForcedByMaxBank = forcedByMaxBank;
                }
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    CashOutDialogState = LobbyCashOutDialogState.Visible;
                    _cashOutTimer?.Stop();
                    _cashOutTimer?.Start();
                    _lobbyStateManager.AddFlagState(LobbyState.CashOut);
                });
        }

        private void HandleEvent(CashOutStartedEvent platformEvent)
        {
            Logger.Debug($"Detected CashOutStartedEvent.  Forced By Max Bank: {platformEvent.ForcedByMaxBank}");
            _forcedCashOutData.Enqueue(platformEvent.ForcedByMaxBank);
        }

        private void HandleEvent(CashOutAbortedEvent platformEvent)
        {
            Logger.Debug("Detected CashOutAbortedEvent");
            // Dequeue the forced cashout data from this failed operation.
            _forcedCashOutData.TryDequeue(out _);

            MvvmHelper.ExecuteOnUI(UpdateUI);
        }

        private void HandleEvent(WatTransferInitiatedEvent platformEvent)
        {
            SetEdgeLighting();
            PlayAudioFile(Sound.CoinOut);
        }

        private void HandleEvent(WatTransferCommittedEvent platformEvent)
        {
            Logger.Debug($"Detected WatTransferCommittedEvent.  Amount: {platformEvent.Transaction.TransactionAmount}");
            if (platformEvent.Transaction.Direction == WatDirection.HostInitiated && BaseState == LobbyState.Chooser)
            {
                StartAttractTimer();
            }
        }

        private void HandleEvent(VoucherOutStartedEvent platformEvent)
        {
            SetEdgeLighting();
            PlayAudioFile(Sound.CoinOut);
        }

        private void HandleEvent(HandpayStartedEvent platformEvent)
        {
            Logger.Debug($"Detected HandpayStartedEvent.  HandpayType: {platformEvent.Handpay}");

            SetEdgeLighting();

            var payResetMethod = _properties.GetValue(AccountingConstants.LargeWinHandpayResetMethod, LargeWinHandpayResetMethod.PayByHand);
            if (payResetMethod == LargeWinHandpayResetMethod.PayByMenuSelection &&
                platformEvent.EligibleResetToCreditMeter)
            {
                _eventBus.Subscribe<DownEvent>(
                    this,
                    evt =>
                    {
                        if (!IsSelectPayModeVisible)
                        {
                            IsSelectPayModeVisible = true;
                            _properties.SetProperty(AccountingConstants.MenuSelectionHandpayInProgress, true);
                            SelectedMenuSelectionPayOption = MenuSelectionPayOption.ReturnToLockup;
                        }
                        else
                        {
                            IsSelectPayModeVisible = false;
                            _properties.SetProperty(AccountingConstants.MenuSelectionHandpayInProgress, false);
                            if (_selectedMenuSelectionPayOption != MenuSelectionPayOption.ReturnToLockup)
                            {
                                _eventBus.Unsubscribe<DownEvent>(this);
                            }
                        }
                    }, evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
            }

            if (platformEvent.Handpay == HandpayType.GameWin)
            {
                PlayGameWinHandPaySound();
            }
            else
            {
                PlayAudioFile(Sound.CoinOut);
            }
        }

        private void HandleEvent(HandpayCanceledEvent platformEvent)
        {
            _eventBus.Unsubscribe<DownEvent>(this);
            _lobbyStateManager.CashOutState = LobbyCashOutState.Undefined;
        }
        private void HandleEvent(HandpayKeyedOffEvent platformEvent)
        {
            _eventBus.Unsubscribe<DownEvent>(this);

            if (platformEvent.Transaction.HandpayType == HandpayType.GameWin)
            {
                _playCollectSound = false;
                _audio.Stop();
            }

            SetEdgeLighting();
        }

        private void HandleEvent(ReprintTicketEvent platformEvent)
        {
            Logger.Debug("Detected ReprintTicketEvent.");

            if (_gameState.Idle)
            {
                _printingReprintTicket = true;

                MvvmHelper.ExecuteOnUI(
                    () => _lobbyStateManager.AddFlagState(LobbyState.CashOut, platformEvent.Amount, false));
            }
        }

        private void HandleEvent(GamePlayDisabledEvent gameDisabledEvent)
        {
            MvvmHelper.ExecuteOnUI(() => SendTrigger(LobbyTrigger.Disable));
        }

        private void HandleEvent(GamePlayEnabledEvent gameEnabledEvent)
        {
            // Restore the fast-launch capability after tilts.
            _lobbyStateManager.AllowGameAutoLaunch = true;
            MvvmHelper.ExecuteOnUI(() => SendTrigger(LobbyTrigger.Enable));

            // If game is ready but not loaded due to disable, load it now
            if (GameReady)
            {
                MvvmHelper.ExecuteOnUI(() => SendTrigger(LobbyTrigger.GameLoaded));
            }
        }

        private void HandleEvent(ViewResizeEvent viewResizeEvent)
        {
            Logger.Debug($"Detected ViewResizeEvent Resizing:{viewResizeEvent.Resizing}");
            if (viewResizeEvent.Resizing)
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        _lobbyStateManager.AddFlagState(LobbyState.MediaPlayerResizing);
                        SendTrigger(LobbyTrigger.AttractModeExit);
                        _responsibleGaming?.OnGamePlayDisabled();
                        UpdateUI();
                    });
            }
            else
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        _lobbyStateManager.RemoveFlagState(LobbyState.MediaPlayerResizing);
                        _responsibleGaming?.OnGamePlayEnabled();
                        UpdateUI();
                    });
            }
        }

        private void HandleEvent(PrimaryOverlayMediaPlayerEvent primaryOverlayEvent)
        {
            if (primaryOverlayEvent.IsShowing)
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        _lobbyStateManager.AddFlagState(LobbyState.MediaPlayerOverlay);
                        _responsibleGaming?.OnGamePlayDisabled();
                    });
            }
            else
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        _lobbyStateManager.RemoveFlagState(LobbyState.MediaPlayerOverlay);
                        _responsibleGaming?.OnGamePlayEnabled();
                    });
            }
        }

        private void HandleEvent(GameDiagnosticsStartedEvent replayStartedEvent)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    var game =
                        (from g in _properties.GetValues<IGameDetail>(GamingConstants.Games)
                         where g.Id == replayStartedEvent.GameId
                         let denom = replayStartedEvent.Denomination
                         select ToGameInfo(g, denom))
                        .Single();

                    SendTrigger(LobbyTrigger.LaunchGameForDiagnostics, game);
                });
        }

        private void HandleEvent(GameDiagnosticsCompletedEvent replayCompletedEvent)
        {
            Logger.Debug("Detected GameDiagnosticsCompletedEvent");
            MvvmHelper.ExecuteOnUI(() => SendTrigger(LobbyTrigger.GameDiagnosticsExit));
        }

        private void HandleEvent(GameOrderChangedEvent evt)
        {
            // The game info needs to be reloaded, since we can't be certain no other attributes around the game have changed
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    OnUserInteraction();
                    LoadGameInfo();
                });
        }

        private void HandleEvent(GameAddedEvent added)
        {
            // This could be done better by only adding the game the specific game id
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    OnUserInteraction();
                    _gameOrderSettings.OnGameAdded(added.ThemeId);
                    LoadGameInfo();
                });
        }

        private void HandleEvent(GameRemovedEvent removed)
        {
            // This could be done better by only removing the game the specific game id
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    OnUserInteraction();
                    // Do not remove games from game order when we remove them.
                    LoadGameInfo();
                });
        }

        private void HandleEvent(GameEnabledEvent enabled)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    LoadGameInfo();
                    SetTabViewToDefault();
                });
        }

        private void HandleEvent(GameDisabledEvent disabled)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    LoadGameInfo();
                    SetTabViewToDefault();
                });
        }

#if !(RETAIL)
        private void HandleEvent(GameLoadRequestedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    var game = GameList.FirstOrDefault(g => g.GameId == evt.GameId && g.FilteredDenomination == evt.Denomination);
                    LaunchGameFromUi(game);
                });
        }

        private void HandleEvent(DebugCurrencyAcceptedEvent evt)
        {
            // need to set this so that debug currency can start Responsible Gaming
            if (HasZeroCredits)
            {
                _responsibleGaming?.OnInitialCurrencyIn();
            }
        }
#endif

        private void HandleEvent(GameFatalErrorEvent evt)
        {
            // VLT-3544: Legitimacy lock up screen should turn black. Legitimacy check
            // is fatal game error, and so can only be cleared by a RAM clear, so we do
            // not need to reset the opacity.

            // Note: Not sure this is needed anymore, but leaving it just in case
            MvvmHelper.ExecuteOnUI(() => MessageOverlayDisplay.MessageOverlayData.Opacity = 1.0);
        }

        private void HandleEvent(GameTagsChangedEvent evt)
        {
            if (evt.Game == null)
            {
                return;
            }

            var game = _gameList.FirstOrDefault(g => g.GameId == evt.Game.Id && g.ThemeId == evt.Game.ThemeId);
            SetGameAsNew(game, evt.Game.GameTags);
        }

        private void HandleEvent(GameRequestedLobbyEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    // Normal game exit, if >1 game or old-style behavior is desired
                    if (!_lobbyStateManager.AllowSingleGameAutoLaunch)
                    {
                        _gameService.ShutdownBegin();
                        return;
                    }

                    if (evt.ImmediateAttract)
                    {
                        if (MessageOverlayDisplay.ShowVoucherNotification)
                        {
                            // We don't want to stop the game rendering in these cases
                            return;
                        }

                        // Cause an immediate attract sequence
                        AttractTimer_Tick(null, EventArgs.Empty);
                    }
                    else
                    {
                        // Fool the lobby into thinking that a game exited, so normal lobby behavior will ensue.
                        // (it hasn't really exited, but that won't matter till attempt to launch again)
                        _lobbyStateManager.AllowGameAutoLaunch = false;
                        _lobbyStateManager.SendTrigger(LobbyTrigger.GameNormalExit);
                        _eventBus.Publish(new ControlGameRenderingEvent(false));
                    }
                });
        }

        private void HandleEvent(PropertyChangedEvent evt)
        {
            if (evt.PropertyName == GamingConstants.IdleText)
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        IdleText = (string)_properties.GetProperty(GamingConstants.IdleText, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.IdleTextDefault));
                    });
            }
        }

        private void HandleEvent(GameIdleEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (HasZeroCredits)
                    {
                        // if we have 0 credits at the end of a play round, end the Responsible Gaming Session
                        _responsibleGaming?.EndResponsibleGamingSession();
                        CheckForExitGame();
                    }
                    else if (_responsibleGaming != null && _responsibleGaming.ShowTimeLimitDlgPending)
                    {
                        OnResponsibleGamingDialogPending();
                    }

                    UpdateUI();
                });
        }

        private void HandleEvent(OperatorMenuEnteredEvent evt)
        {
            RaisePropertyChanged(nameof(IsInOperatorMenu));
            UpdateLcdButtonDeckRenderSetting(false);
            // VLT-4426: Need to remove the Responsible Gaming Dialog while the Operator Menu is up.
            Logger.Debug("Clearing Responsible Gaming Dialog For Operator Menu");

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    // VLT-4160:  Set this so that we can reset localization after going to the Operator Menu
                    // A few pages in the operator menu share resources with the lobby, but the Operator Menu
                    // is not localized to the same language as the lobby.  Currently we change the Resource 
                    // Culture when we move to the Operator Menu so that everything works and then change it back
                    // when we exit the Operator Menu.  We probably need a better solution in the future.    

                    _printingHelplineWhileResponsibleGamingReset = ContainsAnyState(LobbyState.PrintHelpline);
                    _responsibleGamingInfoWhileResponsibleGamingReset = ContainsAnyState(
                        LobbyState.ResponsibleGamingInfoLayeredLobby,
                        LobbyState.ResponsibleGamingInfoLayeredGame);

                    //if (Resources.Culture.Name.ToUpper() != EnglishCultureCode)
                    //{
                    //    Resources.Culture = new CultureInfo(EnglishCultureCode);
                    //}

                    // set this so that if we go to the operator menu while a responsible gaming dialog is up, 
                    // we can restore the dialog on operator menu exit.
                    _responsibleGamingDialogResetWhenOperatorMenuEntered = ResetResponsibleGamingDialog(true);

                    if (ContainsAnyState(LobbyState.AgeWarningDialog))
                    {
                        Logger.Debug("Clearing Age Warning Dialog For Operator Menu");
                        _ageWarningTimer.Stop();
                        _lobbyStateManager.RemoveStackableState(LobbyState.AgeWarningDialog);
                        _launchGameAfterAgeWarning = null;
                    }

                    if (ContainsAnyState(LobbyState.CashOutFailure))
                    {
                        _lobbyStateManager.RemoveFlagState(LobbyState.CashOutFailure);
                    }

                    if (IsSingleGameMode)
                    {
                        _gameService.ShutdownBegin();
                    }

                    UpdateUI();
                });

            _audio.Stop();
        }

        private void HandleEvent(OperatorMenuExitedEvent evt)
        {
            RaisePropertyChanged(nameof(IsInOperatorMenu));
            UpdateLcdButtonDeckRenderSetting(!IsGameRenderingToLcdButtonDeck());

            if (_responsibleGaming?.ShowTimeLimitDlgPending ?? false)
            {
                var allowDialogWhileDisabled = _responsibleGamingDialogResetWhenOperatorMenuEntered;
                // VLT-4426: Need to put the Responsible Gaming Dialog back when the Operator Menu exits.
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        OnResponsibleGamingDialogPending(allowDialogWhileDisabled);
                        if (_responsibleGamingInfoWhileResponsibleGamingReset)
                        {
                            _lobbyStateManager.AddStackableState(
                                BaseState == LobbyState.Game
                                    ? LobbyState.ResponsibleGamingInfoLayeredGame
                                    : LobbyState.ResponsibleGamingInfoLayeredLobby);

                            if (_printingHelplineWhileResponsibleGamingReset)
                            {
                                _lobbyStateManager.AddStackableState(LobbyState.PrintHelpline);
                            }
                        }
                    });
            }

            _responsibleGamingDialogResetWhenOperatorMenuEntered = false;

            // VLT-4160:  Change back to the Resource Localization Culture that we had prior to 
            // entering the Operator Menu
            //if (Resources.Culture.Name.ToUpper() != _localeCodePreOperatorMenu)
            //{
            //    MvvmHelper.ExecuteOnUI(
            //        () => { Resources.Culture = new CultureInfo(_localeCodePreOperatorMenu); });
            //}

            RaisePropertiesChanged();

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    TryLaunchSingleGame();
                    UpdateUI();
                });
        }

        private void HandleEvent(PrimaryGameStartedEvent evt)
        {
            if (CashOutDialogState == LobbyCashOutDialogState.VisiblePendingTimeout)
            {
                MvvmHelper.ExecuteOnUI(() =>
                {
                    ClearCashOutDialog(true);
                });
            }

            if (IsInState(LobbyState.Chooser) ||
                CurrentState == LobbyState.Attract ||
                CurrentState == LobbyState.ResponsibleGamingInfo ||
                CurrentState == LobbyState.ResponsibleGamingTimeLimitDialog && IsInLobby)
            {
                // VLT-4408: We have received a PrimaryGameStartedEvent while in an unexpected state.  This may be due to a crash
                // Check to see if recovery is required.
                Logger.Debug("PrimaryGameStartedEvent received Unexpectedly");
                if (_gameHistory.IsRecoveryNeeded)
                {
                    Logger.Debug("Sending InitiateRecovery Trigger");
                    MvvmHelper.ExecuteOnUI(() => SendTrigger(LobbyTrigger.InitiateRecovery, false));
                }
            }
        }

        private void HandleEvent(GameEndedEvent evt)
        {
            if (!Config.RemoveIdlePaidMessageOnSessionStart && !MessageOverlayDisplay.ShowPaidMeterForAutoCashout)
            {
                UpdatePaidMeterValue(0);
            }

            MessageOverlayDisplay.ShowPaidMeterForAutoCashout = false;
        }

        private void HandleEvent(DisableCountdownTimerEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (evt.Start)
                    {
                        // If the Disable Timer is already running
                        // StartDisableCountdownTimer won't do anything
                        StartDisableCountdownTimer(evt.CountdownTime);
                    }
                    else
                    {
                        StopDisableCountdownTimer();
                    }
                });
        }

        private void HandleEvent(PrintCompletedEvent evt)
        {
            Logger.Debug("Detected PrintCompletedEvent");
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_printingHelplineTicket)
                    {
                        _printingHelplineTicket = false;
                        _printingHelplineWhileResponsibleGamingReset = false;
                        _inPrintHelplineTicketWaitPeriod = true;
                        _printHelplineTicketTimer?.Stop();
                        _printHelplineTicketTimer?.Start();
                        SendTrigger(LobbyTrigger.PrintHelplineComplete);
                    }

                    if (_printingReprintTicket && evt.FieldOfInterest)
                    {
                        _printingReprintTicket = false;
                        _lobbyStateManager.RemoveFlagState(LobbyState.CashOut, true);
                    }
                });
        }

        private void HandleEvent(FieldOfInterestEvent evt)
        {
            Logger.Debug("Detected FieldOfInterestEvent");
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (_printingReprintTicket)
                    {
                        _printingReprintTicket = false;
                        _lobbyStateManager.RemoveFlagState(LobbyState.CashOut, true);
                    }
                });
        }

        private void HandleEvent(DisplayingTimeRemainingChangedEvent evt)
        {
            Logger.Debug($"Detected DisplayingTimeRemainingChangedEvent {evt.IsDisplayingTimeRemaining}");
            if (Config.DisplaySessionTimeInClock)
            {
                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        ClockTimer.ChangeClockState(
                            evt.IsDisplayingTimeRemaining
                                ? LobbyClockState.ResponsibleGamingSessionTime
                                : LobbyClockState.Clock,
                            false,
                            true);
                    });
            }
        }

        private void HandleEvent(SetValidationEvent evt)
        {
            Logger.Debug("Detected SetValidationEvent");
            if (evt.Identity == null)
            {
                // logging out
                MvvmHelper.ExecuteOnUI(() => IsPrimaryLanguageSelected = true);
            }
            else
            {
                // logging in
                MvvmHelper.ExecuteOnUI(() => SetLanguage(evt.Identity.LocaleId));
            }
        }

        private void HandleEvent(TimeUpdatedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() => _lobbyStateManager.OnUserInteraction());
        }

        private void HandleEvent(ShowServiceConfirmationEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() => ShowVbdServiceConfirmationDialog(evt.Show));
        }

        private void HandleEvent(MissedStartupEvent evt)
        {
            Logger.Debug($"Detected MissedStartupEvent:  {evt.MissedEvent.GetType()}");
            dynamic param = evt.MissedEvent;

            HandleEvent(param);
        }

        /// <summary>
        /// This is to handle missed events not handled by LobbyViewModel.
        /// </summary>
        /// <param name="evt"></param>
        // ReSharper disable once UnusedParameter.Local
        private static void HandleEvent(IEvent evt)
        {
            //no implementation intentionally
        }


        private void HandleEvent(HandpayCompletedEvent evt)
        {
            // if the handpay has completed, restart the cash out dialog timer
            _cashOutTimer?.Stop();
            _cashOutTimer?.Start();
        }

        private void HandleEvent(HandpayKeyOffPendingEvent evt)
        {
            // We are waiting for a handpay key off--stop the cash out dialog timer and reset the dialog state
            _cashOutTimer?.Stop();
            CashOutDialogState = LobbyCashOutDialogState.Visible;
        }

        private void HandleEvent(SessionInfoEvent evt)
        {
            if (evt.SessionInfoValue > 0 || Config.RemoveIdlePaidMessageOnSessionStart)
            {
                UpdatePaidMeterValue(evt.SessionInfoValue);
            }
        }

        private void HandleEvent(CultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                HandleMessageOverlayText();

                if (evt is PlayerCultureChangedEvent)
                {
                    // todo let player culture provider manage multi-language support for lobby
                    // IsPrimaryLanguageSelected = playerCultureChanged.IsPrimary;
                }
            });
        }

        private void HandleEvent(LobbySettingsChangedEvent evt)
        {
            switch (evt.SettingType)
            {
                case LobbySettingType.ServiceButtonVisible:
                    GetServiceButtonVisible();
                    break;
                case LobbySettingType.ShowTopPickBanners:
                    MvvmHelper.ExecuteOnUI(LoadGameInfo);
                    break;
            }
        }

        private void HandleEvent(CurrencyCultureChangedEvent evt)
        {
            RaisePropertyChanged(nameof(FormattedCredits));
        }

        private void HandleEvent(GameDenomChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(LoadGameInfo);
        }

        private void HandleEvent(AttractConfigurationChangedEvent evt)
        {
            RefreshAttractGameList();
        }

        private void HandleEvent(DenominationSelectedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() => DenominationSelectionChanged(evt.GameId, evt.Denomination));
        }

        private void HandleEvent(InfoBarDisplayTransientMessageEvent evt) => RequestInfoBarOpen(evt.DisplayTarget, true);

        private void HandleEvent(InfoBarDisplayStaticMessageEvent evt) => RequestInfoBarOpen(evt.DisplayTarget, true);

        private void HandleEvent(InfoBarCloseEvent evt) => RequestInfoBarOpen(evt.DisplayTarget, false);

        private void HandleEvent(SystemDisableRemovedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (evt.DisableId == ApplicationConstants.LiveAuthenticationDisableKey)
                    {
                        CheckForExitGame();
                    }

                    if (_gameRecovery.IsRecovering && !_systemDisableManager.CurrentDisableKeys.Any() &&
                        !_gameService.Running)
                    {
                        LaunchGameOrRecovery();
                    }

                    UpdateUI();
                });
        }

        protected virtual void OnCustomEventViewChangedEvent(ViewInjectionEvent evt)
        {
            CustomEventViewChangedEvent?.Invoke(evt);
        }

        private void HandleEvent(TransferEnableOnOverlayEvent evt)
        {
            if (evt != null)
            {
                HandleMessageOverlayText();
            }
        }

        private void HandleEvent(PlayerMenuButtonPressedEvent evt)
        {
            if (MessageOverlayDisplay.MessageOverlayData.IsDialogFadingOut)
            {
                return;
            }

            MessageOverlayDisplay.MessageOverlayData.IsDialogFadingOut = !evt.Show;
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (evt.Show)
                    {
                        PlayerMenuPopupViewModel.IsMenuVisible = true;

                        // Reset the attract timer so that it doesn't close while 
                        // a player is adjusting the volume or brightness
                        StartAttractTimer();
                    }
                    else
                    {
                        PlayerMenuPopupViewModel.IsMenuVisible = false;
                    }

                    HandleMessageOverlayVisibility();
                });
        }

        private void HandleEvent(PlayerInfoDisplayEnteredEvent @event)
        {
            Logger.Debug("Player Info Display On");
            MvvmHelper.ExecuteOnUI(HandleMessageOverlayVisibility);
        }

        private void HandleEvent(PlayerInfoDisplayExitedEvent @event)
        {
            Logger.Debug("Player Info Display Off");
            MvvmHelper.ExecuteOnUI(HandleMessageOverlayVisibility);
        }

        private void HandleEvent(GambleFeatureActiveEvent evt)
        {
            _isGambleFeatureActive = evt.Active;
            RaisePropertyChanged(nameof(ReturnToLobbyAllowed));
            RaisePropertyChanged(nameof(CashOutEnabledInPlayerMenu));
            RaisePropertyChanged(nameof(ReserveMachineAllowed));
        }

        private void HandleMessageOverlayVisibility()
        {
            MessageOverlayDisplay.HandleOverlayWindowDialogVisibility();
        }
    }
}
