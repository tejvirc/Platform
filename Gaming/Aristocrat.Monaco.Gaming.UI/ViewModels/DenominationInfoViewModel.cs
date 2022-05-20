namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using Application.Contracts.Extensions;
    using MVVM.Model;

    public class DenominationInfoViewModel : BaseNotify
    {
        private bool _isSelected;
        private bool _isVisible;
        private const string DenominationBackOff = "DenominationBackOff";
        private const string DenominationBackOn = "DenominationBackOn";
        private const string DenominationBackOff2 = "DenominationBackOff2";
        private const string DenominationBackOn2 = "DenominationBackOn2";

        public DenominationInfoViewModel(long denomination)
        {
            Denomination = denomination;
            DenomText = Denomination.MillicentsToCents().FormattedDenomString();
        }

        public long Denomination { get; }

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
                    RaisePropertyChanged(nameof(DenomBackground));
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    RaisePropertyChanged(nameof(IsVisible));
                }
            }
        }

        public string DenomBackground => IsSelected ? DenominationBackOn : DenominationBackOff;

        public string DenomBackground2 => IsSelected ? DenominationBackOn2 : DenominationBackOff2;

        public string DenomText { get; set; }
    }
}
