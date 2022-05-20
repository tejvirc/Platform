namespace Aristocrat.Monaco.G2S.Handlers.CommConfig
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Services;

    /// <summary>
    ///     Implementation of 'enterCommConfigMode' command of 'CommConfig' G2S class.
    /// </summary>
    public class EnterCommConfigMode : ICommandHandler<commConfig, enterCommConfigMode>
    {
        private readonly ICommandBuilder<ICommConfigDevice, commConfigModeStatus> _commandBuilder;
        private readonly IDisableConditionSaga _configurationMode;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnterCommConfigMode" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm.</param>
        /// <param name="configurationMode">Am <see cref="IDisableConditionSaga" /> instance.</param>
        /// <param name="commandBuilder">Command builder.</param>
        /// <param name="eventLift">An <see cref="IEventLift" /> instance</param>
        public EnterCommConfigMode(
            IG2SEgm egm,
            IDisableConditionSaga configurationMode,
            ICommandBuilder<ICommConfigDevice, commConfigModeStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _configurationMode = configurationMode ?? throw new ArgumentNullException(nameof(configurationMode));
        }

        /// <summary>
        ///     Invoked before the call to Handle to verify the command should be executed. The Verify method should throw an
        ///     exception indicating the error to be returned
        /// </summary>
        /// <param name="command">The command to be verified</param>
        /// <returns>Returns an Error if the verification failed, or null if successful.</returns>
        public async Task<Error> Verify(ClassCommand<commConfig, enterCommConfigMode> command)
        {
            var error = await Sanction.OnlyOwner<ICommConfigDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            if (command.Command.disableCondition.Equals(DisableCondition.None.ToG2SString()) ||
                !command.Command.enable &&
                command.Command.disableCondition.Equals(DisableCondition.ZeroCredits.ToG2SString()))
            {
                return new Error(ErrorCode.G2S_CCX018);
            }

            return null;
        }

        /// <summary>
        ///     The handler is invoked when command is received from a host
        /// </summary>
        /// <param name="command">The command to be processed</param>
        /// <returns>The Handler should return true if a response needs to generated, otherwise false.</returns>
        public async Task Handle(ClassCommand<commConfig, enterCommConfigMode> command)
        {
            var device = _egm.GetDevice<ICommConfigDevice>(command.IClass.deviceId);

            if (_configurationMode.Enabled(device) == command.Command.enable)
            {
                var response = command.GenerateResponse<commConfigModeStatus>();
                await _commandBuilder.Build(device, response.Command);
                return;
            }

            if (!command.Command.enable)
            {
                _configurationMode.Exit(
                    device,
                    DisableCondition.Immediate, ////** command.Command.disableCondition.DisableConditionFromG2SString(),
                    TimeSpan.FromMilliseconds(command.IClass.timeToLive),
                    success => HandleResponse(command, success, device));
            }
            else
            {
                _configurationMode.Enter(
                    device,
                    command.Command.disableCondition.DisableConditionFromG2SString(),
                    TimeSpan.FromMilliseconds(command.IClass.timeToLive),
                    () => command.Command.disableText,
                    true,
                    success => HandleResponse(command, success, device));
            }
        }

        private void HandleResponse(
            ClassCommand<commConfig, enterCommConfigMode> command,
            bool success,
            ICommConfigDevice device)
        {
            if (success)
            {
                var response = command.GenerateResponse<commConfigModeStatus>();
                _commandBuilder.Build(device, response.Command);

                if (command.Command.enable)
                {
                    _eventLift.Report(device, EventCode.G2S_CCE007, device.DeviceList(response.Command));
                    _eventLift.Report(device, EventCode.G2S_CCE101, device.DeviceList(response.Command));
                }
                else
                {
                    _eventLift.Report(device, EventCode.G2S_CCE008, device.DeviceList(response.Command));
                    _eventLift.Report(device, EventCode.G2S_CCE102, device.DeviceList(response.Command));
                }
            }
            else
            {
                command.Error.SetErrorCode(ErrorCode.G2S_APX011);
            }

            device.Queue.SendResponse(command);
        }
    }
}