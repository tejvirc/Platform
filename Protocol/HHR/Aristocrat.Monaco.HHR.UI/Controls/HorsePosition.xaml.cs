namespace Aristocrat.Monaco.Hhr.UI.Controls
{
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for HorsePosition.xaml
    /// </summary>
    public partial class HorsePosition
    {
        public HorsePosition()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The value for the horse's number
        /// </summary>
        public int HorseNumber
        {
            get => (int)GetValue(HorseNumberProperty);
            set => SetValue(HorseNumberProperty, value);
        }

        /// <summary>
        /// Dependency property for HorseNumbers
        /// </summary>
        public static readonly DependencyProperty HorseNumberProperty =
            DependencyProperty.Register(
                "HorseNumber",
                typeof(int),
                typeof(HorsePosition),
                new PropertyMetadata(6));

        /// <summary>
        /// The value for the horse's position
        /// </summary>
        public int Position
        {
            get => (int)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        /// <summary>
        /// Dependency property for Position
        /// </summary>
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(
                "Position",
                typeof(int),
                typeof(HorsePosition),
                new PropertyMetadata());

        public Visibility HorseNumberVisible
        {
            get => (Visibility)GetValue(HorseNumberVisibleProperty);
            set => SetValue(HorseNumberVisibleProperty, value);
        }

        // Using a DependencyProperty as the backing store for HorseNumberVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorseNumberVisibleProperty =
            DependencyProperty.Register(
                nameof(HorseNumberVisible),
                typeof(Visibility),
                typeof(HorsePosition),
                new PropertyMetadata(Visibility.Visible));

        /// <summary>
        ///     The handler to invoke when control is clicked
        /// </summary>
        public ICommand OnClickHandler
        {
            get => (ICommand)GetValue(OnClickHandlerProperty);
            set => SetValue(OnClickHandlerProperty, value);
        }

        // Using a DependencyProperty as the backing store for OnClickHandlerProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OnClickHandlerProperty =
            DependencyProperty.Register(
                nameof(OnClickHandler),
                typeof(ICommand),
                typeof(HorsePosition));
    }
}
