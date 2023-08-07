namespace Aristocrat.G2S.Emdi.Handlers.Meters
{
    using Emdi.Meters;
    using log4net;
    using Monaco.Accounting.Contracts;
    using Protocol.v21ext1b1;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Emdi.Host;

    /// <summary>
    /// Handles the <see cref="getMeterInfo"/> command
    /// </summary>
    public class GetMeterInfo : CommandHandler<getMeterInfo>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private readonly IBank _bank;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetMeterInfo"/> class.
        /// </summary>
        /// <param name="bank"></param>
        public GetMeterInfo(
            IBank bank)
        {
            _bank = bank;
        }

        /// <inheritdoc />
        public override Task<CommandResult> ExecuteAsync(getMeterInfo command)
        {
            Logger.Debug($"EMDI: Executing {command.GetType().Name} command on port {Context.Config.Port}");

            if (command.meterSubscription == null)
            {
                return Task.FromResult(InvalidXml());
            }

            var meters = new List<c_meterInfo>();

            try
            {
                meters.AddRange(GetMeters(command.meterSubscription));
            }
            catch (Exception ex)
            {
                Logger.Error($"EMDI: Error retrieving meter info on port {Context.Config.Port}", ex);
            }

            return Task.FromResult(
                Success(
                    new meterReport
                    {
                        meterInfo = meters.ToArray()
                    }));
        }

        private IEnumerable<c_meterInfo> GetMeters(IEnumerable<c_meterSubscription> subs)
        {
            foreach (var sub in subs)
            {
                switch (sub.meterName)
                {
                    case MeterNames.G2SPlayerCashableAmt:
                    {
                        yield return new c_meterInfo
                        {
                            meterName = MeterNames.G2SPlayerCashableAmt,
                            meterType = t_meterTypes.IGT_amount,
                            meterValue = _bank.QueryBalance(AccountType.Cashable)
                        };

                        break;
                    }

                    case MeterNames.G2SPlayerNonCashAmt:
                    {
                        yield return new c_meterInfo
                        {
                            meterName = MeterNames.G2SPlayerNonCashAmt,
                            meterType = t_meterTypes.IGT_amount,
                            meterValue = _bank.QueryBalance(AccountType.NonCash)
                        };

                        break;
                    }

                    case MeterNames.G2SPlayerPromoAmt:
                    {
                        yield return new c_meterInfo
                        {
                            meterName = MeterNames.G2SPlayerPromoAmt,
                            meterType = t_meterTypes.IGT_amount,
                            meterValue = _bank.QueryBalance(AccountType.Promo)
                        };

                        break;
                    }
                }
            }
        }
    }
}
