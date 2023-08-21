namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Contracts.Localization;
    using Hardware.Contracts.TowerLight;
    using Monaco.Common;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class TowerLight : BaseViewModel
    {
        private FlashState _flashState;
        private bool _state;

        public TowerLight(LightTier tier, FlashState flashState)
        {
            Tier = tier;
            _state = flashState != FlashState.Off;
            _flashState = flashState;
        }

        public LightTier Tier { get; }

        public string Name => Tier.GetDescription();

        public bool State
        {
            get => _state;
            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                RaisePropertyChanged(nameof(State));
            }
        }

        public string FlashStateName => Localizer.For(CultureFor.Operator).GetString(FlashState.ToString());

        public FlashState FlashState
        {
            get => _flashState;
            set
            {
                if (_flashState == value)
                {
                    return;
                }

                _flashState = value;
                RaisePropertyChanged(nameof(FlashState));
                RaisePropertyChanged(nameof(FlashStateName));
            }
        }
    }
}
