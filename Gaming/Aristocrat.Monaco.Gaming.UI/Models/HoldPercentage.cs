namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Contracts.Models;
    using System.Collections.Generic;
    using CommunityToolkit.Mvvm.ComponentModel;

    public class HoldPercentage : ObservableObject
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
                OnPropertyChanged(nameof(SelectedOption));
                OnPropertyChanged(nameof(IsOffSelected));
                OnPropertyChanged(nameof(IsLowSelected));
                OnPropertyChanged(nameof(IsMediumSelected));
                OnPropertyChanged(nameof(IsHighSelected));
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