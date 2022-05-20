namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Kernel;

    public abstract class ReelBaseConsumer<TEvent> : Consumes<TEvent>
        where TEvent : BaseEvent
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        protected ReelBaseConsumer(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        protected void AddFault(CabinetFaults fault)
        {
            AddFault((int)fault);
        }

        protected void AddFault(int fault)
        {
            var device = _egm.GetDevice<ICabinetDevice>();
            if (device == null)
            {
                return;
            }

            device.AddCondition(device, EgmState.EgmDisabled, fault);

            var status = new cabinetStatus();
            _commandBuilder.Build(device, status);
            _eventLift.Report(device, EventCode.G2S_CBE309, device.DeviceList(status));
        }

        protected void RemoveFault(CabinetFaults fault)
        {
            RemoveFault((int)fault);
        }

        protected void RemoveFault(int fault)
        {
            var device = _egm.GetDevice<ICabinetDevice>();

            device?.RemoveCondition(device, EgmState.EgmDisabled, fault);
        }
    }
}