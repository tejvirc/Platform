namespace Aristocrat.Monaco.G2S.Handlers.Events
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Handles the v21.setEventHandlerState G2S message
    /// </summary>
    public class SetEventHandlerState : ICommandHandler<eventHandler, setEventHandlerState>
    {
        private readonly ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> _command;
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SetEventHandlerState" /> class.
        ///     Creates a new instance of the SetEventHandlerState handler
        /// </summary>
        public SetEventHandlerState(
            IG2SEgm egm,
            IEgmStateManager egmStateManager,
            ICommandBuilder<IEventHandlerDevice, eventHandlerStatus> command)
        {
            if (egmStateManager == null)
            {
                throw new ArgumentNullException(nameof(egmStateManager));
            }

            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _command = command ?? throw new ArgumentNullException(nameof(command));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<eventHandler, setEventHandlerState> command)
        {
            return await Sanction.OnlyOwner<IEventHandlerDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<eventHandler, setEventHandlerState> command)
        {
            if (command.Class.sessionType == t_sessionTypes.G2S_request)
            {
                var device = _egm.GetDevice<IEventHandlerDevice>(command.IClass.deviceId);
                if (device == null)
                {
                    return;
                }

                if (device.HostEnabled != command.Command.enable)
                {
                    device.DisableText = command.Command.disableText;
                    device.HostEnabled = command.Command.enable;
                }

                device.DeviceStateChanged();

                var response = command.GenerateResponse<eventHandlerStatus>();

                await _command.Build(device, response.Command);
            }
        }
    }
}