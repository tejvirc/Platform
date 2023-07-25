namespace Aristocrat.Monaco.G2S.Consumers
{
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Aristocrat.Monaco.G2S.Handlers;
    using Aristocrat.Monaco.Kernel.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    ///     Handles the <see cref="ExitRequestedEvent" />
    /// </summary>
    public class ShutdownRequestedConsumer : Consumes<ExitRequestedEvent>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ShutdownRequestedConsumer" /> class with a G2S Engine.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        public ShutdownRequestedConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(ExitRequestedEvent theEvent)
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            var status = new cabinetStatus();
            _commandBuilder.Build(cabinet, status);

             _eventLift.Report(cabinet, EventCode.G2S_CBE324, cabinet.DeviceList(status));
        }
    }
}
