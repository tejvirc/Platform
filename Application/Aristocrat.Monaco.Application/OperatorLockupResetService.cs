namespace Aristocrat.Monaco.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Contracts;
    using Contracts.Localization;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     This class handles creating a secondary lockup when a system is disabled by other hard lockups.
    ///     Some jurisdictions require additional action/process to enable the system that has been disabled due to one or more
    ///     different reasons.
    ///     For instance, if a system was disabled due to invalid bill-in (multiple times), then we require additional action
    ///     such as door open-close or Jackpot key toggle
    ///     to enable the system once all other reasons for system disable are resolved/removed.
    ///     Part of this requirement is to adhere to compliance and at the same time keep the (lockup removal) behavior same as
    ///     in Linux software so as to maintain operational
    ///     consistency between EGMs running Linux and Monaco software. Refer : VLT-12854
    /// </summary>
    public class OperatorLockupResetService : IService, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisableManager;
        private readonly object _lockObject = new object();

        private bool _operatorResetLockupActive;
        private bool _disposed;

        // For all these lockups, we must generate a secondary lockup,
        // which cannot be cleared until all of these lockups have been cleared.
        private readonly List<Guid> _lockupsRequireOperatorReset = new List<Guid>
        {
            ApplicationConstants.DisplayDisconnectedLockupKey,
            ApplicationConstants.TouchDisplayDisconnectedLockupKey,
            ApplicationConstants.LcdButtonDeckDisconnectedLockupKey,
            ApplicationConstants.HardMeterDisabled,
            ApplicationConstants.CurrencyIsoInvalidDisableKey,
            ApplicationConstants.NoteAcceptorDisconnectedGuid,
            ApplicationConstants.NoteAcceptorDocumentCheckDisableKey,
            ApplicationConstants.PrinterDisconnectedGuid,
            ApplicationConstants.ReadOnlyMediaDisableKey,
            PrinterFaultTypes.TemperatureFault.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.PrintHeadDamaged.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.NvmFault.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.OtherFault.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.PaperJam.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.PaperEmpty.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.PaperNotTopOfForm.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.PrintHeadOpen.GetAttribute<ErrorGuidAttribute>().Id,
            PrinterFaultTypes.ChassisOpen.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.FirmwareFault.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.MechanicalFault.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.OpticalFault.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.ComponentFault.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.NvmFault.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.OtherFault.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.StackerDisconnected.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.StackerFull.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.StackerJammed.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.StackerFault.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.NoteJammed.GetAttribute<ErrorGuidAttribute>().Id,
            NoteAcceptorFaultTypes.CheatDetected.GetAttribute<ErrorGuidAttribute>().Id
        };

        public OperatorLockupResetService()
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().TryGetService<ISystemDisableManager>())
        {
        }

        public OperatorLockupResetService(
            IEventBus bus,
            IPropertiesManager properties,
            ISystemDisableManager disableManager)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _systemDisableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _operatorResetLockupActive = false;
        }

        /// <inheritdoc />
        public string Name => typeof(OperatorLockupResetService).Name;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(OperatorLockupResetService) };

        /// <inheritdoc />
        public void Initialize()
        {
            var secondaryLockupEnabled = _properties.GetValue(ApplicationConstants.OperatorLockupResetEnabled, false);
            if (!secondaryLockupEnabled)
            {
                return;
            }

            // if a lockup already exists, we also create operator lockup
            if (_lockupsRequireOperatorReset.Any(
                enableKey => _systemDisableManager.CurrentDisableKeys.Contains(enableKey)))
            {
                ShowLockup();
            }

            Setup();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if dispose should be called on managed objects</param>
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

        private void Setup()
        {
            _bus.Subscribe<SystemDisableAddedEvent>(this, HandleEvent);
            _bus.Subscribe<DownEvent>(
                this,
                HandleEvent,
                evt => evt.LogicalId == (int)ButtonLogicalId.Button30);
        }

        private void HandleEvent(SystemDisableAddedEvent evt)
        {
            if (!_lockupsRequireOperatorReset.Contains(evt.DisableId))
            {
                return;
            }
            ShowLockup(evt.Priority);
        }

        private void HandleEvent(DownEvent evt)
        {
            lock (_lockObject)
            {
                if (!_operatorResetLockupActive)
                {
                    return;
                }

                if (_lockupsRequireOperatorReset.Any(enableKey => _systemDisableManager.CurrentDisableKeys.Contains(enableKey)))
                {
                    return;
                }


                _systemDisableManager.Enable(ApplicationConstants.OperatorResetRequiredDisableKey);
                _operatorResetLockupActive = false;
            }
        }

        private void ShowLockup(SystemDisablePriority priority = SystemDisablePriority.Normal)
        {
            lock (_lockObject)
            {
                _systemDisableManager.Disable(
                    ApplicationConstants.OperatorResetRequiredDisableKey,
                    priority,
                    () => Localizer.DynamicCulture().GetString(ResourceKeys.OperatorResetRequiredMessage));
                _operatorResetLockupActive = true;
            }
        }
    }
}
