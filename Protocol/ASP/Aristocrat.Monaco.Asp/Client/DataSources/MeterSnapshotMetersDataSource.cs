namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Gaming.Contracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     This class provides the snapshot of meters as per the definition in Class 2 Type 3 Parameter 3 in ASP5000
    ///     protocol document.
    ///     It communicates with MeterSnapshotProvider to retrieve snapshot meter values.
    /// </summary>
    public class MeterSnapshotMetersDataSource : IDataSource, IParameterLoadActions
    {
        private readonly IMeterSnapshotProvider _meterSnapshotProvider;

        private readonly Dictionary<string, Func<object>> _handlers;

        public MeterSnapshotMetersDataSource(IMeterSnapshotProvider meterSnapshotProvider)
        {
            _meterSnapshotProvider = meterSnapshotProvider ?? throw new ArgumentNullException(nameof(meterSnapshotProvider));

            _handlers = GetMembersMap();

            Members = _handlers.Keys.ToList();
        }

        public IReadOnlyList<string> Members { get; }

        public string Name => "MeterSnapShot";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged = (sender, s) => { };

        public object GetMemberValue(string member)
        {
            return _handlers[member]();
        }

        public void SetMemberValue(string member, object value)
        {
        }

        public void PreLoad()
        {
            _meterSnapshotProvider.CreatePersistentSnapshot(false);
        }

        private Dictionary<string, Func<object>> GetMembersMap()
        {
            return new Dictionary<string, Func<object>>
            {
                {
                    "Time_Stamp",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AspConstants.AuditUpdateTimeStampField)
                },
                {
                    "Credit_Meter",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.CurrentCredits).MillicentsToCents()
                },
                {
                    "Total_Games_Completed",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.PlayedCount)
                },
                {
                    "Total_Games_Won",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.WonCount)
                },
                {
                    "Total_Turnover",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.WageredAmount).MillicentsToCents()
                },
                {
                    "Total_MoneyIn_As_Coins",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.TrueCoinIn).MillicentsToCents()
                },
                {
                    "Total_MoneyIn_As_Bills",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.CurrencyInAmount).MillicentsToCents()
                },
                {
                    "Total_MoneyOut_As_Coins",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.TrueCoinOut).MillicentsToCents()
                },
                {
                    "Total_MoneyIn_CashBox",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.CoinDrop).MillicentsToCents()
                },
                {
                    "Total_Cashless_Credit_Transfer_In",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.WatOnTotalAmount).MillicentsToCents()
                },
                {
                    "Total_Cashless_Credit_Transfer_Out",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.WatOffTotalAmount).MillicentsToCents()
                },
                {
                    "Total_Money_Won_ExBonus",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.EgmPaidGameWonAmount).MillicentsToCents() +
                          _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.EgmPaidProgWonAmount).MillicentsToCents()
                },
                {
                    "Total_BMoney_Won_ToHandpay",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.HandPaidBonusAmount).MillicentsToCents()
                },
                {
                    "Total_BMoney_Won_ToCrdMeter",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.EgmPaidBonusAmount).MillicentsToCents()
                },
                {
                    "Total_MoneyOut_As_Handpay",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.HandpaidCancelAmount).MillicentsToCents() +
                          _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.TotalVouchersOut).MillicentsToCents() +
                          _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.HandPaidTotalWonAmount).MillicentsToCents()
                },
                {
                    "Total_MoneyOut_As_Tickets",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.TotalVouchersOut).MillicentsToCents()
                },
                {
                    "Total_MoneyIn_As_Tickets",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.TotalVouchersIn).MillicentsToCents()
                }
            };
        }
    }
}