namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.MeterPage;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Tickets;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Kernel.Contracts;
    using Localization.Properties;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    [CLSCompliant(false)]
    public class VoucherMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly (string Label, string MeterName, string VisibleSetting)[] _metersIn =
        {
            (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CashableVoucherInLabelContent),
                "VoucherInCashable",
                null),
            (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CashablePromoVoucherInLabelContent),
                "VoucherInCashablePromotional",
                OperatorMenuSetting.ShowCashablePromo),
            (Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NonCashablePromoVoucherInLabelContent),
                "VoucherInNonCashablePromotional",
                null)
        };

        private readonly (string Label, string MeterName, string VisibleSetting)[] _metersOut =
        {
            (Localizer.For(CultureFor.Operator).
                GetString(ResourceKeys.CashableVoucherOutLabelContent), "VoucherOutCashable",
                null),
            (Localizer.For(CultureFor.Operator).
                GetString(ResourceKeys.CashablePromoVoucherOutLabelContent), "VoucherOutCashablePromotional",
                OperatorMenuSetting.ShowCashablePromo),
            (Localizer.For(CultureFor.Operator).
                GetString(ResourceKeys.NonCashablePromoVoucherOutLabelContent), "VoucherOutNonCashablePromotional",
                null)
        };

        private readonly (string Label, string MeterName)[] _rejectedMeters =
        {
            (Localizer.For(CultureFor.Operator)
                .GetString(ResourceKeys.VouchersRejectedLabel), AccountingMeters.VouchersRejectedCount)
        };

        private long _voucherInTotalCount;
        private string _voucherInTotalValue;
        private string _voucherOutTotalValue;
        private long _voucherOutTotalCount;
        private bool _isVoucherInVisible = true;
        private long _rejectedVoucherCount;
        private IMeter _voucherInCountMeter;
        private IMeter _voucherInValueMeter;
        private IMeter _voucherOutCountMeter;
        private IMeter _voucherOutValueMeter;
        private bool _showCategoryCounts;

        private readonly object _voucherLock = new object();

        public VoucherMetersPageViewModel() : base(MeterNodePage.Voucher)
        {
        }

        public ObservableCollection<CountDisplayMeter> VoucherInMeters { get; } =
            new ObservableCollection<CountDisplayMeter>();

        public bool IsVoucherInVisible
        {
            get => _isVoucherInVisible;
            set
            {
                _isVoucherInVisible = value;
                RaisePropertyChanged(nameof(IsVoucherInVisible));
            }
        }

        public long VoucherInTotalCount
        {
            get => _voucherInTotalCount;
            set
            {
                _voucherInTotalCount = value;
                RaisePropertyChanged(nameof(VoucherInTotalCount));
            }
        }

        public string VoucherInTotalValue
        {
            get => _voucherInTotalValue;
            set
            {
                _voucherInTotalValue = value;
                RaisePropertyChanged(nameof(VoucherInTotalValue));
            }
        }

        public ObservableCollection<CountDisplayMeter> VoucherOutMeters { get; } =
            new ObservableCollection<CountDisplayMeter>();

        public long VoucherOutTotalCount
        {
            get => _voucherOutTotalCount;
            set
            {
                _voucherOutTotalCount = value;
                RaisePropertyChanged(nameof(VoucherOutTotalCount));
            }
        }

        public string VoucherOutTotalValue
        {
            get => _voucherOutTotalValue;
            set
            {
                _voucherOutTotalValue = value;
                RaisePropertyChanged(nameof(VoucherOutTotalValue));
            }
        }

        public long RejectedVoucherCount
        {
            get => _rejectedVoucherCount;
            set
            {
                _rejectedVoucherCount = value;
                RaisePropertyChanged(nameof(RejectedVoucherCount));
            }
        }

        public bool ShowCategoryCounts
        {
            get => _showCategoryCounts;
            set
            {
                _showCategoryCounts = value;
                RaisePropertyChanged(nameof(ShowCategoryCounts));
            }
        }

        public ObservableCollection<DisplayMeter> RejectionMeters { get; } =
            new ObservableCollection<DisplayMeter>();

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            var tickets = new List<Ticket>();
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IMetersTicketCreator>();
            var printMeters = new List<Tuple<Tuple<IMeter, IMeter>, string>>();

            if (IsVoucherInVisible)
            {
                printMeters.AddRange(VoucherInMeters.
                    Select(m => new Tuple<Tuple<IMeter, IMeter>, string>(new Tuple<IMeter, IMeter>(ShowCategoryCounts ? m.CountMeter : null, m.ValueMeter), m.Name))
                    .ToList());

                printMeters.Add(new Tuple<Tuple<IMeter, IMeter>, string>(
                    new Tuple<IMeter, IMeter>(_voucherInCountMeter, _voucherInValueMeter),
                    Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherInLabelContent)));

                tickets.Add(ticketCreator?.CreateEgmMetersTicket(printMeters, ShowLifetime));
                printMeters.Clear();
            }

            printMeters.AddRange(VoucherOutMeters.
                Select(m => new Tuple<Tuple<IMeter, IMeter>, string>(new Tuple<IMeter, IMeter>(ShowCategoryCounts ? m.CountMeter : null, m.ValueMeter), m.Name))
                .ToList());

            printMeters.Add(new Tuple<Tuple<IMeter, IMeter>, string>(
                new Tuple<IMeter, IMeter>(_voucherOutCountMeter, _voucherOutValueMeter),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherOutLabelContent)));

            printMeters.AddRange(RejectionMeters.
                Select(m => new Tuple<Tuple<IMeter, IMeter>, string>(new Tuple<IMeter, IMeter>(m.Meter, null), m.Name))
                .ToList());

            tickets.Add(ticketCreator?.CreateEgmMetersTicket(printMeters, ShowLifetime));

            return tickets;
        }

        protected override void OnLoaded()
        {
            IsVoucherInVisible = PropertiesManager.GetValue(PropertyKey.VoucherIn, true);
            ShowCategoryCounts = GetConfigSetting(OperatorMenuSetting.ShowCategoryCounts, true);
            base.OnLoaded();
            InitializeMeters(); // need to run again in the case detailed in VLT-12225 to sync master/period status
        }

        protected override void InitializeMeters()
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            ClearMeters();
            lock (_voucherLock)
            {
                foreach (var meterIn in _metersIn)
                {
                    var visible = string.IsNullOrEmpty(meterIn.VisibleSetting) || GetConfigSetting(meterIn.VisibleSetting, true);
                    if (!visible)
                    {
                        continue;
                    }

                    var count = meterManager.GetMeter(meterIn.MeterName + "Count");
                    var value = meterManager.GetMeter(meterIn.MeterName + "Value");

                    var displayNode = MeterNodes?.FirstOrDefault(a => a.Name.Equals(meterIn.MeterName));

                    var meter = new CountDisplayMeter(displayNode?.DisplayName ?? meterIn.Label, count, value, ShowLifetime);
                    meter.PropertyChanged += OnMeterChangedEvent;

                    VoucherInMeters.Add(meter);
                }

                foreach (var meterOut in _metersOut)
                {
                    var visible = string.IsNullOrEmpty(meterOut.VisibleSetting) || GetConfigSetting(meterOut.VisibleSetting, true);
                    if (!visible)
                    {
                        continue;
                    }

                    var count = meterManager.GetMeter(meterOut.MeterName + "Count");
                    var value = meterManager.GetMeter(meterOut.MeterName + "Value");

                    var displayNode = MeterNodes?.FirstOrDefault(a => a.Name.Equals(meterOut.MeterName));

                    var meter = new CountDisplayMeter(displayNode?.DisplayName ?? meterOut.Label, count, value, ShowLifetime);
                    meter.PropertyChanged += OnMeterChangedEvent;

                    VoucherOutMeters.Add(meter);
                }

                _voucherInCountMeter = meterManager.GetMeter(AccountingMeters.TotalVouchersInCount);
                _voucherInValueMeter = meterManager.GetMeter(AccountingMeters.TotalVouchersIn);
                _voucherOutCountMeter = meterManager.GetMeter(AccountingMeters.TotalVouchersOutCount);
                _voucherOutValueMeter = meterManager.GetMeter(AccountingMeters.TotalVouchersOut);

                RejectionMeters.Clear();

                foreach (var rejectMeter in _rejectedMeters)
                {
                    var count = meterManager.GetMeter(rejectMeter.MeterName);

                    var meter = new DisplayMeter(rejectMeter.Label, count, ShowLifetime);

                    RejectionMeters.Add(meter);
                }

                if (RejectionMeters.Any())
                {
                    RejectedVoucherCount = RejectionMeters.First().Count;
                }
            }

            UpdateMeterTotals();
        }

        protected override void UpdateMeters()
        {
            //base.UpdateMeters(); // commented out to fix VLT-12203 no need to call base here since we don't access base Meters collection in this page
            lock (_voucherLock)
            {
                foreach (var meter in VoucherInMeters)
                {
                    meter.ShowLifetime = ShowLifetime;
                }

                foreach (var meter in VoucherOutMeters)
                {
                    meter.ShowLifetime = ShowLifetime;
                }

                foreach (var meter in RejectionMeters)
                {
                    meter.ShowLifetime = ShowLifetime;
                }
            }

            UpdateMeterTotals();
        }

        private void OnMeterChangedEvent(object sender, PropertyChangedEventArgs e)
        {
            UpdateMeterTotals();
        }

        private void UpdateMeterTotals()
        {
            lock (_voucherLock)
            {
                VoucherInTotalCount = ShowLifetime ? _voucherInCountMeter.Lifetime : _voucherInCountMeter.Period;
                VoucherInTotalValue = (ShowLifetime ? _voucherInValueMeter.Lifetime : _voucherInValueMeter.Period).MillicentsToDollars().FormattedCurrencyString();
                VoucherOutTotalCount = ShowLifetime ? _voucherOutCountMeter.Lifetime : _voucherOutCountMeter.Period;
                VoucherOutTotalValue = (ShowLifetime ? _voucherOutValueMeter.Lifetime : _voucherOutValueMeter.Period).MillicentsToDollars().FormattedCurrencyString();
            }
        }

        protected override void DisposeInternal()
        {
            ClearMeters();

            base.DisposeInternal();
        }

        private void ClearMeters()
        {
            lock (_voucherLock)
            {
                foreach (var meter in VoucherInMeters)
                {
                    meter.PropertyChanged -= OnMeterChangedEvent;
                    meter.Dispose();
                }
                VoucherInMeters.Clear();

                foreach (var meter in VoucherOutMeters)
                {
                    meter.PropertyChanged -= OnMeterChangedEvent;
                    meter.Dispose();
                }
                VoucherOutMeters.Clear();
            }
        }
    }
}
