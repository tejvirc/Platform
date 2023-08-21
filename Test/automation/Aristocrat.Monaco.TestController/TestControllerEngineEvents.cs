namespace Aristocrat.Monaco.TestController
{
    using Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Hardware.Contracts.TowerLight;
    using DataModel;
    using G2S.Common.Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Lobby;
    using Gaming.Diagnostics;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using System;
    using Gaming.Contracts.Events;
    using Aristocrat.Monaco.Gaming.UI.Events;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;

    public partial class TestControllerEngine
    {
        private const int vouchersIssuedLimit = 30;

        public void SubscribeToEvents()
        {
            _eventBus.Subscribe<TimeLimitDialogVisibleEvent>(
                this,
                _ =>
                {
                    CheckForWait(typeof(TimeLimitDialogVisibleEvent));
                    HandleTimeLimitDialog();
                });

            _eventBus.Subscribe<TimeLimitDialogHiddenEvent>(
                this,
                _ => { CheckForWait(typeof(TimeLimitDialogHiddenEvent)); });

            _eventBus.Subscribe<GameSelectedEvent>(this, _ => { CheckForWait(typeof(GameSelectedEvent)); });

            _eventBus.Subscribe<LobbyInitializedEvent>(
               this,
               _ =>
               {
                   _platformState |= PlatformStateEnum.InLobby;
                   CheckForWait(typeof(LobbyInitializedEvent));
               });

            _eventBus.Subscribe<GameProcessExitedEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.GamePlaying;
                    _platformState &= ~PlatformStateEnum.GameIdle;
                    _platformState &= ~PlatformStateEnum.GameLoaded;
                    _platformState |= PlatformStateEnum.InLobby;
                    CheckForWait(typeof(GameProcessExitedEvent));
                });

            _eventBus.Subscribe<GameInitializationCompletedEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.InLobby;
                    _platformState |= PlatformStateEnum.GameLoaded;
                    _platformState |= PlatformStateEnum.GameIdle;
                    CheckForWait(typeof(GameInitializationCompletedEvent));
                });

            _eventBus.Subscribe<GameIdleEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.GamePlaying;
                    _platformState |= PlatformStateEnum.GameIdle;
                    CheckForWait(typeof(GameIdleEvent));
                });

            _eventBus.Subscribe<PrimaryGameStartedEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.GameIdle;
                    _platformState |= PlatformStateEnum.GamePlaying;
                    CheckForWait(typeof(PrimaryGameStartedEvent));
                });

            _eventBus.Subscribe<OperatorMenuEnteredEvent>(
                this,
                _ =>
                {
                    _platformState |= PlatformStateEnum.InAudit;
                    CheckForWait(typeof(OperatorMenuEnteredEvent));
                });

            _eventBus.Subscribe<OperatorMenuExitedEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.InAudit;
                    CheckForWait(typeof(OperatorMenuExitedEvent));
                });

            _eventBus.Subscribe<GamePlayDisabledEvent>(
                this,
                _ =>
                {
                    _platformState |= PlatformStateEnum.GamePlayDisabled;
                    CheckForWait(typeof(GamePlayDisabledEvent));
                });

            _eventBus.Subscribe<GamePlayEnabledEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.GamePlayDisabled;
                    CheckForWait(typeof(GamePlayEnabledEvent));
                });

            _eventBus.Subscribe<SystemDisableAddedEvent>(
                this,
                evt =>
                {
                    try
                    {
                        _currentLockups.AddOrUpdate(evt.DisableId, evt.DisableReasons, (k, v) => evt.DisableReasons);
                    }
                    catch(Exception ex)
                    {
                        _logger.Info(ex.ToString());
                    }
                    _platformState |= PlatformStateEnum.SystemDisabled;
                    CheckForWait(typeof(SystemDisableAddedEvent));
                });

            _eventBus.Subscribe<SystemDisableRemovedEvent>(
                this,
                evt =>
                {
                    try
                    {
                        _currentLockups.TryRemove(evt.DisableId, out string value);
                    }
                    catch(Exception ex)
                    {
                        _logger.Info(ex.ToString());
                    }
                });

            _eventBus.Subscribe<SystemDisableUpdatedEvent>(
                this,
                evt =>
                {
                    try
                    {
                        _currentLockups.AddOrUpdate(evt.DisableId, evt.DisableReasons, (k, v) => evt.DisableReasons);
                    }
                    catch (Exception ex)
                    {
                        _logger.Info(ex.ToString());
                    }
                });

            _eventBus.Subscribe<SystemEnabledEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.SystemDisabled;
                    CheckForWait(typeof(SystemEnabledEvent));
                });

            _eventBus.Subscribe<RecoveryStartedEvent>(
                this,
                _ =>
                {
                    _platformState |= PlatformStateEnum.InRecovery;
                    _runtimeMode = RuntimeMode.Recovery;
                    CheckForWait(typeof(RecoveryStartedEvent));
                });

            _eventBus.Subscribe<PrimaryGameEndedEvent>(
                this,
                _ =>
                {
                    _platformState &= ~PlatformStateEnum.InRecovery;
                    _runtimeMode = RuntimeMode.Regular;
                    CheckForWait(typeof(PrimaryGameEndedEvent));
                });

            _eventBus.Subscribe<GameResultEvent>(
                this,
                  evt =>
                  {
                  });

            _eventBus.Subscribe<IdPresentedEvent>(
                this,
                _ => { CheckForWait(typeof(IdPresentedEvent)); });

            _eventBus.Subscribe<IdClearedEvent>(
                this,
                _ => { CheckForWait(typeof(IdClearedEvent)); });

            _eventBus.Subscribe<ReadErrorEvent>(
                this,
                _ => { CheckForWait(typeof(ReadErrorEvent)); });

            _eventBus.Subscribe<IdReaderTimeoutEvent>(
                this,
                _ => { CheckForWait(typeof(IdReaderTimeoutEvent)); });

            _eventBus.Subscribe<SetValidationEvent>(
                this,
                evt =>
                {
                    var type = typeof(DummyEvent_IdNullEvent);

                    if (evt.Identity != null)
                    {
                        type = evt.Identity.Type != IdTypes.Invalid
                            ? typeof(DummyEvent_IdValidEvent)
                            : typeof(DummyEvent_IdInvalidEvent);
                    }

                    CheckForWait(type);
                });

            _eventBus.Subscribe<CommunicationsStateChangedEvent>(
                this,
                _ => _platformState |= PlatformStateEnum.InLobby);

            _eventBus.Subscribe<TowerLightOffEvent>(
                this,
                evt =>
                {
                    _towerLightStates[(int)evt.LightTier] = false;
                });

            _eventBus.Subscribe<TowerLightOnEvent>(
                this,
                evt =>
                {
                    _towerLightStates[(int)evt.LightTier] = true;
                });

            _eventBus.Subscribe<VoucherIssuedEvent>(this, evt =>
            {
                _lastVoucherIssued = evt;
                if (_vouchersIssued.Count >= vouchersIssuedLimit)
                    _vouchersIssued.RemoveAt(0);
                _vouchersIssued.Add(_lastVoucherIssued);
            });

            _eventBus.Subscribe<Hardware.Contracts.Printer.HardwareFaultEvent>(this, evt => _printerFaults |= evt.Fault);

            _eventBus.Subscribe<Hardware.Contracts.Printer.HardwareFaultClearEvent>(this, evt => _printerFaults &= ~evt.Fault);

            _eventBus.Subscribe<Hardware.Contracts.Printer.HardwareWarningEvent>(this, evt => _printerWarnings |= evt.Warning );

            _eventBus.Subscribe<Hardware.Contracts.Printer.HardwareWarningClearEvent>(this, evt => _printerWarnings &= ~evt.Warning);

            _eventBus.Subscribe<MessageAddedEvent>(this, evt =>
            {
                if (!string.IsNullOrEmpty(evt.Message.Message))
                {
                    _gameLineMessages.Add(evt.Message);
                }
            });

            _eventBus.Subscribe<MessageOverlayDataEvent>(this, evt =>
            {
                // Only assign non-empty, since some overlay message comes/goes quickly, it won't be possible to fetch them with API call, so the last overlay message can be fetched to verify.
                if ((!string.IsNullOrWhiteSpace(evt.Message.Text) && !string.IsNullOrEmpty(evt.Message.Text)) || evt.Message.IsSubTextVisible || evt.Message.IsSubText2Visible)
                    _messageOverlayData = new MessageOverlayData
                    {
                        Text = evt.Message.Text,
                        SubText = evt.Message.SubText,
                        SubText2 = evt.Message.SubText2,
                        IsSubTextVisible = evt.Message.IsSubTextVisible,
                        IsSubText2Visible = evt.Message.IsSubText2Visible
                    };
            });

            _eventBus.Subscribe<MessageRemovedEvent>(this, evt =>
            {
                if (!string.IsNullOrEmpty(evt.Message.Message))
                {
                    _gameLineMessages.Remove(evt.Message);
                }
            });

            _eventBus.Subscribe<GameDiagnosticsStartedEvent>(this, evt =>
            {
                if(evt.Context.GetType() == typeof(CombinationTestContext))
                {
                    _runtimeMode = RuntimeMode.Combination;
                }
                else if (evt.Context.GetType() == typeof(ReplayContext))
                {
                    _runtimeMode = RuntimeMode.Replay;
                }
            });

            _eventBus.Subscribe<GameDiagnosticsCompletedEvent>(this, _ =>
            {
                _runtimeMode = RuntimeMode.Regular;
            });

            _eventBus.Subscribe<AttractModeEntered>(this, _ =>
            {
                _platformState |= PlatformStateEnum.InAttractMode;
            });

            _eventBus.Subscribe<AttractModeExited>(this, _ =>
            {
                _platformState &= ~PlatformStateEnum.InAttractMode;
            });
        }

        /// <summary>
        ///     Unsubscribes to all events used by objects of this class.
        /// </summary>
        public void CancelEventSubscriptions()
        {
            _eventBus.UnsubscribeAll(this);
        }

        private class DummyEvent_IdValidEvent
        {
        }

        private class DummyEvent_IdInvalidEvent
        {
        }

        private class DummyEvent_IdNullEvent
        {
        }
    }
}
