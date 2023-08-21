namespace Aristocrat.Monaco.Bingo.UI.Models
{
    using System;
    using System.Windows.Media;
    using Common.GameOverlay;
    using MVVM.Model;

    /// <summary>
    ///     Bingo number
    /// </summary>
    public class BingoNumberModel : BaseNotify
    {
        private const string TransparentColor = "Transparent";

        private readonly BingoAppearance _appearance;
        private double _fontSize;

        private int _number = -1;
        private BingoNumberState _state = BingoNumberState.Undefined;
        private SolidColorBrush _foreColor = new(Colors.Transparent);
        private bool _daubed;

        public BingoNumberModel(BingoAppearance appearance, double fontSize)
        {
            _appearance = appearance ?? throw new ArgumentNullException(nameof(appearance));
            _fontSize = fontSize;
        }

        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        public SolidColorBrush ForeColor
        {
            get => _foreColor;
            set => SetProperty(ref _foreColor, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (!(Math.Abs(_fontSize - value) > 0.1f))
                {
                    return;
                }

                _fontSize = value;
                RaisePropertyChanged(nameof(FontSize));
            }
        }

        public BingoNumberState State
        {
            get => _state;
            set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;

                PickColors();

                FontSize = _fontSize;
            }
        }

        public bool Daubed
        {
            get => _daubed;
            set
            {
                if (_daubed == value)
                {
                    return;
                }

                _daubed = value;

                PickColors();
            }
        }

#if !(RETAIL)
        // ReSharper disable UnusedAutoPropertyAccessor.Global Used by Automation API
        public string ForeColorForAutomation { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global Used by Automation API
#endif

        private void PickColors()
        {
            var foreColor = _state switch
            {
                BingoNumberState.CardInitial => _appearance.CardInitialNumberColor,
                BingoNumberState.CardCovered => _daubed
                    ? _appearance.DaubNumberColor
                    : _appearance.CardInitialNumberColor,
                BingoNumberState.BallCallInitial => _daubed
                    ? _appearance.DaubNumberColor
                    : _appearance.BallsEarlyNumberColor,
                BingoNumberState.BallCallLate => _daubed
                    ? _appearance.DaubNumberColor
                    : _appearance.BallsLateNumberColor,
                BingoNumberState.HelpPattern => TransparentColor,
                BingoNumberState.Undefined => TransparentColor,
                _ => TransparentColor
            };

#if !(RETAIL)
            ForeColorForAutomation = foreColor;
#endif

            ForeColor = new SolidColorBrush((Color)(ColorConverter.ConvertFromString(foreColor) ?? Colors.Transparent));
        }
    }
}
