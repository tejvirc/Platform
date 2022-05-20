namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    public class SetActiveDenominations : ICommandHandler<gamePlay, setActiveDenoms>
    {
        private readonly ICommandBuilder<IGamePlayDevice, gameDenomList> _denomListBuilder;
        private readonly IDisableConditionSaga _disableCondition;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly ICommandBuilder<IGamePlayDevice, gamePlayStatus> _gamePlayStatusBuilder;
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _properties;

        public SetActiveDenominations(
            IG2SEgm egm,
            IDisableConditionSaga disableCondition,
            IGameProvider gameProvider,
            ICommandBuilder<IGamePlayDevice, gameDenomList> denomListBuilder,
            ICommandBuilder<IGamePlayDevice, gamePlayStatus> gamePlayStatusBuilder,
            IPropertiesManager properties,
            IEventLift eventLift)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _disableCondition = disableCondition ?? throw new ArgumentNullException(nameof(disableCondition));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _denomListBuilder = denomListBuilder ?? throw new ArgumentNullException(nameof(denomListBuilder));
            _gamePlayStatusBuilder =
                gamePlayStatusBuilder ?? throw new ArgumentNullException(nameof(gamePlayStatusBuilder));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
        }

        public async Task<Error> Verify(ClassCommand<gamePlay, setActiveDenoms> command)
        {
            var error = await Sanction.OnlyOwner<IGamePlayDevice>(_egm, command);
            if (error != null && error.IsError)
            {
                return error;
            }

            var device = _egm.GetDevice<IGamePlayDevice>(command.IClass.deviceId);
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
            if (game == null)
            {
                return new Error(ErrorCode.G2S_APX003);
            }

            if (!_gameProvider.ValidateConfiguration(game, GetActiveDenoms(command.Command)))
            {
                return new Error(ErrorCode.G2S_GPX002);
            }

            var activeDenoms = GetActiveDenoms(command.Command);

            return activeDenoms.Except(game.SupportedDenominations).Any() ? new Error(ErrorCode.G2S_GPX001) : null;
        }

        public async Task Handle(ClassCommand<gamePlay, setActiveDenoms> command)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(command.IClass.deviceId);

            var applyCondition = command.Command.applyCondition.ApplyConditionFromG2SString();

            var activeDenoms = GetActiveDenoms(command.Command).ToList();

            var game = _gameProvider.GetGame(command.IClass.deviceId);
            if (!game.ActiveDenominations.Except(activeDenoms).Any() && !activeDenoms.Except(game.ActiveDenominations).Any())
            {
                var response = command.GenerateResponse<gameDenomList>();
                await _denomListBuilder.Build(device, response.Command);
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
                    var response = command.GenerateResponse<gameDenomList>();
                    SetDenominations(device, activeDenoms);
                    await _denomListBuilder.Build(device, response.Command);
                }
                    break;
                case ApplyCondition.Disable:
                    _disableCondition.Enter(
                        device,
                        disableCondition,
                        TimeSpan.FromMilliseconds(command.IClass.timeToLive),
                        () => string.Empty,
                        success =>
                        {
                            if (success)
                            {
                                var status = new gamePlayStatus();

                                _gamePlayStatusBuilder.Build(device, status);
                                _eventLift.Report(device, EventCode.G2S_GPE007, device.DeviceList(status));

                                SetDenominations(device, activeDenoms);

                                var response = command.GenerateResponse<gameDenomList>();
                                _denomListBuilder.Build(device, response.Command);

                                _disableCondition.Exit(
                                    device,
                                    DisableCondition.Immediate,
                                    TimeSpan.Zero,
                                    _ =>
                                    {
                                        _gamePlayStatusBuilder.Build(device, status);
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
                case ApplyCondition.EgmAction:
                    // We currently aren't supporting this type of apply condition
                    command.Error.SetErrorCode(ErrorCode.G2S_GPX004);
                    break;
                case ApplyCondition.Cancel:
                    {
                        var response = command.GenerateResponse<gameDenomList>();
                        await _denomListBuilder.Build(device, response.Command);
                    }
                    break;
                default:
                    command.Error.SetErrorCode(ErrorCode.G2S_GPX004);
                    break;
            }
        }

        private static IEnumerable<long> GetActiveDenoms(c_activeDenomList denomList)
        {
            if (denomList?.Items == null)
            {
                return Enumerable.Empty<long>();
            }

            var activeDenoms = new List<long>();

            foreach (var item in denomList.Items)
            {
                activeDenoms.AddRange(GetDenoms((dynamic)item));
            }

            return activeDenoms;
        }

        private static IEnumerable<long> GetDenoms(c_activeRange range)
        {
            var denomList = new List<long>();

            for (var denom = range.denomMin; denom <= range.denomMax; denom += range.denomInterval)
            {
                denomList.Add(denom);
            }

            return denomList;
        }

        private static IEnumerable<long> GetDenoms(c_activeDenom denom)
        {
            return new List<long> { denom.denomId };
        }

        private void SetDenominations(IGamePlayDevice device, IEnumerable<long> activeDenoms)
        {
            _gameProvider.SetActiveDenominations(device.Id, activeDenoms);

            var game = _gameProvider.GetGame(device.Id);

            var status = new gamePlayStatus();
            _gamePlayStatusBuilder.Build(device, status);

            status.egmEnabled = game.EgmEnabled;

            _eventLift.Report(device, EventCode.G2S_GPE201, device.DeviceList(status));
            _eventLift.Report(device, EventCode.G2S_GPE005, device.DeviceList(status));
        }
    }
}