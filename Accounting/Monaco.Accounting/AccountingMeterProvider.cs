namespace Aristocrat.Monaco.Accounting
{
    using System.Collections.Generic;
    using Application.Contracts;
    using Application.Contracts.Metering;
    using Contracts;
    using Kernel;

    public class AccountingMeterProvider : BaseMeterProvider
    {
        public AccountingMeterProvider()
            : base(typeof(AccountingMeterProvider).ToString())
        {
            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.TotalOut,
                    timeFrame =>
                    {
                        var meters = ServiceManager.GetInstance().GetService<IMeterManager>();

                        return meters.GetMeter(AccountingMeters.TotalVouchersOut).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HardMeterOutAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HandpaidCashableAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HandpaidPromoAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HandpaidNonCashableAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.ElectronicTransfersOffTotalAmount).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.TotalVouchersOut,
                        AccountingMeters.HardMeterOutAmount,
                        AccountingMeters.HandpaidCashableAmount,
                        AccountingMeters.HandpaidPromoAmount,
                        AccountingMeters.HandpaidNonCashableAmount,
                        AccountingMeters.ElectronicTransfersOffTotalAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.TotalIn,
                    timeFrame =>
                    {
                        var meters = ServiceManager.GetInstance().GetService<IMeterManager>();

                        return meters.GetMeter(AccountingMeters.CurrencyInAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.TotalVouchersIn).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.ElectronicTransfersOnTotalAmount).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.CurrencyInAmount,
                        AccountingMeters.TotalVouchersIn,
                        AccountingMeters.ElectronicTransfersOnTotalAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.TotalOutHard,
                    timeFrame =>
                    {
                        var meters = ServiceManager.GetInstance().GetService<IMeterManager>();

                        return meters.GetMeter(AccountingMeters.TotalVouchersOut).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HardMeterOutAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.WatOffTotalAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HandpaidCashableAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HandpaidPromoAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HandpaidNonCashableAmount).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.TotalVouchersOut,
                        AccountingMeters.HardMeterOutAmount,
                        AccountingMeters.WatOffTotalAmount,
                        AccountingMeters.HandpaidCashableAmount,
                        AccountingMeters.HandpaidPromoAmount,
                        AccountingMeters.HandpaidNonCashableAmount
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalHandpaidCashVoucherAndTickets,
                    timeFrame =>
                    {
                        var meters = ServiceManager.GetInstance().GetService<IMeterManager>();

                        return meters.GetMeter(AccountingMeters.HandpaidCashableAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.HandpaidPromoAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.TotalVouchersOut).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.HandpaidCashableAmount,
                        AccountingMeters.HandpaidPromoAmount,
                        AccountingMeters.TotalVoucherOutCashableAndPromoAmount,
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.CurrentCredits,
                    timeFrame =>
                    {
                        var bank = ServiceManager.GetInstance().GetService<IBank>();

                        return bank.QueryBalance();
                    },
                    new List<string>(),
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    ApplicationMeters.DropMeter,
                    timeFrame =>
                    {
                        var meters = ServiceManager.GetInstance().GetService<IMeterManager>();

                        return meters.GetMeter(AccountingMeters.CurrencyInAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.TotalVouchersIn).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.CurrencyInAmount,
                        AccountingMeters.TotalVouchersIn
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalCashCoinTicketInAmount,
                    timeFrame =>
                    {
                        var meters = ServiceManager.GetInstance().GetService<IMeterManager>();

                        return meters.GetMeter(AccountingMeters.CurrencyInAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.TrueCoinIn).GetValue(timeFrame)+
                               meters.GetMeter(AccountingMeters.TotalVouchersIn).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.CurrencyInAmount,
                        AccountingMeters.TrueCoinIn,
                        AccountingMeters.TotalVouchersIn
                    },
                    new CurrencyMeterClassification()));

            AddMeter(
                new CompositeMeter(
                    AccountingMeters.TotalCancelCreditAmount,
                    timeFrame =>
                    {
                        var meters = ServiceManager.GetInstance().GetService<IMeterManager>();

                        return meters.GetMeter(AccountingMeters.HandpaidCancelAmount).GetValue(timeFrame) +
                               meters.GetMeter(AccountingMeters.TotalVouchersOut).GetValue(timeFrame);
                    },
                    new List<string>
                    {
                        AccountingMeters.HandpaidCancelAmount,
                        AccountingMeters.TotalVouchersOut
                    },
                    new CurrencyMeterClassification()));

        }
    }
}