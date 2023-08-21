namespace Aristocrat.Monaco.Asp.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts;
    using Contracts;
    using Extensions;
    using Gaming.Contracts;
    using Kernel;

    /// <inheritdoc />
    // ReSharper disable once UnusedMember.Global
    public class EgmMetersDataSource : IDisposableDataSource
    {
        /// <summary>
        ///     The category of the meter
        /// </summary>
        public enum MeterCategory
        {
            Currency,
            Occurrence
        }

        private readonly IMeterManager _meterManager;

        private readonly List<DataMemberMeterMapping> _metersMapping = new List<DataMemberMeterMapping>();

        private readonly IEventBus _eventBus;

        private bool _disposed;

        public EgmMetersDataSource(IMeterManager meterManager, IEventBus eventBus)
        {
            _meterManager = meterManager ?? throw new ArgumentNullException(nameof(meterManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            // Populate DataMembers here, ideally these should be part of a factory.
            _metersMapping.Add(new DataMemberMeterMapping("Games_Completed", GamingMeters.PlayedCount, category: MeterCategory.Occurrence));
            _metersMapping.Add(new DataMemberMeterMapping("Total_Turnover", GamingMeters.WageredAmount));
            _metersMapping.Add(new DataMemberMeterMapping("Reserved", AccountingConstants.LargeWinLimit));
            _metersMapping.Add(new DataMemberMeterMapping("Tot_Money_Won_ToHandpaid", GamingMeters.HandPaidBonusAmount));
            _metersMapping.Add(new DataMemberMeterMapping("Total_Money_Won_ToCreditMeter", GamingMeters.EgmPaidBonusAmount));
            _metersMapping.Add(new DataMemberMeterMapping("Current_Credit_Base_Units", AccountingMeters.CurrentCredits));

            _eventBus.Subscribe<BankBalanceChangedEvent>(this, OnBankBalanceChanged);
            BindMeterChangedEventHandlers();
        }

        public IReadOnlyList<string> Members => _metersMapping.Select(x => x.MemberName).ToList();

        public string Name { get; } = "EGMMeters";

        public event EventHandler<Dictionary<string, object>> MemberValueChanged;

        public object GetMemberValue(string member)
        {
            var mapping = _metersMapping.Find(x => x.MemberName == member);
            if (mapping != null)
            {
                return GetMeterValue(mapping);
            }

            return null;
        }

        public void SetMemberValue(string member, object value)
        {
        }

        private void OnBankBalanceChanged(BankBalanceChangedEvent theEvent)
        {
            if (theEvent.OldBalance != theEvent.NewBalance)
            {
                MemberValueChanged?.Invoke(this, new Dictionary<string, object> { { "Current_Credit_Base_Units" , theEvent.NewBalance} });
            }
        }

        private void BindMeterChangedEventHandlers()
        {
            var maps = from map in _metersMapping
                where map.Subscribed == false && _meterManager.IsMeterProvided(map.MeterName)
                select map;

            foreach (var map in maps)
            {
                var meter = _meterManager.GetMeter(map.MeterName);
                meter.MeterChangedEvent += OnMeterValueChanged;
                map.Subscribed = true;
            }
        }

        private void UnbindMeterChangedEventHandlers()
        {
            if (_meterManager == null) return;

            var maps = from map in _metersMapping
                where map.Subscribed == true && _meterManager.IsMeterProvided(map.MeterName)
                select map;

            foreach (var map in maps)
            {
                var meter = _meterManager.GetMeter(map.MeterName);
                if (meter == null) continue;
                meter.MeterChangedEvent -= OnMeterValueChanged;
                map.Subscribed = false;
            }
        }

        private void OnMeterValueChanged(object sender, MeterChangedEventArgs e)
        {
            if (!(sender is IMeter meter))
            {
                return;
            }

            var maps = from map in _metersMapping
                where map.MeterName == meter.Name
                select map;

            foreach (var map in maps)
            {
                MemberValueChanged?.Invoke(this, this.GetMemberSnapshot(map.MemberName));
            }
        }

        private object GetMeterValue(DataMemberMeterMapping meterMap)
        {
            return _meterManager.IsMeterProvided(meterMap.MeterName)
                ? meterMap.MeterCategory == MeterCategory.Currency
                    ? _meterManager.GetMeter(meterMap.MeterName).Lifetime.MillicentsToCents()
                    : _meterManager.GetMeter(meterMap.MeterName).Lifetime
                : 0;
        }

        private class DataMemberMeterMapping
        {
            public DataMemberMeterMapping(
                string memberName = "",
                string meterName = "",
                bool subscribed = false,
                MeterCategory category = MeterCategory.Currency)
            {
                MemberName = memberName;
                MeterName = meterName;
                Subscribed = subscribed;
                MeterCategory = category;
            }

            public string MemberName { get; }
            public string MeterName { get; }
            public bool Subscribed { get; set; }

            public MeterCategory MeterCategory { get; }
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
                UnbindMeterChangedEventHandlers();
            }

            _disposed = true;
        }
    }
}