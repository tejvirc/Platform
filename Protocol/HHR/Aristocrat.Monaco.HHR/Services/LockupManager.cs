namespace Aristocrat.Monaco.Hhr.Services
{
    using System;
    using System.Text;
    using Application.Contracts.Localization;
    using Client.Messages;
    using Events;
    using Kernel;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;

    /// <summary>
    ///     Manages when the platform should be enabled and disabled.
    /// </summary>
    public class LockupManager : IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisableManager;
        private bool _disposed;

        public LockupManager(
            IEventBus eventBus,
            ISystemDisableManager systemDisableManager)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));

            _eventBus.Subscribe<CentralServerOnline>(
                this,
                _ => systemDisableManager.Enable(HhrConstants.CentralServerOffline));

            _eventBus.Subscribe<CentralServerOffline>(
                this,
                _ => systemDisableManager.Disable(
                    HhrConstants.CentralServerOffline,
                    SystemDisablePriority.Immediate,
                    ResourceKeys.HHRCentralServerOffline,
                    CultureProviderType.Player,
                    true,
                    () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorInfoHHRCentralServerOffline)));

            _eventBus.Subscribe<ProtocolInitializationFailed>(this, OnProtocolInitializationFailed);
            _eventBus.Subscribe<ProtocolInitializationInProgress>(this, OnProtocolInitializationInProgress);
            _eventBus.Subscribe<ProtocolInitializationComplete>(this, OnProtocolInitializationComplete);

            _eventBus.Subscribe<ProgressiveInitializationFailed>(this, Handle);
            _eventBus.Subscribe<PrizeCalculationErrorEvent>(this, Handle);
            _eventBus.Subscribe<GameConfigurationNotSupportedEvent>(this, Handle);

            _eventBus.Subscribe<GameSelectionMismatchEvent>(this, Handle);
            _eventBus.Subscribe<GameSelectionVerificationCompletedEvent>(
                this,
                _ => systemDisableManager.Enable(HhrConstants.GameSelectionMismatchKey));
            _eventBus.Subscribe<GamePlayRequestFailedEvent>(this, Handle);

            _eventBus.Subscribe<TransactionPendingEvent>(this, Handle);
            _eventBus.Subscribe<PendingRequestRemovedEvent>(this, Handle);
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
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void OnProtocolInitializationComplete(ProtocolInitializationComplete obj)
        {
            _systemDisableManager.Enable(HhrConstants.ProtocolInitializationFailed);
            _systemDisableManager.Enable(HhrConstants.ProtocolInitializationInProgress);
        }

        private void Handle(PendingRequestRemovedEvent obj)
        {
            _systemDisableManager.Enable(HhrConstants.TransactionPendingKey);
        }

        private void Handle(TransactionPendingEvent obj)
        {
            _systemDisableManager.Disable(
                HhrConstants.TransactionPendingKey,
                SystemDisablePriority.Normal,
                ResourceKeys.TransactionRequestPendingText,
                CultureProviderType.Player,
                true,
                () => Localizer.GetString(ResourceKeys.TransactionRequestPendingHelpText, CultureProviderType.Operator));
        }

        private void OnProtocolInitializationInProgress(ProtocolInitializationInProgress obj)
        {
            _systemDisableManager.Disable(
                HhrConstants.ProtocolInitializationInProgress,
                SystemDisablePriority.Immediate,
                ResourceKeys.HHRProtocolInitializationInProgress,
                CultureProviderType.Player,
                true,
                () => Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.ErrorInfoHHRProtocolInitializationInProgress));
        }

        private void OnProtocolInitializationFailed(ProtocolInitializationFailed obj)
        {
            _systemDisableManager.Disable(
                HhrConstants.ProtocolInitializationFailed,
                SystemDisablePriority.Immediate,
                ResourceKeys.HHRProtocolInitializationFailed,
                CultureProviderType.Player,
                true,
                () => Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.ErrorInfoHHRProtocolInitializationFailed));

            _systemDisableManager.Enable(HhrConstants.ProtocolInitializationInProgress);
        }

        private void Handle(ProgressiveInitializationFailed evt)
        {
            _systemDisableManager.Disable(
                HhrConstants.ProgressivesInitializationFailedKey,
                SystemDisablePriority.Normal,
                ResourceKeys.ProgressiveInitializationFailedMsg,
                CultureProviderType.Player);
        }

        private void Handle(PrizeCalculationErrorEvent evt)
        {
            _systemDisableManager.Disable(
                HhrConstants.PrizeCalculationErrorKey,
                SystemDisablePriority.Immediate,
                ResourceKeys.PrizeCalculationError,
                CultureProviderType.Player,
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClearPrizeCalculationError));
        }

        private void Handle(GameConfigurationNotSupportedEvent evt)
        {
            _systemDisableManager.Disable(
                HhrConstants.GameConfigurationNotSupportedKey,
                SystemDisablePriority.Immediate,
                ResourceKeys.GameConfigurationNotSupported,
                CultureProviderType.Operator,
                true,
                () => Localizer.For(CultureFor.Operator)
                    .GetString(ResourceKeys.ErrorInfoGameConfigurationNotSupported),
                null,
                new object[] { evt.GameId });
        }

        private void Handle(GameSelectionMismatchEvent evt)
        {
            _systemDisableManager.Disable(
                HhrConstants.GameSelectionMismatchKey,
                SystemDisablePriority.Normal,
                ResourceKeys.HhrGameSelectionMismatch,
                CultureProviderType.Player,
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HhrGameSelectionMismatchHelpMessage));
        }

        private void Handle(GamePlayRequestFailedEvent evt)
        {
            _systemDisableManager.Disable(
                HhrConstants.GamePlayRequestFailedKey,
                SystemDisablePriority.Normal,
                ResourceKeys.HhrGamePlayRequestFailed,
                CultureProviderType.Player,
                true,
                () => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HhrGamePlayRequestFailedHelpMessage));
        }
    }
}