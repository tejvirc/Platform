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
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using Contracts;
    using Contracts.Bonus;
    using Kernel;
    using Localization.Properties;

    [CLSCompliant(false)]
    public class BonusMetersPageViewModel : MetersPageViewModelBase
    {
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
                OnPropertyChanged(nameof(EgmPaidBonusAwardsTotalAmountFormatted));
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
                OnPropertyChanged(nameof(HandPaidBonusAwardsTotalAmountFormatted));
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
            Execute.OnUIThread(
                () =>
                {
                    RemoveMeters();
                    AddMeters();
                    OnPropertyChanged(nameof(EgmPaidBonusAwardsMeters));
                    OnPropertyChanged(nameof(HandPaidBonusAwardsMeters));
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

                var meter = new ValueDisplayMeter(egmPaidBonusAward.meterLabel, value, ShowLifetime, 0, UseOperatorCultureForCurrencyFormatting);

                EgmPaidBonusAwardsMeters.Add(meter);
            }

            foreach (var handPaidBonusAward in GetHandPaidBonusAwardNames())
            {
                var value = meterManager.GetMeter(handPaidBonusAward.meterName);

                var meter = new ValueDisplayMeter(handPaidBonusAward.meterLabel, value, ShowLifetime, 0, UseOperatorCultureForCurrencyFormatting);

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
