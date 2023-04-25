namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Application.Contracts.Extensions;
    using MVVM.Model;

    /// <summary>
    ///     The View Model used for binding lobby denom buttons
    /// </summary>
    public class DenominationInfoViewModel : BaseNotify
    {
        private bool _isSelected;
        private const string DenominationBackOff = "DenominationBackOff";
        private const string DenominationBackOn = "DenominationBackOn";
        private const string DenominationBackOff2 = "DenominationBackOff2";
        private const string DenominationBackOn2 = "DenominationBackOn2";

        public DenominationInfoViewModel(long denomination)
        {
            Denomination = denomination;
            DenomText = Denomination.MillicentsToCents().FormattedDenomString();
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
        public string DenomButtonSharedKey => IsSelected ? DenominationBackOn : DenominationBackOff;

        /// <summary>
        ///     Resource key for single game denom buttons
        /// </summary>
        public string DenomButtonSingleKey => IsSelected ? DenominationBackOn2 : DenominationBackOff2;

        /// <summary>
        ///     Text to display on the denom button
        /// </summary>
        public string DenomText { get; set; }
    }
}
