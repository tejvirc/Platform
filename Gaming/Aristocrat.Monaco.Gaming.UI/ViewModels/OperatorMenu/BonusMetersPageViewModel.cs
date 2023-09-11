namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.OperatorMenu;
    using Application.UI.MeterPage;
    using Contracts;
    using Contracts.Bonus;
    using Gaming.Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using MVVM;

    [CLSCompliant(false)]
    public class BonusMetersPageViewModel : MetersPageViewModelBase
    {
        private string _egmPaidBonusAwardsTotalAmountFormatted;
        private string _handPaidBonusAwardsTotalAmountFormatted;

        public BonusMetersPageViewModel()
            : base(null, true)
        {
        }

        public ObservableCollection<BonusInfoMeter> EgmPaidBonusAwardsMeters { get; } =
            new ObservableCollection<BonusInfoMeter>();

        public string EgmPaidBonusAwardsTotalAmountFormatted
        {
            get => _egmPaidBonusAwardsTotalAmountFormatted;
            set
            {
                _egmPaidBonusAwardsTotalAmountFormatted = value;
                RaisePropertyChanged(nameof(EgmPaidBonusAwardsTotalAmountFormatted));
            }
        }

        public ObservableCollection<BonusInfoMeter> HandPaidBonusAwardsMeters { get; } =
            new ObservableCollection<BonusInfoMeter>();

        public string HandPaidBonusAwardsTotalAmountFormatted
        {
            get => _handPaidBonusAwardsTotalAmountFormatted;
            set
            {
                _handPaidBonusAwardsTotalAmountFormatted = value;
                RaisePropertyChanged(nameof(HandPaidBonusAwardsTotalAmountFormatted));
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateMeters();
        }

        protected override void InitializeMeters()
        {
            AddMeters();
            UpdateMeterTotals();
        }

        protected override void UpdateMeters()
        {
            RefreshMeters();
        }

        protected override void DisposeInternal()
        {
            RemoveMeters();
            base.DisposeInternal();
        }

        protected override void RefreshMeters()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    RemoveMeters();
                    AddMeters();
                    RaisePropertyChanged(nameof(EgmPaidBonusAwardsMeters));
                    RaisePropertyChanged(nameof(HandPaidBonusAwardsMeters));
                    UpdateMeterTotals();
                });
        }

        private void UpdateMeterTotals()
        {
            var culture = GetCurrencyDisplayCulture();

            EgmPaidBonusAwardsTotalAmountFormatted =
                EgmPaidBonusAwardsMeters.Sum(m => m.MeterValue).FormattedCurrencyString(false, culture);

            HandPaidBonusAwardsTotalAmountFormatted =
                HandPaidBonusAwardsMeters.Sum(m => m.MeterValue).FormattedCurrencyString(false, culture);
        }

        private void AddMeters()
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            foreach (var egmPaidBonusAward in GetEGMPaidBonusAwardNames())
            {
                var value = meterManager.GetMeter(egmPaidBonusAward.meterName);
                var meter = new BonusInfoMeter(egmPaidBonusAward.meterLabel, ShowLifetime, value);

                EgmPaidBonusAwardsMeters.Add(meter);
            }

            foreach (var handPaidBonusAward in GetHandPaidBonusAwardNames())
            {
                var value = meterManager.GetMeter(handPaidBonusAward.meterName);
                var meter = new BonusInfoMeter(handPaidBonusAward.meterLabel, ShowLifetime, value);

                HandPaidBonusAwardsMeters.Add(meter);
            }
        }

        private void RemoveMeters()
        {
            foreach(var meter in EgmPaidBonusAwardsMeters)
            {
                meter.Dispose();
            }

            foreach(var meter in HandPaidBonusAwardsMeters)
            {
                meter.Dispose();
            }

            EgmPaidBonusAwardsMeters.Clear();

            HandPaidBonusAwardsMeters.Clear();
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;
            var serviceManager = ServiceManager.GetInstance();
            var ticketCreator = serviceManager.TryGetService<IGameBonusInfoTicketCreator>();
            if (ticketCreator == null)
            {
                return null;
            }

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                    tickets = GetGameBonusMeterTickets(ticketCreator);
                    break;
            }

            return tickets;
        }

        private IEnumerable<Ticket> GetGameBonusMeterTickets(IGameBonusInfoTicketCreator ticketCreator)
        {
            var ticketList = new List<Ticket>();
            ticketList.Add(ticketCreator.Create(
                (ResourceKeys.MachinePaidBonusAwardsLabel, ResourceKeys.MachinePaidBonusTotalLabel),
                EgmPaidBonusAwardsMeters));

            ticketList.Add(ticketCreator.Create(
                (ResourceKeys.AttendantPaidBonusAwardsLabel, ResourceKeys.AttendantPaidBonusTotalLabel),
                HandPaidBonusAwardsMeters));

            return ticketList;
        }

        private (string meterLabel, string meterName)[] GetEGMPaidBonusAwardNames()
        {
            return new (string meterLabel, string meterName)[]
            {
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftCashableBonusLabel), BonusMeters.BonusBaseCashableInAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftNonCashableBonusLabel), GamingMeters.EgmPaidBonusNonCashInAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftPromotionalBonusLabel), GamingMeters.EgmPaidBonusPromoInAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MjtBonusLabel), BonusMeters.EgmPaidMjtBonusAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WagerMatchBonusLabel), GamingMeters.EgmPaidBonusWagerMatchAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasLegacyBonusTaxDeductibleLabel), GamingMeters.EgmPaidBonusDeductibleAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasLegacyBonusNonTaxDeductibleLabel), GamingMeters.EgmPaidBonusNonDeductibleAmount)
            };
        }

        private (string meterLabel, string meterName)[] GetHandPaidBonusAwardNames()
        {
            return new (string meterLabel, string meterName)[]
            {
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftCashableBonusLabel), BonusMeters.HandPaidBonusBaseCashableInAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftNonCashableBonusLabel), GamingMeters.HandPaidBonusNonCashInAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AftPromotionalBonusLabel), GamingMeters.HandPaidBonusPromoInAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.MjtBonusLabel), BonusMeters.HandPaidMjtBonusAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WagerMatchBonusLabel), GamingMeters.HandPaidBonusWagerMatchAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasLegacyBonusTaxDeductibleLabel), GamingMeters.HandPaidBonusDeductibleAmount),
                ( Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasLegacyBonusNonTaxDeductibleLabel), GamingMeters.HandPaidBonusNonDeductibleAmount)
            };
        }
    }
}