namespace Aristocrat.Monaco.G2S.Handlers.OptionConfig
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
    ///     Implementation of 'enterOptionConfigMode' command of 'OptionConfig' G2S class.
    /// </summary>
    public class EnterOptionConfigMode : ICommandHandler<optionConfig, enterOptionConfigMode>
    {
        private readonly ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> _commandBuilder;
        private readonly IDisableConditionSaga _configurationMode;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnterOptionConfigMode" /> class.
        /// </summary>
        /// <param name="egm">A G2S egm.</param>
        /// <param name="configurationMode">The configuration mode.</param>
        /// <param name="commandBuilder">Command builder.</param>
        /// <param name="eventLift">The event lift.</param>
        public EnterOptionConfigMode(
            IG2SEgm egm,
            IDisableConditionSaga configurationMode,
            ICommandBuilder<IOptionConfigDevice, optionConfigModeStatus> commandBuilder,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _configurationMode = configurationMode ?? throw new ArgumentNullException(nameof(configurationMode));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<optionConfig, enterOptionConfigMode> command)
        {
            if (command.Command.disableCondition.Equals(DisableCondition.None.ToG2SString()) ||
                !command.Command.enable &&
                command.Command.disableCondition.Equals(DisableCondition.ZeroCredits.ToG2SString()))
            {
                return new Error(ErrorCode.G2S_OCX017);
            }

            return await Sanction.OnlyOwner<IOptionConfigDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<optionConfig, enterOptionConfigMode> command)
        {
            var device = _egm.GetDevice<IOptionConfigDevice>(command.IClass.deviceId);

            if (_configurationMode.Enabled(device) == command.Command.enable)
            {
                var response = command.GenerateResponse<optionConfigModeStatus>();
                await _commandBuilder.Build(device, response.Command);
                return;
            }

            if (!command.Command.enable)
            {
                _configurationMode.Exit(
                    device,
                    DisableCondition.Immediate, ////** command.Command.disableCondition.DisableConditionFromG2SString(),
                    TimeSpan.FromMilliseconds(command.IClass.timeToLive),
                    success => { ChangeConfigModeCallback(command, success, device, false); });
            }
            else
            {
                _configurationMode.Enter(
                    device,
                    command.Command.disableCondition.DisableConditionFromG2SString(),
                    TimeSpan.FromMilliseconds(command.IClass.timeToLive),
                    () => command.Command.disableText,
                    true,
                    success => { ChangeConfigModeCallback(command, success, device, true); });
            }
        }

        // Extract callback function in separate method allows to implement a unit test for it.
        // Because when we implement a unit test for this class, we will be Mock<IConfigurationModeSaga>.
        private void ChangeConfigModeCallback(
            ClassCommand<optionConfig, enterOptionConfigMode> command,
            bool success,
            IOptionConfigDevice device,
            bool enterMode)
        {
            if (success)
            {
                var response = command.GenerateResponse<optionConfigModeStatus>();
                _commandBuilder.Build(device, response.Command);

                if (enterMode)
                {
                    _eventLift.Report(device, EventCode.G2S_OCE007, device.DeviceList(response.Command));
                    _eventLift.Report(device, EventCode.G2S_OCE101, device.DeviceList(response.Command));
                }
                else
                {
                    _eventLift.Report(device, EventCode.G2S_OCE008, device.DeviceList(response.Command));
                    _eventLift.Report(device, EventCode.G2S_OCE102, device.DeviceList(response.Command));
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