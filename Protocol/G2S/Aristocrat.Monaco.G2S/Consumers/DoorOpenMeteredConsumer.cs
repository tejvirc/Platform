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

    public class DoorOpenMeteredConsumer : Consumes<DoorOpenMeteredEvent>
    {
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _cabinetStatusBuilder;

        private readonly Dictionary<DoorLogicalId,
            Tuple<Func<bool, string>, Func<bool, string>, Action<DoorOpenMeteredEvent>>> _doorMap;

        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IMetersSubscriptionManager _metersSubscriptionManager;
        private readonly IMeterAggregator<INoteAcceptorDevice> _noteAcceptorMeters;
        private readonly ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> _noteAcceptorStatusBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DoorOpenConsumer" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm</param>
        /// <param name="cabinetStatusBuilder">An <see cref="ICommandBuilder{ICabinetDevice,cabinetStatus}" /> instance</param>
        /// <param name="noteAcceptorStatusBuilder">
        ///     An <see cref="ICommandBuilder{INoteAcceptorDevice,noteAcceptorStatus}" /> instance
        /// </param>
        /// <param name="cabinetMeters">An <see cref="IMeterAggregator{ICabinetDevice}" /> instance</param>
        /// <param name="noteAcceptorMeters">An <see cref="IMeterAggregator{INoteAcceptorDevice}" /> instance</param>
        /// <param name="eventLift">The G2S event lift.</param>
        /// <param name="metersSubscriptionManager">Meters subscription manager.</param>
        public DoorOpenMeteredConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> cabinetStatusBuilder,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> noteAcceptorStatusBuilder,
            IMeterAggregator<ICabinetDevice> cabinetMeters,
            IMeterAggregator<INoteAcceptorDevice> noteAcceptorMeters,
            IEventLift eventLift,
            IMetersSubscriptionManager metersSubscriptionManager)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _cabinetStatusBuilder =
                cabinetStatusBuilder ?? throw new ArgumentNullException(nameof(cabinetStatusBuilder));
            _noteAcceptorStatusBuilder = noteAcceptorStatusBuilder ??
                                         throw new ArgumentNullException(nameof(noteAcceptorStatusBuilder));
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));
            _noteAcceptorMeters = noteAcceptorMeters ?? throw new ArgumentNullException(nameof(noteAcceptorMeters));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _metersSubscriptionManager = metersSubscriptionManager ??
                                         throw new ArgumentNullException(nameof(metersSubscriptionManager));

            _doorMap =
                new Dictionary<DoorLogicalId,
                    Tuple<Func<bool, string>, Func<bool, string>, Action<DoorOpenMeteredEvent>>>
                {
                    {
                        DoorLogicalId.Logic,
                        Tuple.Create<Func<bool, string>, Func<bool, string>, Action<DoorOpenMeteredEvent>>(
                            poweredOff => poweredOff ? EventCode.G2S_CBE317 : EventCode.G2S_CBE303,
                            poweredOff =>
                                poweredOff
                                    ? CabinetMeterName.PowerOffLogicDoorOpenCount
                                    : CabinetMeterName.LogicDoorOpenCount,
                            HandleCabinetDoorEvent)
                    },
                    {
                        DoorLogicalId.TopBox,
                        Tuple.Create<Func<bool, string>, Func<bool, string>, Action<DoorOpenMeteredEvent>>(
                            poweredOff => poweredOff ? EventCode.G2S_CBE318 : EventCode.G2S_CBE305,
                            poweredOff =>
                                poweredOff
                                    ? CabinetMeterName.PowerOffAuxiliaryDoorOpenCount
                                    : CabinetMeterName.AuxiliaryDoorOpenCount,
                            HandleCabinetDoorEvent)
                    },
                    {
                        DoorLogicalId.Main,
                        Tuple.Create<Func<bool, string>, Func<bool, string>, Action<DoorOpenMeteredEvent>>(
                            poweredOff => poweredOff ? EventCode.G2S_CBE319 : EventCode.G2S_CBE307,
                            poweredOff =>
                                poweredOff
                                    ? CabinetMeterName.PowerOffCabinetDoorOpenCount
                                    : CabinetMeterName.CabinetDoorOpenCount,
                            HandleCabinetDoorEvent)
                    },
                    {
                        DoorLogicalId.CashBox,
                        Tuple.Create<Func<bool, string>, Func<bool, string>, Action<DoorOpenMeteredEvent>>(
                            poweredOff => poweredOff ? EventCode.G2S_NAE118 : EventCode.G2S_NAE112,
                            poweredOff =>
                                poweredOff
                                    ? CurrencyMeterName.PowerOffDropDoorOpenCount
                                    : CurrencyMeterName.DropDoorOpenCount,
                            HandleNoteAcceptorDoorEvent)
                    }
                };
        }

        /// <inheritdoc />
        public override void Consume(DoorOpenMeteredEvent theEvent)
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

            var status = new cabinetStatus();

            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            _cabinetStatusBuilder.Build(device, status);

            var meters = _cabinetMeters.GetMeters(device, doorInfo.Item2(theEvent.WhilePoweredDown));

            _eventLift.Report(
                device,
                doorInfo.Item1(theEvent.WhilePoweredDown),
                device.DeviceList(status),
                new meterList { meterInfo = meters.ToArray() },
                theEvent);

            if (!theEvent.WhilePoweredDown && (DoorLogicalId)theEvent.LogicalId == DoorLogicalId.Main)
            {
                _metersSubscriptionManager.SendEndOfDayMeters(true);
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

            var meters = _noteAcceptorMeters.GetMeters(
                device,
                doorInfo.Item2(theEvent.WhilePoweredDown));

            _eventLift.Report(
                device,
                doorInfo.Item1(theEvent.WhilePoweredDown),
                device.DeviceList(status),
                new meterList { meterInfo = meters.ToArray() },
                theEvent);
        }
    }
}
