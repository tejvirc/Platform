namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;

    public class SetBonusLockout : ICommandHandler<bonus, setBonusLockOut>
    {
        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEgmStateManager _egmStateManager;
        private readonly IEventLift _eventLift;

        public SetBonusLockout(
            IG2SEgm egm,
            ICommandBuilder<IBonusDevice, bonusStatus> commandBuilder,
            IEgmStateManager egmStateManager,
            IEventLift eventLift)
        {
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _egmStateManager = egmStateManager ?? throw new ArgumentNullException(nameof(egmStateManager));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<bonus, setBonusLockOut> command)
        {
            return await Sanction.OnlyOwner<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<bonus, setBonusLockOut> command)
        {
            var device = _egm.GetDevice<IBonusDevice>(command.IClass.deviceId);

            device.NotifyActive();

            if (command.Command.lockOut)
            {
                _egmStateManager.Lock(
                    device,
                    EgmState.HostLocked,
                    () => command.Command.lockText,
                    TimeSpan.FromMilliseconds(command.Command.lockTimeOut),
                    () =>
                    {
                        device.HostLocked = false;
                        GenerateEvent(device);
                    });
            }
            else
            {
                _egmStateManager.Enable(device, EgmState.HostLocked);
            }

            device.HostLocked = command.Command.lockOut;

            GenerateEvent(device);

            var response = command.GenerateResponse<bonusStatus>();

            await _commandBuilder.Build(device, response.Command);
        }

        private void GenerateEvent(IBonusDevice device)
        {
            var status = new bonusStatus();

            _commandBuilder.Build(device, status);

            _eventLift.Report(
                device,
                device.HostLocked ? EventCode.G2S_BNE007 : EventCode.G2S_BNE008,
                device.DeviceList(status));
        }
    }
}