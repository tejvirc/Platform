namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.Localization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel.MessageDisplay;
    using Kernel.Contracts.MessageDisplay;
    using Localization.Properties;

    public class BonusActivity : ICommandHandler<bonus, bonusActivity>
    {
        private static readonly Guid DisableId = new Guid("{6F88BA88-0609-4C76-99BC-C18DC65A13CF}");
        private static readonly Guid DisplayId = new Guid("{56DB67BA-AA19-4673-83FE-892C4FCC9085}");

        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _commandBuilder;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IEgmStateManager _stateManager;
        private readonly IMessageDisplay _messageDisplay;

        public BonusActivity(
            IG2SEgm egm,
            ICommandBuilder<IBonusDevice, bonusStatus> commandBuilder,
            IMessageDisplay messageDisplay,
            IEgmStateManager stateManager,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _messageDisplay = messageDisplay ?? throw new ArgumentNullException(nameof(messageDisplay));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public async Task<Error> Verify(ClassCommand<bonus, bonusActivity> command)
        {
            return await Sanction.OnlyOwner<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<bonus, bonusActivity> command)
        {
            var device = _egm.GetDevice<IBonusDevice>(command.IClass.deviceId);

            var current = device.BonusActive;

            device.SetKeepAlive(command.Command.bonusActive, status => HandleStatusChange(device, status));

            var response = command.GenerateResponse<bonusStatus>();

            await _commandBuilder.Build(device, response.Command);

            if (current != command.Command.bonusActive)
            {
                _eventLift.Report(
                    device,
                    command.Command.bonusActive ? EventCode.G2S_BNE111 : EventCode.G2S_BNE110,
                    device.DeviceList(response.Command));
            }
        }

        private void HandleStatusChange(IBonusDevice device, bool status)
        {
            if (device.RequiredForPlay)
            {
                if (!status)
                {
                    _stateManager.Disable(
                        DisableId,
                        device,
                        EgmState.EgmDisabled,
                        false,
                        () => device.NoHostText ?? Localizer.GetString(ResourceKeys.NoBonusHost, CultureProviderType.Player));
                }
                else
                {
                    _stateManager.Enable(DisableId, device, EgmState.EgmDisabled);
                }
            }
            else
            {
                if (!status)
                {
                    _messageDisplay.DisplayMessage(GetDisplayableMessage(device));
                }
                else
                {
                    _messageDisplay.RemoveMessage(DisplayId);
                }
            }



            var deviceStatus = new bonusStatus();

            _commandBuilder.Build(device, deviceStatus);

            _eventLift.Report(
                device,
                status ? EventCode.G2S_BNE109 : EventCode.G2S_BNE108,
                device.DeviceList(deviceStatus));
        }

        private static IDisplayableMessage GetDisplayableMessage(IBonusDevice device)
        {
            return new DisplayableMessage(
                () => device?.NoHostText ?? Localizer.GetString(ResourceKeys.NoBonusHost, CultureProviderType.Player),
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Immediate,
                DisplayId);
        }
    }
}