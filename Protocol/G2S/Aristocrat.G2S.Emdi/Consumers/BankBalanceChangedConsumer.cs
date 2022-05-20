namespace Aristocrat.G2S.Emdi.Consumers
{
    using Host;
    using log4net;
    using Meters;
    using Monaco.Accounting.Contracts;
    using Protocol.v21ext1b1;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Consumes the <see cref="BankBalanceChangedEvent"/> event
    /// </summary>
    public class BankBalanceChangedConsumer : Consumes<BankBalanceChangedEvent>
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBank _bank;
        private readonly IReporter _reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BankBalanceChangedConsumer"/> class.
        /// </summary>
        /// <param name="bank"></param>
        /// <param name="reporter"></param>
        public BankBalanceChangedConsumer(
            IBank bank,
            IReporter reporter)
        {
            _bank = bank;
            _reporter = reporter;
        }

        /// <inheritdoc />
        protected override async Task ConsumeAsync(BankBalanceChangedEvent theEvent)
        {
            try
            {
                Logger.Debug("EMDI: Received BankBalanceChangedEvent event");

                await _reporter.ReportAsync(GetMeters(), MeterNames.G2SPlayerNonCashAmt, MeterNames.G2SPlayerPromoAmt, MeterNames.G2SPlayerCashableAmt);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending cabinet meter report", ex);
            }
        }

        private IEnumerable<c_meterInfo> GetMeters()
        {
            yield return new c_meterInfo
            {
                meterName = MeterNames.G2SPlayerCashableAmt,
                meterType = t_meterTypes.IGT_amount,
                meterValue = _bank.QueryBalance(AccountType.Cashable)
            };

            yield return new c_meterInfo
            {
                meterName = MeterNames.G2SPlayerNonCashAmt,
                meterType = t_meterTypes.IGT_amount,
                meterValue = _bank.QueryBalance(AccountType.NonCash)
            };

            yield return new c_meterInfo
            {
                meterName = MeterNames.G2SPlayerPromoAmt,
                meterType = t_meterTypes.IGT_amount,
                meterValue = _bank.QueryBalance(AccountType.Promo)
            };
        }
    }
}
