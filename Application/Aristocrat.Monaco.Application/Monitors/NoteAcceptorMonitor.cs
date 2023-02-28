namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;
    using Contracts.Localization;
    using Contracts.NoteAcceptorMonitor;
    using Contracts.OperatorMenu;
    using Hardware.Contracts;
    using Hardware.Contracts.Dfu;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;
    using Util;
    using Audio = Hardware.Contracts.Audio;

    /// <summary>
    ///     Definition of the NoteAcceptorMonitor class.
    /// </summary>
    public class NoteAcceptorMonitor : GenericBaseMonitor, INoteAcceptorMonitor, IService
    {
        private const string BlockName = "Aristocrat.Monaco.Application.Monitors.NoteAcceptorMonitor";
        private const string NoteAcceptorZeroCreditsKey = "NoteAcceptorZeroCredits";
        private const string SelfTestFailedKey = "SelfTestFailed";
        private const string StackerDisconnected = "StackerDisconnected";

        private readonly IEventBus _eventBus;
        private readonly INoteAcceptor _noteAcceptor;
        private readonly Audio.IAudio _audioService;
        private readonly IMeterManager _meterManager;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IMessageDisplay _messageDisplay;
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private long _currentCredits;
        private string _delayDisableKey;
        private bool _lockupOnDisconnect;
        private bool _lockupOnStackerFull;
        private bool _softErrorOnStackerFull;
        private string _noteAcceptorErrorSoundFilePath;
        private bool _isOperatorMenuOpen;
        private bool _errorWhileInGamePlay;
        private bool _stopAlarmWhenAuditMenuOpened;

        public NoteAcceptorMonitor(IEventBus eventBus,
            INoteAcceptor noteAcceptor,
            Audio.IAudio audioService,
            IMeterManager meterManager,
            IPersistentStorageManager persistentStorage,
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager,
            IMessageDisplay messageDisplay)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _noteAcceptor = noteAcceptor ?? throw new ArgumentNullException(nameof(noteAcceptor));
            _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
        }

        public NoteAcceptorMonitor()
        {
            _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            _audioService = ServiceManager.GetInstance().GetService<Audio.IAudio>();
            _eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            _meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            _persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            _disableManager = ServiceManager.GetInstance().GetService<ISystemDisableManager>();
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _messageDisplay = ServiceManager.GetInstance().GetService<IMessageDisplay>();
        }

        /// <summary>
        ///     Gets the key used to disable the system when the document check occurs.
        /// </summary>
        private static Guid NoteAcceptorDocumentCheckDisableKey => ApplicationConstants.NoteAcceptorDocumentCheckDisableKey;

        /// <inheritdoc />
        public override string DeviceName => "NoteAcceptor";

        private string BehavioralDelayKey => _currentCredits == 0 ? null : _delayDisableKey;

        /// <inheritdoc />
        public void SetCurrentCredits(long credits)
        {
            if (credits == 0)
            {
                PerformDelayedDisables(_delayDisableKey);
            }

            _currentCredits = credits;
        }

        /// <inheritdoc />
        public string Name => GetType().Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(INoteAcceptorMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            if (_noteAcceptor == null)
            {
                return;
            }

            if (!_persistentStorage.BlockExists(BlockName))
            {
                var block = _persistentStorage.CreateBlock(PersistenceLevel.Transient, BlockName, 1);
                using (var transaction = block.StartTransaction())
                {
                    transaction[DisconnectedKey] = false;
                    transaction[StackerDisconnected] = false;
                    transaction.Commit();
                }
            }

            // In some jurisdictions, hard tilts won't disable the EGM until credits are zero.
            Configure();

            foreach (var value in Enum.GetValues(typeof(NoteAcceptorFaultTypes)).Cast<NoteAcceptorFaultTypes>())
            {
                var lockUpOnError = !value.Equals(NoteAcceptorFaultTypes.StackerFull) || _lockupOnStackerFull;
                ManageErrorEnum(value,
                        _softErrorOnStackerFull ?
                            value.Equals(NoteAcceptorFaultTypes.StackerFull) ?
                                DisplayableMessageClassification.SoftError : DisplayableMessageClassification.HardError
                            : DisplayableMessageClassification.HardError,
                        value.Equals(NoteAcceptorFaultTypes.StackerDisconnected)
                            ? DisplayableMessagePriority.Immediate
                            : DisplayableMessagePriority.Normal,
                        lockUpOnError);
            }

            ManageBinaryCondition(
                DisconnectedKey,
                DisplayableMessageClassification.SoftError,
                DisplayableMessagePriority.Normal,
               ApplicationConstants.NoteAcceptorDisconnectedGuid,
                _lockupOnDisconnect);
            ManageBinaryCondition(
                DisabledKey,
                DisplayableMessageClassification.Diagnostic,
                DisplayableMessagePriority.Normal,
                new Guid("{1814BD96-2F99-4E1C-A71E-B294378239FB}"));
            ManageBinaryCondition(
                SelfTestFailedKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                ApplicationConstants.NoteAcceptorSelfTestFailedGuid,
                true);
            ManageBinaryCondition(
                DfuInprogressKey,
                DisplayableMessageClassification.HardError,
                DisplayableMessagePriority.Immediate,
                new Guid("{43D71747-4A38-43ED-87CD-4BD65D08F6C9}"),
                true);

            _propertiesManager.AddPropertyProvider(this);

            SubscribeToEvents();

            LoadSounds();

            // check device status on boot up
            CheckDeviceStatus();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ConnectedEvent>(this, _ => { Disconnected(false); });
            _eventBus.Subscribe<DisconnectedEvent>(this, _ => { Disconnected(true, BehavioralDelayKey); });
            _eventBus.Subscribe<EnabledEvent>(this, _ => { SetBinary(DisabledKey, false); });
            _eventBus.Subscribe<DisabledEvent>(
                this,
                _ =>
                {
                    SetBinary(DisabledKey, true, BehavioralDelayKey);
                    var device = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
                    if (device is { DisabledByError: true } && _errorWhileInGamePlay)
                    {
                        if (PlayNoteAcceptorErrorSound())
                        {
                            _errorWhileInGamePlay = false;
                        }
                    }
                });
            _eventBus.Subscribe<HardwareFaultClearEvent>(this, Handle);
            _eventBus.Subscribe<HardwareFaultEvent>(this, Handle);
            _eventBus.Subscribe<SelfTestPassedEvent>(
                this,
                _ =>
                {
                    SetBinary(SelfTestFailedKey, false);
                    ClearStackerDisconnected();
                    HandleSelfTestComplete();
                });
            _eventBus.Subscribe<SelfTestFailedEvent>(
                this,
                _ =>
                {
                    SetBinary(SelfTestFailedKey, true);
                    HandleSelfTestComplete();
                });
            _eventBus.Subscribe<InspectionFailedEvent>(this, e => { Disconnected(true); });
            _eventBus.Subscribe<InspectedEvent>(this, e => { Disconnected(false); });
            _eventBus.Subscribe<DfuDownloadStartEvent>(
                this,
                e =>
                {
                    if (e.Device == DeviceType.NoteAcceptor)
                    {
                        SetBinary(DfuInprogressKey, true);
                    }
                });
            _eventBus.Subscribe<DfuErrorEvent>(this, e => { SetBinary(DfuInprogressKey, false); });
            _eventBus.Subscribe<FirmwareInstalledEvent>(this, e => { SetBinary(DfuInprogressKey, false); });
            _eventBus.Subscribe<OperatorMenuEnteredEvent>(this,
                _ =>
                {
                    if (_stopAlarmWhenAuditMenuOpened)
                        _isOperatorMenuOpen = true;
                });
            _eventBus.Subscribe<OperatorMenuExitedEvent>(this, e => _isOperatorMenuOpen = false);
        }

        private void Disconnected(bool disconnected, string behavioralDelayKey = null)
        {
            if (disconnected && !_isOperatorMenuOpen)
            {
                PlayNoteAcceptorErrorSound();
            }

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                var block = _persistentStorage.GetBlock(BlockName);

                if (disconnected && !(bool)block[DisconnectedKey])
                {
                    _meterManager.GetMeter(ApplicationMeters.NoteAcceptorDisconnectCount).Increment(1);
                }

                using (var transaction = block.StartTransaction())
                {
                    transaction[DisconnectedKey] = disconnected;
                    transaction.Commit();
                }

                scope.Complete();
            }

            var disconnectedMessage = new DisplayableMessage(DisconnectedMessageCallback, DisplayableMessageClassification.SoftError, DisplayableMessagePriority.Immediate);

            if (disconnected)
            {
                _messageDisplay.DisplayMessage(disconnectedMessage);
            }
            else
            {
                _messageDisplay.RemoveMessage(disconnectedMessage);
            }

            SetBinary(DisconnectedKey, disconnected, behavioralDelayKey);
        }

        private string DisconnectedMessageCallback()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoteAcceptor_Disconnected);
        }

        private void CheckDeviceStatus()
        {
            if (_noteAcceptor != null && !_noteAcceptor.Enabled)
            {
                if (_noteAcceptor.Faults != NoteAcceptorFaultTypes.None)
                {
                    _eventBus.Publish(new HardwareFaultEvent(_noteAcceptor.Faults));
                }
                else
                {
                    SetBinary(DisabledKey, true);
                }

                if (_noteAcceptor.LastError.Contains(typeof(SelfTestFailedEvent).ToString()))
                {
                    SetBinary(SelfTestFailedKey, true);
                }
            }

            if (_noteAcceptor?.LogicalState == NoteAcceptorLogicalState.Uninitialized)
            {
                Disconnected(true);
            }

            if (_noteAcceptor?.WasStackingOnLastPowerUp ?? false)
            {
                DisableForDocumentCheck();
            }
        }

        private void LoadSounds()
        {
            _noteAcceptorErrorSoundFilePath =
                _propertiesManager?.GetValue(ApplicationConstants.NoteAcceptorErrorSoundKey, string.Empty);
            _audioService.LoadSound(_noteAcceptorErrorSoundFilePath);
        }

        /// <summary>
        ///     Plays the sound defined in the Application Config for PlayNoteAcceptorErrorSound
        /// </summary>
        private bool PlayNoteAcceptorErrorSound()
        {
            if (!string.IsNullOrEmpty(_noteAcceptorErrorSoundFilePath))
            {
                if (_noteAcceptor != null && !_noteAcceptor.ReasonDisabled.HasFlag(DisabledReasons.GamePlay))
                {
                    _audioService.PlaySound(_propertiesManager, _noteAcceptorErrorSoundFilePath);
                    return true;
                }
                else
                {
                    _errorWhileInGamePlay = true;
                }
            }

            return false;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }
        }

        private void Handle(HardwareFaultClearEvent @event)
        {
            var faults = @event.Fault;
            foreach (NoteAcceptorFaultTypes e in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (e != NoteAcceptorFaultTypes.None && faults.HasFlag(e))
                {
                    ClearFault(e);
                }
            }

            if (@event.Fault == NoteAcceptorFaultTypes.StackerDisconnected)
            {
                var block = _persistentStorage.GetBlock(BlockName);
                using (var transaction = block.StartTransaction())
                {
                    transaction[StackerDisconnected] = false;
                    transaction.Commit();
                }
            }
        }

        private void Handle(HardwareFaultEvent @event)
        {
            var faults = @event.Fault;

            if (!_isOperatorMenuOpen)
            {
                PlayNoteAcceptorErrorSound();
            }

            foreach (NoteAcceptorFaultTypes e in Enum.GetValues(typeof(NoteAcceptorFaultTypes)))
            {
                if (e != NoteAcceptorFaultTypes.None && faults.HasFlag(e))
                {
                    AddFault(e, BehavioralDelayKey);
                    if (e != NoteAcceptorFaultTypes.StackerDisconnected
                        && e != NoteAcceptorFaultTypes.CheatDetected)
                    {
                        _meterManager.GetMeter(ApplicationMeters.NoteAcceptorErrorCount).Increment(1);
                    }
                }
            }

            if (@event.Fault == NoteAcceptorFaultTypes.StackerDisconnected)
            {
                if (_meterManager != null)
                {
                    using (var scope = _persistentStorage.ScopedTransaction())
                    {
                        var block = _persistentStorage.GetBlock(BlockName);

                        if (!(bool)block[StackerDisconnected])
                        {
                            if ((bool)_propertiesManager.GetProperty(ApplicationConstants.StackerRemovedBehaviorAutoClearPeriodMetersText, false))
                            {
                                _meterManager.ClearAllPeriodMeters();
                            }

                            if (_meterManager.IsMeterProvided(ApplicationMeters.StackerRemovedCount))
                            {
                                _meterManager.GetMeter(ApplicationMeters.StackerRemovedCount).Increment(1);
                            }
                        }

                        using (var transaction = block.StartTransaction())
                        {
                            transaction[StackerDisconnected] = true;
                            transaction.Commit();
                        }

                        scope.Complete();
                    }
                }
            }
        }

        private void Configure()
        {
            var configuration = ConfigurationUtilities.GetConfiguration(
                ApplicationConstants.JurisdictionConfigurationExtensionPath,
                () => new ApplicationConfiguration
                {
                    NoteAcceptorMonitor =
                        new ApplicationConfigurationNoteAcceptorMonitor
                        {
                            DisableOnError = NoteAcceptorMonitorDisableBehavior.Immediate,
                            LockupOnDisconnect = true,
                            SoftErrorOnStackerFull = false,
                            StopAlarmWhenAuditMenuOpened = true
                        }
                });

            if (configuration.NoteAcceptorMonitor.DisableOnError == NoteAcceptorMonitorDisableBehavior.Queue)
            {
                Logger.Debug("BNA disable behavior set to Queue");
                _delayDisableKey = NoteAcceptorZeroCreditsKey;
            }

            _lockupOnDisconnect = configuration.NoteAcceptorMonitor.LockupOnDisconnect;
            _lockupOnStackerFull = configuration.NoteAcceptorMonitor.LockupOnStackerFull;
            _softErrorOnStackerFull = configuration.NoteAcceptorMonitor.SoftErrorOnStackerFull;
            _stopAlarmWhenAuditMenuOpened = configuration.NoteAcceptorMonitor.StopAlarmWhenAuditMenuOpened;
        }

        private void DisableForDocumentCheck()
        {
            _eventBus.Publish(new NoteAcceptorDocumentCheckOccurredEvent());
            _disableManager.Disable(
                NoteAcceptorDocumentCheckDisableKey,
                SystemDisablePriority.Immediate,
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.NoteAcceptorFaultTypes_DocumentCheck));
        }

        private void ClearStackerDisconnected()
        {
            bool disconnected = false;
            using (var scope = _persistentStorage.ScopedTransaction())
            {
                var block = _persistentStorage.GetBlock(BlockName);
                if (block != null)
                {
                    disconnected = (bool)block[StackerDisconnected];
                    if (disconnected)
                    {
                        using (var transaction = block.StartTransaction())
                        {
                            // if connected, clear stacker disconnected error in transaction
                            transaction[StackerDisconnected] = false;
                            transaction.Commit();
                        }
                    }

                    scope.Complete();
                }
            }

            // clear stacker disconnected lockup
            if (disconnected)
            {
                ClearFault(NoteAcceptorFaultTypes.StackerDisconnected);
            }
        }

        private void HandleSelfTestComplete()
        {
            if (!_disableManager.CurrentImmediateDisableKeys.Contains(NoteAcceptorDocumentCheckDisableKey))
            {
                return;
            }

            if (_noteAcceptor?.WasStackingOnLastPowerUp == false)
            {
                _eventBus.Publish(new NoteAcceptorDocumentCheckClearedEvent());
                _disableManager.Enable(NoteAcceptorDocumentCheckDisableKey);
            }
        }
    }
}