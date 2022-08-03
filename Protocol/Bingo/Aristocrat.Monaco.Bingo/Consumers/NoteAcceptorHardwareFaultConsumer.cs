﻿namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultEvent" /> event for note acceptors.
    /// </summary>
    public class NoteAcceptorHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;
        private readonly IReportTransactionQueueService _bingoTransactionReportHandler;
        private readonly IMeterManager _meterManager;

        private static readonly IReadOnlyDictionary<NoteAcceptorFaultTypes, ReportableEvent> BingoErrorMapping =
            new Dictionary<NoteAcceptorFaultTypes, ReportableEvent>
            {
                { NoteAcceptorFaultTypes.FirmwareFault, ReportableEvent.BillAcceptorSoftwareError },
                { NoteAcceptorFaultTypes.OpticalFault, ReportableEvent.BillAcceptorHardwareFailure },
                { NoteAcceptorFaultTypes.ComponentFault, ReportableEvent.BillAcceptorHardwareFailure },
                { NoteAcceptorFaultTypes.NvmFault, ReportableEvent.BillAcceptorHardwareFailure },
                { NoteAcceptorFaultTypes.StackerFull, ReportableEvent.BillAcceptorStackerIsFull },
                { NoteAcceptorFaultTypes.StackerFault, ReportableEvent.BillAcceptorHardwareFailure },
                { NoteAcceptorFaultTypes.CheatDetected, ReportableEvent.BillAcceptorCheatDetected },
                { NoteAcceptorFaultTypes.OtherFault, ReportableEvent.BillAcceptorError },
                { NoteAcceptorFaultTypes.MechanicalFault, ReportableEvent.BillAcceptorError },
                { NoteAcceptorFaultTypes.StackerDisconnected, ReportableEvent.StackerRemoved },
                { NoteAcceptorFaultTypes.StackerJammed, ReportableEvent.BillAcceptorStackerJammed },
                { NoteAcceptorFaultTypes.NoteJammed, ReportableEvent.BillAcceptorStackerJammed }
            };

        public NoteAcceptorHardwareFaultConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            IReportTransactionQueueService bingoTransactionReportHandler,
            IMeterManager meterManager)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _bingoTransactionReportHandler =
                bingoTransactionReportHandler ?? throw new ArgumentNullException(nameof(bingoTransactionReportHandler));
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
        }

        public override void Consume(HardwareFaultEvent theEvent)
        {
            if (theEvent.Fault == NoteAcceptorFaultTypes.None)
            {
                return;
            }

            var fault = BingoErrorMapping.TryGetValue(theEvent.Fault, out var flt)
                ? flt
                : ReportableEvent.BillAcceptorHardwareFailure;

            _bingoServerEventReportingService.AddNewEventToQueue(fault);

            if (theEvent.Fault == NoteAcceptorFaultTypes.StackerDisconnected)
            {
                _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CashDrop);
                var totalInMeter = _meterManager.GetMeter(ApplicationMeters.TotalIn);
                _bingoTransactionReportHandler.AddNewTransactionToQueue(TransactionType.Drop, totalInMeter.Period.MillicentsToCents());
            }
        }
    }
}