namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Application.Contracts.Extensions;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     The View Model used for binding lobby denom buttons
    /// </summary>
    public class DenominationInfoViewModel : ObservableObject
    {
        private bool _isSelected;

        // Multiple game denomination buttons (shared between all games on page)
        private const string DenomButtonMultipleOff = "DenominationBackOff";
        private const string DenomButtonMultipleOn = "DenominationBackOn";

        // Single game denomination buttons (per individual game)
        private const string DenomButtonSingleOff = "DenominationBackOff2";
        private const string DenomButtonSingleOn = "DenominationBackOn2";

        private string _denomButtonSingleOffOverride;

        public DenominationInfoViewModel(long denomination)
        {
            Denomination = denomination;
            var cents = denomination.MillicentsToCents();

            if (cents >= CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit)
            {
                // Use the denom string formatted with major unit symbol included
                DenomText = cents.FormattedDenomString();
                DenomSymbol = string.Empty;
            }
            else
            {
                // Keep the minor unit separate from the cents value to be displayed in a separate TextPath
                DenomText = cents.ToString();
                DenomSymbol = CurrencyExtensions.MinorUnitSymbol;
            }
        }

        /// <summary>
        ///     Gets the denomination in millicents
        /// </summary>
        public long Denomination { get; }

        /// <summary>
        ///     Gets or sets whether the denom button is currently selected
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                    RaisePropertyChanged(nameof(DenomText));
                    RaisePropertyChanged(nameof(DenomButtonSharedKey));
                    RaisePropertyChanged(nameof(DenomButtonSingleKey));
                }
            }
        }

        /// <summary>
        ///     Resource key for shared game denom buttons
        /// </summary>
        public string DenomButtonSharedKey => IsSelected ? DenomButtonMultipleOn : DenomButtonMultipleOff;

        /// <summary>
        ///     Resource key for individual game denom buttons
        /// </summary>
        public string DenomButtonSingleKey
        {
            get
            {
                var key = IsSelected
                    ? DenomButtonSingleOn
                    : DenomButtonSingleOffOverride ?? DenomButtonSingleOff;
                return key;
            }
        }

        /// <summary>
        ///     Custom denom button resource key assigned per individual game
        /// </summary>
        public string DenomButtonSingleOffOverride
        {
            get => _denomButtonSingleOffOverride;
            set => SetProperty(ref _denomButtonSingleOffOverride, value, nameof(DenomButtonSingleKey));
        }

        /// <summary>
        ///     Text to display on the denom button
        /// </summary>
        public string DenomText { get; set; }

        public string DenomSymbol { get; }
    }
}
