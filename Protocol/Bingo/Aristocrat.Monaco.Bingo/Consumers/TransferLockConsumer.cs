namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Kernel;
    using Sas.Contracts.Events;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="TransferLockEvent" /> event.
    ///     This event is sent by the SAS AftLockHandler when the
    ///     lock status changes
    /// </summary>
    public class TransferLockConsumer : Consumes<TransferLockEvent>
    {
        private readonly IReportEventQueueService _bingoEventQueue;

        public TransferLockConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IReportEventQueueService bingoEventQueue)
            : base(eventBus, sharedConsumer)
        {
            _bingoEventQueue = bingoEventQueue ?? throw new ArgumentNullException(nameof(bingoEventQueue));
        }

        public override void Consume(TransferLockEvent theEvent)
        {
            if (theEvent.TransferConditions is not AftTransferConditions.BonusAwardToGamingMachineOk)
            {
                _bingoEventQueue.AddNewEventToQueue(
                    theEvent.Locked ? ReportableEvent.TransferLock : ReportableEvent.TransferUnlock);
            }
        }
    }
}
