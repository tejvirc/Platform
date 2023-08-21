namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Printer;

    /// <summary>
    ///     Base class for printer related events
    /// </summary>
    /// <typeparam name="T">The event type</typeparam>
    public abstract class PrinterConsumerBase<T> : Consumes<T>
        where T : PrinterBaseEvent
    {
        private readonly ICommandBuilder<IPrinterDevice, printerStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly string _eventCode;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrinterConsumerBase{T}" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> implementation</param>
        /// <param name="eventLift">A G2S event lift.</param>
        /// <param name="eventCode">The G2S Event code</param>
        protected PrinterConsumerBase(
            IG2SEgm egm,
            ICommandBuilder<IPrinterDevice, printerStatus> commandBuilder,
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
            var printer = _egm.GetDevice<IPrinterDevice>(theEvent.PrinterId);
            if (printer == null)
            {
                return;
            }

            var status = new printerStatus();
            _commandBuilder.Build(printer, status);

            _eventLift.Report(
                printer,
                _eventCode,
                printer.DeviceList(status),
                GetMeters(),
                theEvent);
        }

        /// <summary>
        ///     Used to gather meters related to the event
        /// </summary>
        /// <returns>An optional meter list</returns>
        protected virtual meterList GetMeters()
        {
            return null;
        }
    }
}