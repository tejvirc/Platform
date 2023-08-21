namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System.Windows;

    public partial class PreviousResultHorseNumber
    {
        public static readonly DependencyProperty HorseNumberProperty =
            DependencyProperty.Register(
                nameof(HorseNumber),
                typeof(int),
                typeof(PreviousResultHorseNumber),
                new PropertyMetadata(3));

        public static readonly DependencyProperty IsCorrectPickProperty =
            DependencyProperty.Register(
                nameof(IsCorrectPick),
                typeof(bool),
                typeof(PreviousResultHorseNumber),
                new PropertyMetadata(false));

        public PreviousResultHorseNumber()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Horse number value
        /// </summary>
        public int HorseNumber
        {
            get => (int)GetValue(HorseNumberProperty);
            set => SetValue(HorseNumberProperty, value);
        }

        /// <summary>
        ///     True is this horses's finish position was chosen correctly
        /// </summary>
        public bool IsCorrectPick
        {
            get => (bool)GetValue(IsCorrectPickProperty);
            set => SetValue(IsCorrectPickProperty, value);
        }
    }
}