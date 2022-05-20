namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;

    public class CancelBonusAward : ICommandHandler<bonus, cancelBonusAward>
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly IG2SEgm _egm;

        public CancelBonusAward(IG2SEgm egm, IBonusHandler bonusHandler)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
        }

        public async Task<Error> Verify(ClassCommand<bonus, cancelBonusAward> command)
        {
            var error = await Sanction.OnlyOwner<IBonusDevice>(_egm, command);
            if (error != null)
            {
                return error;
            }

            return !_bonusHandler.Exists(command.Command.transactionId) ? new Error(ErrorCode.G2S_BNX002) : null;
        }

        public async Task Handle(ClassCommand<bonus, cancelBonusAward> command)
        {
            _egm.GetDevice<IBonusDevice>(command.IClass.deviceId).NotifyActive();

            if (!_bonusHandler.Cancel(command.Command.transactionId))
            {
                command.Error.SetErrorCode(ErrorCode.G2S_BNX007);
                return;
            }

            var response = command.GenerateResponse<cancelBonusAwardAck>();

            response.Command.transactionId = command.Command.transactionId;
            response.Command.bonusId = command.Command.bonusId;

            await Task.CompletedTask;
        }
    }
}