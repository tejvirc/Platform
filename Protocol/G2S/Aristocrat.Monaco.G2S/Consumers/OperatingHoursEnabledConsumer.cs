namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Application.Contracts.Operations;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;

    /// <summary>
    ///     Handled the <see cref="OperatingHoursEnabledEvent" /> event.
    /// </summary>
    public class OperatingHoursEnabledConsumer : Consumes<OperatingHoursEnabledEvent>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatingHoursEnabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> instance.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance.</param>
        public OperatingHoursEnabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(OperatingHoursEnabledEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            device.RemoveCondition(device, EgmState.EgmDisabled);

            var status = new cabinetStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(device, EventCode.GTK_CBE004, device.DeviceList(status));
        }
    }
}