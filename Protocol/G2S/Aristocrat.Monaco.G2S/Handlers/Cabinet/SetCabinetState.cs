namespace Aristocrat.Monaco.G2S.Handlers.Cabinet
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel;

    public class SetCabinetState : ICommandHandler<cabinet, setCabinetState>
    {
        private readonly ICommandBuilder<ICabinetDevice, cabinetStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IPropertiesManager _properties;
        private readonly ISystemDisableManager _systemDisableManager;

        public SetCabinetState(
            IG2SEgm egm,
            ICommandBuilder<ICabinetDevice, cabinetStatus> commandBuilder,
            ISystemDisableManager systemDisableManager,
            IPropertiesManager properties,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _systemDisableManager =
                systemDisableManager ?? throw new ArgumentNullException(nameof(systemDisableManager));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        private static Guid GamePlayDisabledKey => new Guid("{18E7C6CD-3A47-4969-92F8-744C268355B6}");

        public async Task<Error> Verify(ClassCommand<cabinet, setCabinetState> command)
        {
            return await Sanction.OnlyOwner<ICabinetDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<cabinet, setCabinetState> command)
        {
            var device = _egm.GetDevice<ICabinetDevice>(command.IClass.deviceId);

            SetMoneyInState(device, command.Command.enableMoneyIn);

            // NOTE: enableMoneyIn has been deprecated, but it is required by the IGT host.
            //  We're only partially supporting this.
            //  We're going to combine this with the cabinet state, so it can't be independently disabled.
            //  We may need to re-evaluate this if needed.
            SetMoneyOutState(device, command.Command.enable || command.Command.enableMoneyOut);

            SetState(device, command.Command.enable, command.Command.disableText);

            SetGamePlayState(device, command.Command.enableGamePlay, command.Command.disableText);

            var response = command.GenerateResponse<cabinetStatus>();

            await _commandBuilder.Build(device, response.Command);
        }

        private void SetState(ICabinetDevice cabinet, bool enabled, string message)
        {
            if (cabinet.HostEnabled == enabled)
            {
                return;
            }

            cabinet.DisableText = message;
            cabinet.HostEnabled = enabled;
        }

        private void SetGamePlayState(ICabinetDevice cabinet, bool enabled, string message)
        {
            if (enabled == cabinet.GamePlayEnabled)
            {
                return;
            }

            cabinet.GamePlayEnabled = enabled;

            if (enabled)
            {
                _systemDisableManager.Enable(GamePlayDisabledKey);
            }
            else
            {
                _systemDisableManager.Disable(GamePlayDisabledKey, SystemDisablePriority.Normal, () => message);
            }

            var status = new cabinetStatus();
            _commandBuilder.Build(cabinet, status);
            _eventLift.Report(
                cabinet,
                enabled ? EventCode.G2S_CBE102 : EventCode.G2S_CBE101,
                cabinet.DeviceList(status));
        }

        private void SetMoneyInState(ICabinetDevice cabinet, bool enabled)
        {
            var current = _properties.GetValue(AccountingConstants.MoneyInEnabled, true);

            if (current == enabled)
            {
                return;
            }

            _properties.SetProperty(AccountingConstants.MoneyInEnabled, enabled);

            var status = new cabinetStatus();
            _commandBuilder.Build(cabinet, status);
            _eventLift.Report(
                cabinet,
                enabled ? EventCode.G2S_CBE104 : EventCode.G2S_CBE103,
                cabinet.DeviceList(status));
        }

        private void SetMoneyOutState(ICabinetDevice cabinet, bool enabled)
        {
            if (enabled == cabinet.MoneyOutEnabled)
            {
                return;
            }

            cabinet.MoneyOutEnabled = enabled;

            var status = new cabinetStatus();
            _commandBuilder.Build(cabinet, status);
            _eventLift.Report(
                cabinet,
                enabled ? EventCode.G2S_CBE106 : EventCode.G2S_CBE105,
                cabinet.DeviceList(status));
        }
    }
}