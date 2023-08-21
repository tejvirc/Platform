namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Contracts;
    using Extensions;
    using Gaming.Contracts;
    using Kernel;

    /// <summary>
    ///     This class provides the snapshot of USA meters as per the definition in Class 2 Type 3 Parameter 5 in ASP5000
    ///     protocol document.
    ///     It communicates with MeterSnapshotProvider to create snapshot, retrieve snapshot meter values.
    /// </summary>
    public class UsaMeterSnapshotMetersDataSource : IDisposableDataSource, IParameterLoadActions
    {
        private readonly IMeterSnapshotProvider _meterSnapshotProvider;

        private readonly Dictionary<string, Func<object>> _handlers;

        private readonly IEventBus _eventBus;

        private bool _disposed;

        public UsaMeterSnapshotMetersDataSource(IMeterSnapshotProvider meterSnapshotProvider, IEventBus eventBus)
        {
            _meterSnapshotProvider =
                meterSnapshotProvider ?? throw new ArgumentNullException(nameof(meterSnapshotProvider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _eventBus.Subscribe<MeterSnapshotCompletedEvent>(this, MeterSnapshotCompleted);
            _handlers = GetMembersMap();
        }

        public IReadOnlyList<string> Members => _handlers.Keys.ToList();

        public string Name => "UsaMeterSnapShot";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

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

        private void MeterSnapshotCompleted(MeterSnapshotCompletedEvent theEvent)
        {
            MemberValueChanged?.Invoke(this, this.GetMemberSnapshot(Members));
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
                    "USA_Coin_In_Meter",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.WageredAmount).MillicentsToCents()
                },
                {
                    "USA_Coin_Out_Meter",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.TotalEgmPaidAmt).MillicentsToCents()
                },
                {
                    "USA_Drop_Meter",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.CurrencyInAmount).MillicentsToCents() +
                          _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.CoinDrop).MillicentsToCents()
                },
                {
                    "USA_Jackpot_Meter",
                    () => _meterSnapshotProvider.GetSnapshotMeter(GamingMeters.TotalHandPaidAmt).MillicentsToCents()
                },
                {
                    "USA_Cancel_Credit_Meter",
                    () => _meterSnapshotProvider.GetSnapshotMeter(AccountingMeters.HandpaidCancelAmount).MillicentsToCents()
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
                }
            };
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _eventBus?.UnsubscribeAll(this);
            }

            _disposed = true;
        }
    }
}