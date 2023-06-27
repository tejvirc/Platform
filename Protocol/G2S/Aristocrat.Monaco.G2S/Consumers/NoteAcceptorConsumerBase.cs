namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Kernel;

    /// <summary>
    ///     Base class for stacker related events
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    public abstract class NoteAcceptorConsumerBase<T> : Consumes<T>
        where T : BaseEvent
    {
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly string _eventCode;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorConsumerBase{T}" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        /// <param name="eventCode">The G2S Event code</param>
        protected NoteAcceptorConsumerBase(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IEventLift eventLift,
            string eventCode)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _eventCode = eventCode;
        }

        /// <inheritdoc />
        public override void Consume(T theEvent)
        {
            var noteAcceptor = _egm.GetDevice<INoteAcceptorDevice>();
            if (noteAcceptor == null)
            {
                return;
            }

            if (!ReportEvent(theEvent))
            {
                return;
            }

            var status = new noteAcceptorStatus();
            _commandBuilder.Build(noteAcceptor, status);

            var log = GetLog();

            _eventLift.Report(
                noteAcceptor,
                GetEventCode(theEvent),
                noteAcceptor.DeviceList(status),
                log?.transactionId ?? 0,
                log != null ? noteAcceptor.TransactionList(log) : null,
                GetMeters(),
                theEvent);
        }

        /// <summary>
        ///     Used to get event code
        /// </summary>
        /// <param name="theEvent">The event</param>
        /// <returns>return the event code.</returns>
        protected virtual string GetEventCode(T theEvent)
        {
            return _eventCode;
        }

        /// <summary>
        ///     Used to prevent the event report
        /// </summary>
        /// <param name="theEvent">The event</param>
        /// <returns>return true to report the event otherwise false</returns>
        protected virtual bool ReportEvent(T theEvent)
        {
            return true;
        }

        /// <summary>
        ///     Used to gather meters related to the event
        /// </summary>
        /// <returns>An optional meter list</returns>
        protected virtual meterList GetMeters()
        {
            return null;
        }

        /// <summary>
        ///     Used to get the transaction for event reports requiring logs
        /// </summary>
        /// <returns>The transaction associated to the event</returns>
        protected virtual notesAcceptedLog GetLog()
        {
            return null;
        }
    }
}