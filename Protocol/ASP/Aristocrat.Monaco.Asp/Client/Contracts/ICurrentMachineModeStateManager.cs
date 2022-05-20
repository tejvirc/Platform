namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    using System;
    using Application.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel.Contracts.Events;

    /// <summary>
    ///     State management to track Current Machine Mode
    /// </summary>
    public interface ICurrentMachineModeStateManager : IDisposable
    {
        /// <summary>
        ///     Fires when Machine Mode is changed
        /// </summary>
        event EventHandler<MachineMode> MachineModeChanged;
        /// <summary>
        ///     Provides current Machine Mode
        /// </summary>
        /// <returns></returns>
        MachineMode GetCurrentMode();
        /// <summary>
        ///     Handler for InitializationCompletedEvent Event
        /// </summary>
        /// <param name="event"></param>
        void HandleEvent(InitializationCompletedEvent @event);
        /// <summary>
        ///     Handler for PersistentStorageIntegrityCheckFailedEvent Event
        /// </summary>
        /// <param name="event"></param>
        void HandleEvent(PersistentStorageIntegrityCheckFailedEvent @event);
        /// <summary>
        ///     Handler for StorageErrorEvent Event
        /// </summary>
        /// <param name="event"></param>
        void HandleEvent(StorageErrorEvent @event);
        /// <summary>
        ///     Handler for SystemDisabledByOperatorEvent Event
        /// </summary>
        /// <param name="event"></param>
        void HandleEvent(SystemDisabledByOperatorEvent @event);
        /// <summary>
        ///     Handler for SystemEnabledByOperatorEvent Event
        /// </summary>
        /// <param name="event"></param>
        void HandleEvent(SystemEnabledByOperatorEvent @event);
    }
}