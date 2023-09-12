namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;

    internal sealed class RelmSynchronization : ISynchronizationImplementation
    {
        private readonly IRelmCommunicator _communicator;

        public RelmSynchronization(IRelmCommunicator communicator)
        {
            _communicator = communicator;
            _communicator.SynchronizationCompleted += HandleSynchronizationCompleted;
            _communicator.SynchronizationStarted += HandleSynchronizationStarted;
        }

        public event EventHandler<ReelSynchronizationEventArgs> SynchronizationStarted;

        public event EventHandler<ReelSynchronizationEventArgs> SynchronizationCompleted;

        public void Dispose()
        {
            _communicator.SynchronizationCompleted -= HandleSynchronizationCompleted;
            _communicator.SynchronizationStarted -= HandleSynchronizationStarted;
        }

        public Task<bool> Synchronize(ReelSynchronizationData syncData, CancellationToken token = default)
        {
            return _communicator.Synchronize(syncData, token);
        }

        private void HandleSynchronizationCompleted(object sender, ReelSynchronizationEventArgs args)
        {
            SynchronizationCompleted?.Invoke(sender, args);
        }

        private void HandleSynchronizationStarted(object sender, ReelSynchronizationEventArgs args)
        {
            SynchronizationStarted?.Invoke(sender, args);
        }
    }
}
