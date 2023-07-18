namespace Aristocrat.Monaco.Gaming.UI.ViewModels
{
    using System.Globalization;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using CommunityToolkit.Mvvm.ComponentModel;

    public class SubTabInfoViewModel : ObservableObject
    {
        private bool _isSelected;
        private bool _isVisible;

        private const string SubTab = "SubTab";
        private const string Normal = "Normal";
        private const string Selected = "Selected";
        private const string OneHandResourceKey = "OneHand";
        private const string ThreeHandResourceKey = "ThreeHand";
        private const string FiveHandResourceKey = "FiveHand";
        private const string TenHandResourceKey = "TenHand";
        private const string SingleCardResourceKey = "SingleCard";
        private const string FourCardResourceKey = "FourCard";
        private const string MultiCardResourceKey = "MultiCard";
        private const string BlackJackResourceKey = "BlackJack";
        private const string RouletteResourceKey = "Roulette";

        private const string SubTypeOneHand = "Single Hand";
        private const string SubTypeThreeHand = "Three Hand";
        private const string SubTypeFiveHand = "Five Hand";
        private const string SubTypeTenHand = "Ten Hand";
        private const string SubTypeSingleCard = "Single Card";
        private const string SubTypeFourCard = "Four Card";
        private const string SubTypeMultiCard = "Multi Card";
        private const string SubTypeBlackJack = "BlackJack";
        private const string SubTypeRoulette = "Roulette";

        public SubTabInfoViewModel(string typeText)
        {
            GameSubCategory? type = ConvertToSubTab(typeText);
            if (type.HasValue)
            {
                Type = type.Value;
            }
        }

        public SubTabInfoViewModel(GameSubCategory type)
        {
            Type = type;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged(nameof(IsSelected));
                    RaisePropertyChanged(nameof(ResourceKey));
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

        public GameSubCategory Type { get; }

        public string TypeText => GetSubTypeText(Type);

        public string ResourceKey
        {
            get
            {
                string type = string.Empty;
                switch (Type)
                {
                    case GameSubCategory.OneHand:
                        type = OneHandResourceKey;
                        break;
                    case GameSubCategory.ThreeHand:
                        type = ThreeHandResourceKey;
                        break;
                    case GameSubCategory.FiveHand:
                        type = FiveHandResourceKey;
                        break;
                    case GameSubCategory.TenHand:
                        type = TenHandResourceKey;
                        break;
                    case GameSubCategory.SingleCard:
                        type = SingleCardResourceKey;
                        break;
                    case GameSubCategory.FourCard:
                        type = FourCardResourceKey;
                        break;
                    case GameSubCategory.MultiCard:
                        type = MultiCardResourceKey;
                        break;
                    case GameSubCategory.BlackJack:
                        type = BlackJackResourceKey;
                        break;
                    case GameSubCategory.Roulette:
                        type = RouletteResourceKey;
                        break;
                }

                var selected = IsSelected ? Selected : Normal;
                return $"{SubTab}{type}{selected}";
            }
        }

        public static GameSubCategory? ConvertToSubTab(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            GameSubCategory? type = null;

            if (string.Compare(text, SubTypeOneHand, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.OneHand;
            }
            else if (string.Compare(text, SubTypeThreeHand, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.ThreeHand;
            }
            else if (string.Compare(text, SubTypeFiveHand, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.FiveHand;
            }
            else if (string.Compare(text, SubTypeTenHand, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.TenHand;
            }
            else if (string.Compare(text, SubTypeSingleCard, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.SingleCard;
            }
            else if (string.Compare(text, SubTypeFourCard, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.FourCard;
            }
            else if (string.Compare(text, SubTypeMultiCard, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.MultiCard;
            }
            else if (string.Compare(text, SubTypeBlackJack, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.BlackJack;
            }
            else if (string.Compare(text, SubTypeRoulette, true, CultureInfo.InvariantCulture) == 0)
            {
                type = GameSubCategory.Roulette;
            }

            return type;
        }

        static public string GetSubTypeText(GameSubCategory? type)
        {
            if (!type.HasValue)
            {
                return string.Empty;
            }

            switch (type.Value)
            {
                case GameSubCategory.OneHand:
                    return SubTypeOneHand;
                case GameSubCategory.ThreeHand:
                    return SubTypeThreeHand;
                case GameSubCategory.FiveHand:
                    return SubTypeFiveHand;
                case GameSubCategory.TenHand:
                    return SubTypeTenHand;
                case GameSubCategory.SingleCard:
                    return SubTypeSingleCard;
                case GameSubCategory.FourCard:
                    return SubTypeFourCard;
                case GameSubCategory.MultiCard:
                    return SubTypeMultiCard;
                case GameSubCategory.BlackJack:
                    return SubTypeBlackJack;
                case GameSubCategory.Roulette:
                    return SubTypeRoulette;
                default:
                    return string.Empty;
            }
        }
    }
}
