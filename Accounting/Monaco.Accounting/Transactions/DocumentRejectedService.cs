namespace Aristocrat.Monaco.Accounting.Transactions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Util;
    using Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Localization.Properties;

    public class DocumentRejectedService : IService, IDisposable
    {
        private readonly IEventBus _bus;

        private readonly DisplayableMessage _documentRejectedMessage;
        private readonly IMessageDisplay _messageDisplay;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly ExcessiveDocumentRejectLockupType _lockupType;
        private readonly IAudio _audioService;
        private readonly ResetMethodKeyType _lockupResetMethodKeyType;

        private INoteAcceptor _noteAcceptor;
        private int _consecutiveDocumentRejectCount;
        private bool _disposed;
        private string _excessiveDocumentRejectErrorSoundFilePath;

        public DocumentRejectedService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IMessageDisplay>(),
                ServiceManager.GetInstance().TryGetService<INoteAcceptor>(),
                ServiceManager.GetInstance().TryGetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().TryGetService<IAudio>())
        {
        }

        public DocumentRejectedService(
            IEventBus bus,
            IPropertiesManager properties,
            IMessageDisplay messageDisplay,
            INoteAcceptor noteAcceptor,
            ISystemDisableManager disableManager,
            IAudio audio)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _noteAcceptor = noteAcceptor;
            _systemDisableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _audioService = audio ?? throw new ArgumentNullException(nameof(audio));

            // Handle Consecutive Bills/Vouchers Reject
            _lockupType = _properties.GetValue(
                ApplicationConstants.ExcessiveDocumentRejectLockupType,
                ExcessiveDocumentRejectLockupType.Soft);

            _lockupResetMethodKeyType = _properties.GetValue(
                ApplicationConstants.ExcessiveDocumentRejectResetMethodKey,
                ResetMethodKeyType.MainDoor);

            _documentRejectedMessage = new DisplayableMessage(
                () => Localizer.For(CultureFor.Player).GetString(ResourceKeys.ExcessiveDocumentRejectMessage),
                (DisplayableMessageClassification)_lockupType,
                DisplayableMessagePriority.Immediate,
                typeof(ExcessiveDocumentRejectedEvent),
                ApplicationConstants.ExcessiveDocumentRejectGuid);
        }

        public string Name => nameof(DocumentRejectedService);

        public ICollection<Type> ServiceTypes => new[] { typeof(DocumentRejectedService) };

        public void Initialize()
        {
            _bus.Subscribe<ServiceAddedEvent>(this, HandleEvent);

            switch (_lockupResetMethodKeyType)
            {
                case ResetMethodKeyType.MainDoor:
                    _bus.Subscribe<ClosedEvent>(
                        this,
                        evt => ClearDocumentReject(),
                        evt => evt.LogicalId == (int)DoorLogicalId.Main);
                    break;
                case ResetMethodKeyType.JackpotKey:
                    _bus.Subscribe<DownEvent>(
                        this,
                        evt => ClearDocumentReject(),
                        evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
                    break;
            }

            _bus.Subscribe<CurrencyInCompletedEvent>(this, HandleEvent);
            _bus.Subscribe<VoucherRejectedEvent>(this, HandleEvent);
            _bus.Subscribe<VoucherRedeemedEvent>(this, HandleEvent);

            LoadSounds();
            CheckDocumentRejectLockupEnabled();
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
                _bus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleEvent(ServiceAddedEvent evt)
        {
            if (evt.ServiceType == typeof(INoteAcceptor))
            {
                _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            }
        }

        private void HandleEvent(CurrencyInCompletedEvent evt)
        {
            if (evt.Amount == 0)
            {
                HandleDocumentReject();
                return;
            }

            if (_lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                ClearDocumentReject();
                return;
            }

            ResetConsecutiveDocumentRejectCount();
        }

        private void HandleEvent(VoucherRedeemedEvent evt)
        {
            if (_lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                ClearDocumentReject();
                return;
            }

            ResetConsecutiveDocumentRejectCount();
        }

        private void HandleEvent(VoucherRejectedEvent evt)
        {
            HandleDocumentReject();
        }

        private void HandleDocumentReject()
        {
            var consecutiveRejectsBeforeLockup = _properties.GetValue(ApplicationConstants.ExcessiveDocumentRejectCount, -1);

            if (_noteAcceptor == null || consecutiveRejectsBeforeLockup == -1 || _properties.GetValue(
                    ApplicationConstants.NoteAcceptorDiagnosticsKey,
                    false))
            {
                return;
            }

            _consecutiveDocumentRejectCount++;

            if (_consecutiveDocumentRejectCount != consecutiveRejectsBeforeLockup)
            {
                return;
            }

            _bus.Publish(new ExcessiveDocumentRejectedEvent());

            ShowLockup();
            _noteAcceptor.Disable(DisabledReasons.Operator);
        }

        private void ClearDocumentReject()
        {
            if (_noteAcceptor == null)
            {
                return;
            }

            ClearLockup();
            _noteAcceptor.Enable(EnabledReasons.Operator);
            ResetConsecutiveDocumentRejectCount();
        }

        private void CheckDocumentRejectLockupEnabled()
        {
            var lockupEnabled = _properties.GetValue(
                AccountingConstants.ExcessiveDocumentRejectLockupEnabled,
                false);

            if (lockupEnabled)
            {
                ShowLockup();
            }
        }

        private void ShowLockup()
        {
            if (_lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                _messageDisplay.DisplayMessage(_documentRejectedMessage);
                return;
            }

            PlayErrorSound();
            _systemDisableManager.Disable(
                ApplicationConstants.ExcessiveDocumentRejectGuid,
                    SystemDisablePriority.Immediate,
                    () => Localizer.DynamicCulture().GetString(ResourceKeys.ExcessiveDocumentRejectMessage));
            _properties.SetProperty(AccountingConstants.ExcessiveDocumentRejectLockupEnabled, true);
        }

        private void ClearLockup()
        {
            if (_lockupType == ExcessiveDocumentRejectLockupType.Soft)
            {
                _messageDisplay.RemoveMessage(_documentRejectedMessage);
                return;
            }
            _systemDisableManager.Enable(ApplicationConstants.ExcessiveDocumentRejectGuid);
            _properties.SetProperty(AccountingConstants.ExcessiveDocumentRejectLockupEnabled, false);
        }

        private void ResetConsecutiveDocumentRejectCount()
        {
            if (!_systemDisableManager.CurrentImmediateDisableKeys.Contains(ApplicationConstants.ExcessiveDocumentRejectGuid))
            {
                _consecutiveDocumentRejectCount = 0;
            }
        }

        /// <summary>
        /// Load sound if configured for ExcessiveDocumentRejectedSound
        /// </summary>
        private void LoadSounds()
        {
            _excessiveDocumentRejectErrorSoundFilePath = _properties.GetValue(
                ApplicationConstants.ExcessiveDocumentRejectSoundFilePath,
                string.Empty);
            _audioService.LoadSound(_excessiveDocumentRejectErrorSoundFilePath);
        }

        /// <summary>
        /// Plays the sound defined in the Application Config for ExcessiveDocumentRejectedSound.
        /// </summary>
        private void PlayErrorSound()
        {
            _audioService.PlaySound(
                _properties,
                _excessiveDocumentRejectErrorSoundFilePath);
        }
    }
}