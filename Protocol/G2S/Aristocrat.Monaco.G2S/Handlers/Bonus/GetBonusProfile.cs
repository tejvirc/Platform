namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;
    using Localization.Properties;

    public class GetBonusProfile : ICommandHandler<bonus, getBonusProfile>
    {
        private readonly IG2SEgm _egm;
        private readonly ITransactionHistory _transactions;
        private readonly IBonusHandler _bonuses;

        public GetBonusProfile(IG2SEgm egm, ITransactionHistory transactions, IBonusHandler bonuses)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            _bonuses = bonuses ?? throw new ArgumentNullException(nameof(bonuses));
        }

        public async Task<Error> Verify(ClassCommand<bonus, getBonusProfile> command)
        {
            return await Sanction.OwnerAndGuests<IBonusDevice>(_egm, command);
        }

        public async Task Handle(ClassCommand<bonus, getBonusProfile> command)
        {
            var device = _egm.GetDevice<IBonusDevice>(command.IClass.deviceId);

            device.NotifyActive();

            var response = command.GenerateResponse<bonusProfile>();

            response.Command.configurationId = device.ConfigurationId;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = _transactions.GetMaxTransactions<BonusTransaction>();
            response.Command.timeToLive = device.TimeToLive;
            response.Command.noMessageTimer = (int)device.NoResponseTimer.TotalMilliseconds;
            response.Command.noHostText = device.NoHostText;
            response.Command.idReaderId = device.IdReaderId;
            response.Command.maxPendingBonus = _bonuses.MaxPending;
            response.Command.allowMulticast = t_g2sBoolean.G2S_false;
            response.Command.eligibleTimer = (int)device.EligibilityTimer.TotalMilliseconds;
            response.Command.displayLimit = device.DisplayLimit;
            response.Command.displayLimitText = device.DisplayLimitText ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded);
            response.Command.displayLimitDuration = (int)device.DisplayLimitDuration.TotalMilliseconds;
            response.Command.wmCardRequired = device.WagerMatchCardRequired;
            response.Command.wmLimit = device.WagerMatchLimit;
            response.Command.wmLimitText = device.WagerMatchLimitText ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded);
            response.Command.wmLimitDuration = (int)device.WagerMatchLimitDuration.TotalMilliseconds;
            response.Command.wmExitText = device.WagerMatchExitText ?? Localizer.For(CultureFor.Player).GetString(ResourceKeys.WagerMatchExitText);
            response.Command.wmExitDuration = (int)device.WagerMatchExitDuration.TotalMilliseconds;
            response.Command.wmCardRequired = device.WagerMatchCardRequired;

            response.Command.configComplete = device.ConfigComplete;
            response.Command.configDateTime = device.ConfigDateTime;

            await Task.CompletedTask;
        }
    }
}