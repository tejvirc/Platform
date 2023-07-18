namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts.Models;
    using System.Collections.Generic;

    public class HoldPercentage : BaseViewModel
    {
        private GameHoldPercentageType? _selectedOption;

        public HoldPercentage(GameType gameType)
        {
            GameType = gameType;

            Options = new List<GameHoldPercentageType>
            {
                GameHoldPercentageType.Off,
                GameHoldPercentageType.Low,
                GameHoldPercentageType.Medium,
                GameHoldPercentageType.High
            };
        }

        public GameType GameType { get; }

        public List<GameHoldPercentageType> Options { get; }

        public GameHoldPercentageType? SelectedOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                RaisePropertyChanged(nameof(SelectedOption));
                RaisePropertyChanged(nameof(IsOffSelected));
                RaisePropertyChanged(nameof(IsLowSelected));
                RaisePropertyChanged(nameof(IsMediumSelected));
                RaisePropertyChanged(nameof(IsHighSelected));
            }
        }

        public bool IsOffSelected
        {
            get => SelectedOption == GameHoldPercentageType.Off;
            set
            {
                if (value)
                {
                    SelectedOption = GameHoldPercentageType.Off;
                }
            }
        }

        public bool IsLowSelected
        {
            get => SelectedOption == GameHoldPercentageType.Low;
            set
            {
                if (value)
                {
                    SelectedOption = GameHoldPercentageType.Low;
                }
            }
        }

        public bool IsMediumSelected
        {
            get => SelectedOption == GameHoldPercentageType.Medium;
            set
            {
                if (value)
                {
                    SelectedOption = GameHoldPercentageType.Medium;
                }
            }
        }

        public bool IsHighSelected
        {
            get => SelectedOption == GameHoldPercentageType.High;
            set
            {
                if (value)
                {
                    SelectedOption = GameHoldPercentageType.High;
                }
            }
        }
    }
}
