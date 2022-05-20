namespace Aristocrat.Monaco.G2S.Handlers.Bonus
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts.Localization;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts.Bonus;
    using Gaming.Contracts.Session;
    using Kernel;
    using Localization.Properties;

    [ProhibitWhenDisabled]
    public class SetBonusAward : ICommandHandler<bonus, setBonusAward>
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly IPlayerService _players;
        private readonly IPropertiesManager _properties;
        private readonly IG2SEgm _egm;

        public SetBonusAward(
            IG2SEgm egm,
            IBonusHandler bonusHandler,
            IPlayerService players,
            IPropertiesManager properties)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _players = players ?? throw new ArgumentNullException(nameof(players));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public async Task<Error> Verify(ClassCommand<bonus, setBonusAward> command)
        {
            _egm.GetDevice<IBonusDevice>(command.IClass.deviceId)?.NotifyActive();

            var error = await Sanction.OnlyOwner<IBonusDevice>(_egm, command);
            if (error != null && error.IsError)
            {
                return error;
            }

            var mode = command.Command.bonusMode.ToBonusMode();
            switch (mode)
            {
                case BonusMode.MultipleJackpotTime:
                    {
                        const int maxMjtActiveCount = 3;

                        var activeCount = _bonusHandler.Transactions.Count(
                            t => t.Mode == BonusMode.MultipleJackpotTime && t.State == BonusState.Pending);

                        if (activeCount >= maxMjtActiveCount)
                        {
                            return new Error(ErrorCode.G2S_BNX008);
                        }

                        break;
                    }
                case BonusMode.Standard when command.Command.payMethod != t_bonusPayMethods.G2S_payHandpay &&
                                             command.Command.bonusAwardAmt == 0:
                    return new Error(ErrorCode.G2S_BNX002);
            }

            // Per the spec, for any non-cashable award that would result in a handpay,
            //  the EGM MUST fail the award if the handpay class is not configured to support non-cashable handpays
            if (command.Command.creditType == t_creditTypes.G2S_nonCash &&
                command.Command.payMethod == t_bonusPayMethods.G2S_payHandpay &&
                !_properties.GetValue(AccountingConstants.RequestNonCash, false))
            {
                return new Error(ErrorCode.G2S_BNX002);
            }

            return null;
        }

        public async Task Handle(ClassCommand<bonus, setBonusAward> command)
        {
            var device = _egm.GetDevice<IBonusDevice>(command.IClass.deviceId);

            device.NotifyActive();

            var mode = command.Command.bonusMode.ToBonusMode();

            IBonusRequest request;

            switch (mode)
            {
                case BonusMode.Standard:
                case BonusMode.NonDeductible:
                    request = ToStandardBonus(device, command.Command);
                    break;
                case BonusMode.WagerMatch:
                    request = ToWagerMatchBonus(device, command.Command);
                    break;
                case BonusMode.MultipleJackpotTime:
                    request = ToMjtBonus(device, command.Command);
                    break;
                default:
                    command.Error.SetErrorCode(ErrorCode.G2S_BNX002);
                    return;
            }

            var transaction = _bonusHandler.Award(request);

            var response = command.GenerateResponse<bonusAwardAck>();

            response.Command.transactionId = transaction.TransactionId;
            response.Command.bonusId = Convert.ToInt64(transaction.BonusId);
            response.Command.idNumber = transaction.IdNumber;
            response.Command.idReaderType = command.Command.idReaderType; //TODO
            response.Command.playerId = transaction.PlayerId;

            await Task.CompletedTask;
        }

        private IBonusRequest ToStandardBonus(IBonusDevice device, setBonusAward award)
        {
            long cashableAmount = 0;
            long nonCashAmount = 0;
            long promoAmount = 0;

            switch (award.creditType)
            {
                case t_creditTypes.G2S_cashable:
                    cashableAmount = award.bonusAwardAmt;
                    break;
                case t_creditTypes.G2S_nonCash:
                    nonCashAmount = award.bonusAwardAmt;
                    break;
                case t_creditTypes.G2S_promo:
                    promoAmount = award.bonusAwardAmt;
                    break;
            }

            var request = new StandardBonus(
                award.bonusId.ToString(),
                cashableAmount,
                nonCashAmount,
                promoAmount,
                award.payMethod.ToPayMethod(),
                protocol: CommsProtocol.G2S)
            {
                MessageDuration = TimeSpan.FromMilliseconds(award.msgDuration),
                DisplayLimit = device.DisplayLimit,
                DisplayLimitText =
                    device.DisplayLimitText ??
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded),
                DisplayLimitTextDuration = device.DisplayLimitDuration
            };

            SetBaseParameters(device, request, award);

            return request;
        }

        private IBonusRequest ToWagerMatchBonus(IBonusDevice device, setBonusAward award)
        {
            long cashableAmount = 0;
            long nonCashAmount = 0;
            long promoAmount = 0;

            switch (award.creditType)
            {
                case t_creditTypes.G2S_cashable:
                    cashableAmount = award.wmAwardAmt;
                    break;
                case t_creditTypes.G2S_nonCash:
                    nonCashAmount = award.wmAwardAmt;
                    break;
                case t_creditTypes.G2S_promo:
                    promoAmount = award.wmAwardAmt;
                    break;
            }

            // This may not hold up. The spec says to evaluate both, but using the min value seems to make the most sense.  Only one of the two can fail the transaction
            var useWagerMatchLimit = device.WagerMatchLimit > 0 && device.DisplayLimit >= device.WagerMatchLimit;

            var request = new WagerMatchBonus(
                award.bonusId.ToString(),
                cashableAmount,
                nonCashAmount,
                promoAmount,
                award.payMethod.ToPayMethod(),
                award.bonusAwardAmt == 0 ? BonusException.None : BonusException.Failed, CommsProtocol.G2S)
            {
                DisplayLimit = useWagerMatchLimit ? device.WagerMatchLimit : device.DisplayLimit,
                DisplayLimitText =
                    (useWagerMatchLimit ? device.WagerMatchLimitText : device.DisplayLimitText) ??
                    Localizer.For(CultureFor.Player).GetString(ResourceKeys.BonusLimitExceeded),
                DisplayLimitTextDuration =
                    useWagerMatchLimit ? device.WagerMatchLimitDuration : device.DisplayLimitDuration
            };

            SetBaseParameters(device, request, award);

            // In the spec, this appears to supersede the id restriction of the command
            request.IdRequired = device.WagerMatchCardRequired;

            return request;
        }

        private IBonusRequest ToMjtBonus(IBonusDevice device, setBonusAward award)
        {
            var request = new MultipleJackpotTimeBonus(
                award.bonusId.ToString(),
                award.creditType.ToAccountType(),
                award.payMethod.ToPayMethod(), protocol: CommsProtocol.G2S)
            {
                WagerRestriction = award.wagerRestriction.ToWagerRestriction(),
                MinimumWin = award.minMultWin,
                MaximumWin = award.maxMultWin,
                AutoPlay = award.autoPlay,
                EndOnCardOut = award.cardOutEnd,
                EndOnCashOut = award.cashOutEnd,
                Games = award.numberOfGames,
                WinMultiplier = award.winMultiplier,
                RejectLowCredits = award.lowCreditReject,
                Timeout = TimeSpan.FromMilliseconds(award.timeout),
                TimeoutRule = award.timeoutRule.ToTimeoutRule()
            };

            if (award.startTimeSpecified)
            {
                request.Start = award.startTime.ToUniversalTime();
            }

            if (award.stopTimeSpecified)
            {
                request.End = award.stopTime.ToUniversalTime();
            }

            SetBaseParameters(device, request, award);

            return request;
        }

        private void SetBaseParameters(IBonusDevice device, IBonusRequest request, setBonusAward award)
        {
            request.Message = award.textMessage;

            switch (award.idRestrict)
            {
                case t_idRestricts.G2S_thisId:
                    request.IdRequired = true;
                    request.IdNumber = award.idNumber;
                    request.PlayerId = award.playerId;
                    break;
                case t_idRestricts.G2S_anyId:
                    request.IdRequired = true;
                    if (_players.HasActiveSession)
                    {
                        request.IdNumber = _players.ActiveSession?.Player.Number;
                        request.PlayerId = _players.ActiveSession?.Player.PlayerId;
                    }

                    break;
                case t_idRestricts.G2S_none:
                    request.IdRequired = false;
                    if (_players.HasActiveSession)
                    {
                        request.IdNumber = _players.ActiveSession?.Player.Number;
                        request.PlayerId = _players.ActiveSession?.Player.PlayerId;
                    }

                    break;
            }

            request.IdReaderType = award.idReaderType.ToReaderType();

            request.OverrideEligibility = award.overrideEligibleTimer;
            request.EligibilityTimer = device.EligibilityTimer;
        }
    }
}