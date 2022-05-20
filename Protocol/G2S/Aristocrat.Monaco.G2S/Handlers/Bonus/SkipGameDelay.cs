namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;

    public class SkipGameDelay : ICommandHandler<bonus, skipGameDelay>
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _commandBuilder;
        private readonly IG2SEgm _egm;

        public SkipGameDelay(
            IG2SEgm egm,
            IBonusHandler bonusHandler,
            ICommandBuilder<IBonusDevice, bonusStatus> commandBuilder)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _commandBuilder = commandBuilder ?? throw new ArgumentNullException(nameof(commandBuilder));
        }

        public async Task<Error> Verify(ClassCommand<bonus, skipGameDelay> command)
        {
            return await Sanction.OnlyOwner<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<bonus, skipGameDelay> command)
        {
            var device = _egm.GetDevice<IBonusDevice>(command.IClass.deviceId);

            device.NotifyActive();

            _bonusHandler.SkipGameEndDelay();

            var response = command.GenerateResponse<bonusStatus>();

            await _commandBuilder.Build(device, response.Command);
        }
    }
}