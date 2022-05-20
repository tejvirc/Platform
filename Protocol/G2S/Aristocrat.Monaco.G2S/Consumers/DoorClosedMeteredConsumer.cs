namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Hardware.Contracts.Door;
    using Meters;

    /// <summary>
    ///     Handles the Hardware.Contracts.Door.DoorClosedMeteredEvent, which sets emits an event.
    /// </summary>
    public class DoorClosedMeteredConsumer : Consumes<DoorClosedMeteredEvent>
    {
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _cabinetStatusBuilder;
        private readonly Dictionary<DoorLogicalId, Tuple<string, string, Action<DoorClosedMeteredEvent>>> _doorMap;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _noteAcceptorStatusBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorClosedMeteredConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="cabinetStatusBuilder">An <see cref="ICommandBuilder{ICabinetDevice,cabinetStatus}" /> instance</param>
        /// <param name="noteAcceptorStatusBuilder">
        ///     An <see cref="ICommandBuilder{INoteAcceptorDevice,noteAcceptorStatus}" /> instance
        /// </param>
        /// <param name="cabinetMeters">An <see cref="IMeterAggregator{ICabinetDevice}" /> instance</param>
        /// <param name="eventLift">The G2S event lift.</param>
        public DoorClosedMeteredConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> cabinetStatusBuilder,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> noteAcceptorStatusBuilder,
            IMeterAggregator<ICabinetDevice> cabinetMeters,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _cabinetStatusBuilder =
                cabinetStatusBuilder ?? throw new ArgumentNullException(nameof(cabinetStatusBuilder));
            _noteAcceptorStatusBuilder = noteAcceptorStatusBuilder ??
                                         throw new ArgumentNullException(nameof(noteAcceptorStatusBuilder));
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));

            _doorMap = new Dictionary<DoorLogicalId, Tuple<string, string, Action<DoorClosedMeteredEvent>>>
            {
                {
                    DoorLogicalId.Logic,
                    Tuple.Create<string, string, Action<DoorClosedMeteredEvent>>(
                        EventCode.G2S_CBE304,
                        string.Empty,
                        HandleCabinetDoorEvent)
                },
                {
                    DoorLogicalId.TopBox,
                    Tuple.Create<string, string, Action<DoorClosedMeteredEvent>>(
                        EventCode.G2S_CBE306,
                        string.Empty,
                        HandleCabinetDoorEvent)
                },
                {
                    DoorLogicalId.Main,
                    Tuple.Create<string, string, Action<DoorClosedMeteredEvent>>(
                        EventCode.G2S_CBE308,
                        CabinetMeterName.GamesSinceDoorClosedCount,
                        HandleCabinetDoorEvent)
                },
                {
                    DoorLogicalId.CashBox,
                    Tuple.Create<string, string, Action<DoorClosedMeteredEvent>>(
                        EventCode.G2S_NAE113,
                        string.Empty,
                        HandleNoteAcceptorDoorEvent)
                }
            };
        }

        /// <inheritdoc />
        public override void Consume(DoorClosedMeteredEvent theEvent)
        {
            if (!_doorMap.TryGetValue((DoorLogicalId)theEvent.LogicalId, out var doorInfo))
            {
                return;
            }

            doorInfo.Item3(theEvent);
        }

        private void HandleCabinetDoorEvent(DoorBaseEvent theEvent)
        {
            if (!_doorMap.TryGetValue((DoorLogicalId)theEvent.LogicalId, out var doorInfo))
            {
                return;
            }

            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            var status = new cabinetStatus();

            _cabinetStatusBuilder.Build(device, status);

            var meters = _cabinetMeters.GetMeters(device, doorInfo.Item2);

            _eventLift.Report(
                device,
                doorInfo.Item1,
                device.DeviceList(status),
                new meterList { meterInfo = meters.ToArray() });

            if (!theEvent.WhilePoweredDown)
            {
                device.RemoveCondition(device, EgmState.EgmDisabled, theEvent.LogicalId);
            }
        }

        private void HandleNoteAcceptorDoorEvent(DoorBaseEvent theEvent)
        {
            if (!_doorMap.TryGetValue((DoorLogicalId)theEvent.LogicalId, out var doorInfo))
            {
                return;
            }

            var status = new noteAcceptorStatus();

            var device = _egm.GetDevice<INoteAcceptorDevice>();
            if (device == null)
            {
                return;
            }

            _noteAcceptorStatusBuilder.Build(device, status);

            _eventLift.Report(
                device,
                doorInfo.Item1,
                device.DeviceList(status));

            if (!theEvent.WhilePoweredDown)
            {
                var cabinet = _egm.GetDevice<ICabinetDevice>();
                cabinet?.RemoveCondition(cabinet, EgmState.EgmDisabled, theEvent.LogicalId);
            }
        }
    }
}
