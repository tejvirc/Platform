namespace Aristocrat.Monaco.Application.UI.Models
{
    using Hardware.Contracts.TowerLight;
    using Monaco.UI.Common.Extensions;
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;

    [CLSCompliant(false)]
    public class TowerLight : ObservableObject
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

        public string Name => Tier.GetDescription(typeof(LightTier));

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
                OnPropertyChanged(nameof(State));
            }
        }

        public string FlashStateName => FlashState.GetDescription(typeof(FlashState));

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
                OnPropertyChanged(nameof(FlashState));
                OnPropertyChanged(nameof(FlashStateName));
            }
        }
    }
}
