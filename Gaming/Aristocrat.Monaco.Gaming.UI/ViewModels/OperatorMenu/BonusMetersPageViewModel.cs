namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Bonus;
    using Kernel;
    using Localization.Properties;
    using MVVM;

    [CLSCompliant(false)]
    public class BonusMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly (string meterLabel, string meterName)[] _egmPaidBonusAwards =
        {
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AftCashableBonusLabel), BonusMeters.BonusBaseCashableInAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AftNonCashableBonusLabel), GamingMeters.EgmPaidBonusNonCashInAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AftPromotionalBonusLabel), GamingMeters.EgmPaidBonusPromoInAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.MjtBonusLabel), BonusMeters.EgmPaidMjtBonusAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.WagerMatchBonusLabel), GamingMeters.EgmPaidBonusWagerMatchAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SasLegacyBonusTaxDeductibleLabel), GamingMeters.EgmPaidBonusDeductibleAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SasLegacyBonusNonTaxDeductibleLabel), GamingMeters.EgmPaidBonusNonDeductibleAmount)
        };

        private readonly (string meterLabel, string meterName)[] _handPaidBonusAwards =
        {
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AftCashableBonusLabel), BonusMeters.HandPaidBonusBaseCashableInAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AftNonCashableBonusLabel), GamingMeters.HandPaidBonusNonCashInAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.AftPromotionalBonusLabel), GamingMeters.HandPaidBonusPromoInAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.MjtBonusLabel), BonusMeters.HandPaidMjtBonusAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.WagerMatchBonusLabel), GamingMeters.HandPaidBonusWagerMatchAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SasLegacyBonusTaxDeductibleLabel), GamingMeters.HandPaidBonusDeductibleAmount),
            ( Localizer.For(CultureFor.OperatorTicket).GetString(ResourceKeys.SasLegacyBonusNonTaxDeductibleLabel), GamingMeters.HandPaidBonusNonDeductibleAmount)
        };

        private string _egmPaidBonusAwardsTotalAmountFormatted;
        private string _handPaidBonusAwardsTotalAmountFormatted;

        public BonusMetersPageViewModel()
            : base(null, true)
        {
        }

        public ObservableCollection<ValueDisplayMeter> EgmPaidBonusAwardsMeters { get; } =
            new ObservableCollection<ValueDisplayMeter>();

        public string EgmPaidBonusAwardsTotalAmountFormatted
        {
            get => _egmPaidBonusAwardsTotalAmountFormatted;
            set
            {
                _egmPaidBonusAwardsTotalAmountFormatted = value;
                RaisePropertyChanged(nameof(EgmPaidBonusAwardsTotalAmountFormatted));
            }
        }

        public ObservableCollection<ValueDisplayMeter> HandPaidBonusAwardsMeters { get; } =
            new ObservableCollection<ValueDisplayMeter>();

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

        private void UpdateMeterTotals()
        {
            EgmPaidBonusAwardsTotalAmountFormatted =
                EgmPaidBonusAwardsMeters.Sum(m => m.MeterValue).FormattedCurrencyString();
            
            HandPaidBonusAwardsTotalAmountFormatted =
                HandPaidBonusAwardsMeters.Sum(m => m.MeterValue).FormattedCurrencyString();
        }

        private void RefreshMeters()
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

        private void AddMeters()
        {
            var meterManager = ServiceManager.GetInstance().GetService<IMeterManager>();

            foreach (var egmPaidBonusAward in _egmPaidBonusAwards)
            {
                var value = meterManager.GetMeter(egmPaidBonusAward.meterName);

                var meter = new ValueDisplayMeter(egmPaidBonusAward.meterLabel, value, ShowLifetime);

                EgmPaidBonusAwardsMeters.Add(meter);
            }

            foreach (var handPaidBonusAward in _handPaidBonusAwards)
            {
                var value = meterManager.GetMeter(handPaidBonusAward.meterName);

                var meter = new ValueDisplayMeter(handPaidBonusAward.meterLabel, value, ShowLifetime);

                HandPaidBonusAwardsMeters.Add(meter);
            }
        }

        private void RemoveMeters()
        {
            foreach (var meter in EgmPaidBonusAwardsMeters)
            {
                meter.Dispose();
            }

            EgmPaidBonusAwardsMeters.Clear();

            foreach (var meter in HandPaidBonusAwardsMeters)
            {
                meter.Dispose();
            }

            HandPaidBonusAwardsMeters.Clear();
        }
    }
}