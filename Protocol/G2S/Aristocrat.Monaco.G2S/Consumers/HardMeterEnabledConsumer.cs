﻿namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.HardMeter;

    /// <summary>
    ///     Handles the Hardware.Contracts.HardMeter.EnabledEvent, which sets the cabinet's service hard meter status.
    /// </summary>
    public class HardMeterEnabledConsumer : Consumes<EnabledEvent>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardMeterEnabledConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="commandBuilder">An instance of ICommandBuilder&lt;ICabinet, cabinetStatus&gt;.</param>
        /// <param name="eventLift">The G2S event lift.</param>
        public HardMeterEnabledConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        /// <inheritdoc />
        public override void Consume(EnabledEvent theEvent)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            device.RemoveCondition(device, EgmState.EgmDisabled, (int)CabinetFaults.HardMeterDisabled);

            var status = new cabinetStatus();

            _commandBuilder.Build(device, status);

            _eventLift.Report(device, EventCode.G2S_CBE327, device.DeviceList(status), theEvent);
        }
    }
}