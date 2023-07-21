namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using Application.Contracts;
    using Application.Contracts.Tickets;
    using Application.UI.OperatorMenu;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.MeterPage;
    using Monaco.Localization.Properties;
    using Aristocrat.Toolkit.Mvvm.Extensions;

    [CLSCompliant(false)]
    public class WatMetersPageViewModel : MetersPageViewModelBase
    {
        private long _watOnTotalCount;
        private string _watOnTotalValue;
        private string _watOffTotalValue;
        private long _watOffTotalCount;
        
        public WatMetersPageViewModel() : base(null)
        {
        }

        public ObservableCollection<CountDisplayMeter> WatOnMeters { get; } =
            new ObservableCollection<CountDisplayMeter>();

        public long WatOnTotalCount
        {
            get => _watOnTotalCount;
            set
            {
                _watOnTotalCount = value;
                OnPropertyChanged(nameof(WatOnTotalCount));
            }
        }

        public string WatOnTotalValue
        {
            get => _watOnTotalValue;
            set
            {
                _watOnTotalValue = value;
                OnPropertyChanged(nameof(WatOnTotalValue));
            }
        }

        public ObservableCollection<CountDisplayMeter> WatOffMeters { get; } =
            new ObservableCollection<CountDisplayMeter>();

        public long WatOffTotalCount
        {
            get => _watOffTotalCount;
            set
            {
                _watOffTotalCount = value;
                OnPropertyChanged(nameof(WatOffTotalCount));
            }
        }

        public string WatOffTotalValue
        {
            get => _watOffTotalValue;
            set
            {
                _watOffTotalValue = value;
                OnPropertyChanged(nameof(WatOffTotalValue));
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateMeters();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var serviceManager = ServiceManager.GetInstance();
            var ticketCreator = serviceManager.TryGetService<IMetersTicketCreator>();
            var printMeters = WatOnMeters.Select(m => new Tuple<IMeter, string>(m.CountMeter, m.Name)).ToList();
            printMeters.AddRange(WatOffMeters.Select(m => new Tuple<IMeter, string>(m.CountMeter, m.Name)));

            return TicketToList(ticketCreator?.CreateEgmMetersTicket(printMeters, ShowLifetime));  
        }

        protected override void InitializeMeters()
        {
            AddMeters();
            UpdateMeterTotals();
        }

        protected override void UpdateMeters()
        {
            base.UpdateMeters();
            RefreshMeters();
        }

        protected override void DisposeInternal()
        {
            RemoveMeters();
            base.DisposeInternal();
        }

        private void UpdateMeterTotals()
        {
            WatOnTotalCount = WatOnMeters.Sum(m => m.Count);
            WatOnTotalValue = WatOnMeters.Sum(m => m.MeterValue).FormattedCurrencyString();

            WatOffTotalCount = WatOffMeters.Sum(m => m.Count);
            WatOffTotalValue = WatOffMeters.Sum(m => m.MeterValue).FormattedCurrencyString();
        }

        protected override void RefreshMeters()
        {
            Execute.OnUIThread(
                () =>
                {
                    RemoveMeters();
                    AddMeters();
                    OnPropertyChanged(nameof(WatOnMeters));
                    OnPropertyChanged(nameof(WatOffMeters));
                    UpdateMeterTotals();
                });
        }

        private void AddMeters()
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            foreach (var meterOn in GetMetersOnNames())
            {
                var count = meterManager.GetMeter(meterOn.Item2 + "Count");
                var value = meterManager.GetMeter(meterOn.Item2 + "Value");

                var meter = new CountDisplayMeter(meterOn.Item1, count, value, ShowLifetime);

                WatOnMeters.Add(meter);
            }

            foreach (var meterOff in GetMetersOffNames())
            {
                var count = meterManager.GetMeter(meterOff.Item2 + "Count");
                var value = meterManager.GetMeter(meterOff.Item2 + "Value");

                var meter = new CountDisplayMeter(meterOff.Item1, count, value, ShowLifetime);

                WatOffMeters.Add(meter);
            }
        }

        private void RemoveMeters()
        {
            foreach (var meter in WatOnMeters)
            {
                meter.Dispose();
            }

            WatOnMeters.Clear();

            foreach (var meter in WatOffMeters)
            {
                meter.Dispose();
            }

            WatOffMeters.Clear();
        }

        private Tuple<string, string>[] GetMetersOnNames()
        {
            return new Tuple<string, string>[]
            {
                Tuple.Create(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashableWatOnLabelContent), "WatOnCashable"),
                Tuple.Create(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashablePromoWatOnLabelContent), "WatOnCashablePromotional"),
                Tuple.Create(Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NonCashablePromoWatOnLabelContent), "WatOnNonCashablePromotional")
            };
        }

        private Tuple<string, string>[] GetMetersOffNames()
        {
            return new Tuple<string, string>[]
            {
                Tuple.Create( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashableWatOffLabelContent), "WatOffCashable"),
                Tuple.Create( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.CashablePromoWatOffLabelContent), "WatOffCashablePromotional"),
                Tuple.Create( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.NonCashablePromoWatOffLabelContent), "WatOffNonCashablePromotional")
            };
        }
    }
}
