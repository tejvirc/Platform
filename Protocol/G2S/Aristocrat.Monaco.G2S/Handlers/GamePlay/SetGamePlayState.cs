namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Gaming.Contracts;
    using Kernel;
    using Services;

    /// <summary>
    ///     An implementation of <see cref="ICommandHandler{TClass,TCommand}" />
    /// </summary>
    public class SetGamePlayState : ICommandHandler<gamePlay, setGamePlayState>
    {
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _commandBuilder;
        private readonly IDisableConditionSaga _disableCondition;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _properties;

        public SetGamePlayState(
            IG2SEgm egm,
            IDisableConditionSaga disableCondition,
            IGameProvider gameProvider,
            ICommandBuilder<IGamePlayDevice, gamePlayStatus> commandBuilder,
            IPropertiesManager properties,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _disableCondition = disableCondition ?? throw new ArgumentNullException(nameof(disableCondition));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public async Task<Error> Verify(ClassCommand<gamePlay, setGamePlayState> command)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(command.IClass.deviceId);

            var error = await Sanction.OnlyOwner<IGamePlayDevice>(_egm, command);
            if (error != null && error.IsError)
            {
                return error;
            }

            if (device.SetViaAccessConfig)
            {
                return new Error(ErrorCode.G2S_GPX005);
            }

            if (command.Command.applyCondition.ApplyConditionFromG2SString() == ApplyCondition.Disable &&
                command.Command.disableCondition.DisableConditionFromG2SString() == DisableCondition.None)
            {
                return new Error(ErrorCode.G2S_GPX003);
            }

            var game = _gameProvider.GetGame(command.IClass.deviceId);

            if (!_gameProvider.ValidateConfiguration(game))
            {
                return new Error(ErrorCode.G2S_GPX002);
            }

            return game == null ? new Error(ErrorCode.G2S_APX003) : null;
        }

        public async Task Handle(ClassCommand<gamePlay, setGamePlayState> command)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(command.IClass.deviceId);

            var applyCondition = command.Command.applyCondition.ApplyConditionFromG2SString();

            var enabled = command.Command.enable;

            if (device.HostEnabled == enabled)
            {
                var response = command.GenerateResponse<gamePlayStatus>();
                await _commandBuilder.Build(device, response.Command);
                return;
            }

            var disableCondition = command.Command.disableCondition.DisableConditionFromG2SString();

            var disableOverride = (DisableCondition)_properties.GetValue(GamingConstants.StateChangeOverride, DisableStrategy.None);
            if (disableOverride != DisableCondition.None &&
                (applyCondition == ApplyCondition.Immediate || applyCondition == ApplyCondition.Disable))
            {
                applyCondition = ApplyCondition.Disable;
                disableCondition = disableOverride > disableCondition ? disableOverride : disableCondition;
            }

            switch (applyCondition)
            {
                case ApplyCondition.Immediate:
                {
                    var response = command.GenerateResponse<gamePlayStatus>();
                    SetState(device, enabled, command.Command.disableText);
                    await _commandBuilder.Build(device, response.Command);
                }
                    break;
                case ApplyCondition.Cancel:
                {
                    var response = command.GenerateResponse<gamePlayStatus>();
                    await _commandBuilder.Build(device, response.Command);
                }
                    break;
                case ApplyCondition.EgmAction:
                    // We currently aren't supporting this type of apply condition
                    command.Error.SetErrorCode(ErrorCode.G2S_GPX004);
                    break;
                case ApplyCondition.Disable:
                    _disableCondition.Enter(
                        device,
                        disableCondition,
                        TimeSpan.FromMilliseconds(command.IClass.timeToLive),
                        () => command.Command.disableText,
                        success =>
                        {
                            if (success)
                            {
                                var status = new gamePlayStatus();

                                _commandBuilder.Build(device, status);
                                _eventLift.Report(device, EventCode.G2S_GPE007, device.DeviceList(status));

                                SetState(device, enabled, command.Command.disableText);

                                var response = command.GenerateResponse<gamePlayStatus>();
                                _commandBuilder.Build(device, response.Command);

                                _disableCondition.Exit(
                                    device,
                                    DisableCondition.Immediate,
                                    TimeSpan.Zero,
                                    _ =>
                                    {
                                        _commandBuilder.Build(device, status);
                                        _eventLift.Report(device, EventCode.G2S_GPE008, device.DeviceList(status));
                                    });
                            }
                            else
                            {
                                command.Error.SetErrorCode(ErrorCode.G2S_APX011);
                            }

                            device.Queue.SendResponse(command);
                        });
                    break;
                default:
                    command.Error.SetErrorCode(ErrorCode.G2S_GPX004);
                    break;
            }
        }

        private void SetState(IGamePlayDevice device, bool enabled, string disableText = null)
        {
            device.DisableText = disableText;
            device.HostEnabled = enabled;
        }
    }
}