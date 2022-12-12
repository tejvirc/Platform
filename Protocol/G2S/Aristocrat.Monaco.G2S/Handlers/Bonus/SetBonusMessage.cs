namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Kernel.Contracts.MessageDisplay;
    using Gaming.Contracts.Session;
    using Kernel.MessageDisplay;

    [ProhibitWhenDisabled]
    public class SetBonusMessage : ICommandHandler<bonus, setBonusMessage>
    {
        private static readonly Guid BonusMessageId = Guid.NewGuid();

        private readonly IG2SEgm _egm;
        private readonly IMessageDisplay _messages;
        private readonly IPlayerService _players;

        public SetBonusMessage(IG2SEgm egm, IMessageDisplay messages, IPlayerService players)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));
            _players = players ?? throw new ArgumentNullException(nameof(players));
        }

        public async Task<Error> Verify(ClassCommand<bonus, setBonusMessage> command)
        {
            var error = await Sanction.OnlyOwner<IBonusDevice>(_egm, command);
            if (error != null && error.IsError)
            {
                return error;
            }

            return IsValid(command) ? null : new Error(ErrorCode.G2S_BNX009);
        }

        public Task Handle(ClassCommand<bonus, setBonusMessage> command)
        {
            _egm.GetDevice<IBonusDevice>(command.IClass.deviceId).NotifyActive();

            _messages.RemoveMessage(BonusMessageId);

            if (command.Command.msgDuration > 0)
            {
                _messages.DisplayMessage(
                    new DisplayableMessage(
                        () => command.Command.textMessage,
                        DisplayableMessageClassification.Informative,
                        DisplayableMessagePriority.Normal,
                        BonusMessageId),
                    command.Command.msgDuration);
            }

            command.GenerateResponse<bonusMessageAck>();

            return Task.CompletedTask;
        }

        private bool IsValid(ClassCommand<bonus, setBonusMessage> command)
        {
            if (command.Command.idRestrict == t_idRestricts.G2S_none)
            {
                return true;
            }

            if (!_players.HasActiveSession || !_players.ActiveSession.Player.ValidationExpired)
            {
                return false;
            }

            var device = _egm.GetDevice<IBonusDevice>(command.IClass.deviceId);
            if (device == null || device.IdReaderId == 0)
            {
                return false;
            }

            var idReader = _egm.GetDevice<IIdReaderDevice>(command.IClass.deviceId);
            if (!idReader.IsEnabled())
            {
                return false;
            }

            if (command.Command.idRestrict == t_idRestricts.G2S_thisId)
            {
                if (string.IsNullOrEmpty(command.Command.idNumber) && string.IsNullOrEmpty(command.Command.playerId))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(command.Command.playerId) &&
                    _players.ActiveSession.Player.PlayerId != command.Command.playerId)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(command.Command.idNumber) &&
                    _players.ActiveSession.Player.Number != command.Command.idNumber)
                {
                    return false;
                }
            }

            return true;
        }
    }
}