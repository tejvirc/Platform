namespace Aristocrat.Monaco.Hardware.Usb.ReelController.Relm
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.Reel.ControlData;
    using Contracts.Reel.Events;
    using Contracts.Reel.ImplementationCapabilities;

    internal class RelmSynchronization : ISynchronizationImplementation
    {
        private readonly IRelmCommunicator _communicator;

        public RelmSynchronization(IRelmCommunicator communicator)
        {
            _communicator = communicator;
        }

#pragma warning disable 67
        public event EventHandler<ReelSynchronizationEventArgs> SynchronizationStarted;

        public event EventHandler<ReelSynchronizationEventArgs> SynchronizationCompleted;
#pragma warning restore 67

        public Task<bool> Synchronize(ReelSynchronizationData data, CancellationToken token)
        {
            return _communicator.Synchronize(data, token);
        }
    }
}
