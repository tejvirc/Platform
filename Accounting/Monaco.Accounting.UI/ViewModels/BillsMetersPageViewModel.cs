﻿namespace Aristocrat.Monaco.Accounting.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Tickets;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using MVVM;
    using MVVM.Command;

    [CLSCompliant(false)]
    public class BillsMetersPageViewModel : MetersPageViewModelBase
    {
        private const string BillCountMeterNamePrefix = "BillCount";
        private static readonly string CurrencyInMeterProviderTypename = typeof(CurrencyInMetersProvider).ToString();

        private readonly Tuple<string, string>[] _rejectedMeters =
        {
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket)
                    .GetString(ResourceKeys.BillsRejectedLabel),
                AccountingMeters.BillsRejectedCount),
            Tuple.Create(
                Localizer.For(CultureFor.OperatorTicket)
                    .GetString(ResourceKeys.DocumentsRejectedLabel),
                AccountingMeters.DocumentsRejectedCount)
        };

        private long _totalCount;
        private string _totalValue;
        private bool _billClearanceButtonEnabled;
        private readonly string _pageName;

        public BillsMetersPageViewModel(string pageName)
            : base(null)
        {
            _pageName = pageName;
            BillClearanceButtonClickedCommand = new ActionCommand<object>(BillClearance_Clicked);
            BillClearanceEnabled = (bool)PropertiesManager.GetProperty(AccountingConstants.BillClearanceEnabled, false);
            _billClearanceButtonEnabled = GameIdle;
        }

        public ICommand BillClearanceButtonClickedCommand { get; }

        public bool BillClearanceEnabled { get; }

        public long TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                RaisePropertyChanged(nameof(TotalCount));
            }
        }

        public string TotalValue
        {
            get => _totalValue;
            set
            {
                _totalValue = value;
                RaisePropertyChanged(nameof(TotalValue));
            }
        }

        public bool BillClearanceButtonEnabled
        {
            get => _billClearanceButtonEnabled && FieldAccessEnabled && BillClearanceEnabled;

            set
            {
                _billClearanceButtonEnabled = value;
                RaisePropertyChanged(nameof(BillClearanceButtonEnabled));
            }
        }

        public ObservableCollection<DisplayMeter> RejectionMeters { get; } =
            new ObservableCollection<DisplayMeter>();

        protected override void OnFieldAccessEnabledChanged()
        {
            if (BillClearanceEnabled)
                RaisePropertyChanged(nameof(BillClearanceButtonEnabled));
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            InitializeMeters();
            PublishPeriodMetersDateTimeChangeRequestEvent();
        }

        private void PublishPeriodMetersDateTimeChangeRequestEvent()
        {
            if (!BillClearanceEnabled)
            {
                return;
            }

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            eventBus.Publish(
                new PeriodMetersDateTimeChangeRequestEvent(
                    _pageName,
                    meterManager.GetPeriodMetersClearanceDate(CurrencyInMeterProviderTypename)));
        }

        protected override void InitializeMeters()
        {
            if (!ServiceManager.GetInstance().IsServiceAvailable<INoteAcceptor>())
            {
                return;
            }

            var noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            ClearMeters();
            foreach (var denomination in noteAcceptor.GetSupportedNotes())
            {
                var meter = meterManager.GetMeter(
                    BillCountMeterNamePrefix + denomination.ToString(CultureInfo.InvariantCulture) + "s");

                meter.MeterChangedEvent += OnMeterChangedEvent;

                Meters.Add(new DenominationDisplayMeter(denomination, meter, ShowLifetime));
            }

            RejectionMeters.Clear();

            foreach (var rejectMeter in _rejectedMeters)
            {
                var count = meterManager.GetMeter(rejectMeter.Item2);

                var meter = new DisplayMeter(rejectMeter.Item1, count, ShowLifetime);
                RejectionMeters.Add(meter);
            }

            UpdateMeterTotals();
        }

        protected override void UpdateMeters()
        {
            base.UpdateMeters();

            foreach (var meter in RejectionMeters)
            {
                meter.ShowLifetime = ShowLifetime;
                meter.OnMeterChangedEvent();
            }

            UpdateMeterTotals();
            PublishPeriodMetersDateTimeChangeRequestEvent();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IMetersTicketCreator>();
            var printMeters = Meters.Where(m => m.Meter != null).Select(m => new Tuple<IMeter, string>(m.Meter, m.Name))
                .ToList();

            printMeters.AddRange(RejectionMeters.Select(m => new Tuple<IMeter, string>(m.Meter, m.Name)).ToList());

            return TicketToList(ticketCreator?.CreateEgmMetersTicket(printMeters, ShowLifetime));
        }

        protected override void DisposeInternal()
        {
            ClearMeters();
            base.DisposeInternal();
        }

        private void BillClearance_Clicked(object sender)
        {
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            var result = dialogService.ShowYesNoDialog(
                this,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConfirmBillClearance));

            if (result != true)
            {
                return;
            }

            ClearBillCountPeriodMeters();
            MvvmHelper.ExecuteOnUI(UpdateMeters);
        }

        private void ClearBillCountPeriodMeters()
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();
            meterManager.ClearPeriodMeters(CurrencyInMeterProviderTypename);
        }

        private void OnMeterChangedEvent(object sender, MeterChangedEventArgs e)
        {
            UpdateMeterTotals();
        }

        private void UpdateMeterTotals()
        {
            TotalCount = Meters.ToList().Sum(m => ((DenominationDisplayMeter)m).Count);
            TotalValue = Meters.ToList().Sum(m => ((DenominationDisplayMeter)m).Total).FormattedCurrencyString();
        }

        private void ClearMeters()
        {
            foreach (var meter in Meters)
            {
                meter.Meter.MeterChangedEvent -= OnMeterChangedEvent;
            }

            Meters.Clear();

            foreach (var meter in RejectionMeters)
            {
                meter.Meter.MeterChangedEvent -= OnMeterChangedEvent;
            }

            RejectionMeters.Clear();
        }
    }
}