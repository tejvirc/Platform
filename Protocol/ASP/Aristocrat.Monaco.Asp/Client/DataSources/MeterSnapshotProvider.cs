namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Utilities;
    using Contracts;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;

    /// <summary>
    ///     This class implements IMeterSnapshotProvider and evaluates necessary conditions prior to taking the snapshot. refer
    ///     Table 1 & Table 2 ASP5000 protocol document.
    /// </summary>
    public class MeterSnapshotProvider : IMeterSnapshotProvider, IDisposable
    {
        private const PersistenceLevel StorageLevel = PersistenceLevel.Critical;

        private const string InitialMetersField = "AuditUpdate.InitialMeters";

        private readonly IMeterManager _meterManager;
        private readonly IPersistentStorageAccessor _accessor;
        private readonly IGamePlayState _gamePlayState;
        private readonly IEventBus _eventBus;
        private readonly object _meterSnapshotLock = new object();

        private readonly IEnumerable<string> _meterNames = new List<string>
        {
            GamingMeters.EgmPaidBonusAmount,
            GamingMeters.EgmPaidGameWonAmount,
            GamingMeters.HandPaidBonusAmount,
            GamingMeters.PlayedCount,
            GamingMeters.TotalEgmPaidAmt,
            GamingMeters.TotalHandPaidAmt,
            GamingMeters.WageredAmount,
            GamingMeters.WonCount,
            GamingMeters.EgmPaidProgWonAmount,
            GamingMeters.HandPaidTotalWonAmount,
            AccountingMeters.CoinDrop,
            AccountingMeters.CurrencyInAmount,
            AccountingMeters.CurrentCredits,
            AccountingMeters.HandpaidCancelAmount,
            AccountingMeters.TotalVouchersIn,
            AccountingMeters.TotalVouchersOut,
            AccountingMeters.TrueCoinIn,
            AccountingMeters.TrueCoinOut,
            AccountingMeters.WatOnTotalAmount,
            AccountingMeters.WatOffTotalAmount
        };

        private readonly IDictionary<string, long> _meterSnapshot = new ConcurrentDictionary<string, long>();
        private bool _disposed;

        public MeterSnapshotProvider(
            IMeterManager meterManager,
            IPersistentStorageManager storageManager,
            IGamePlayState gamePlayState,
            IEventBus eventBus)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            if (storageManager == null)
            {
                throw new ArgumentNullException(nameof(storageManager));
            }

            _gamePlayState = gamePlayState ?? throw new ArgumentNullException(nameof(gamePlayState));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _accessor = storageManager.GetAccessor(StorageLevel, GetType().ToString());
            LoadSnapshot();
            _eventBus.Subscribe<GameEndedEvent>(this, OnGameEnded);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public MeterSnapshotStatus SnapshotStatus
        {
            get => (MeterSnapshotStatus)_accessor[AspConstants.AuditUpdateStatusField];

            set
            {
                _accessor[AspConstants.AuditUpdateStatusField] = value;
                CreatePersistentSnapshot();
            }
        }

        public long GetSnapshotMeter(string name)
        {
            return !_meterSnapshot.ContainsKey(name) ? 0 : _meterSnapshot[name];
        }

        public void CreatePersistentSnapshot(bool notifyOnCompletion = true)
        {
            lock (_meterSnapshotLock)
            {
                if (!_gamePlayState.Idle || SnapshotStatus != MeterSnapshotStatus.Enabled)
                {
                    return;
                }

                _meterSnapshot.Clear();

                var snapshot = _meterManager.CreateSnapshot(_meterNames, MeterValueType.Lifetime);
                snapshot.Add(AspConstants.AuditUpdateTimeStampField, DateTimeHelper.GetNumberOfSecondsSince1990());
                foreach (var meter in snapshot)
                {
                    _meterSnapshot.Add(meter);
                }

                using (var transaction = _accessor.StartTransaction())
                {
                    transaction.UpdateList(
                        InitialMetersField,
                        snapshot.Select(m => new SnapshotMeter { Name = m.Key, Value = m.Value }).ToList());

                    transaction.Commit();
                }

                if (notifyOnCompletion)
                {
                    _eventBus.Publish(new MeterSnapshotCompletedEvent());
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void OnGameEnded(GameEndedEvent thEvent)
        {
            CreatePersistentSnapshot();
        }

        private void LoadSnapshot()
        {
            var meters = _accessor.GetList<SnapshotMeter>(InitialMetersField);

            foreach (var meter in meters)
            {
                _meterSnapshot.Add(meter.Name, meter.Value);
            }
        }
    }
}