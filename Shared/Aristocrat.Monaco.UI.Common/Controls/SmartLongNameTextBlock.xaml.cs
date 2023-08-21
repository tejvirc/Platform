namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Windows;
    using System.Windows.Media;
    using Helpers;

    /// <summary>
    ///     Interaction logic for SmartLongNameTextBlock.xaml, which consists of a TextBlock and an optional
    ///     Button to show (i) if the text doesn't fit.  The (i) button would popup up the full text.
    /// </summary>
    public partial class SmartLongNameTextBlock
    {
        /// <summary>
        ///     Constructor for <see cref="SmartLongNameTextBlock"/>
        /// </summary>
        public SmartLongNameTextBlock()
        {
            InitializeComponent();

            ContentTextBlock.Loaded += ContentTextBlock_Loaded;
            ContentTextBlock.SizeChanged += (sender, args) => Recalculate();
        }

        /// <summary>
        ///     DependencyProperty for binding SmartLongNameTextBlock ContentTextBlock Text
        /// </summary>
        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register(
            "ContentText",
            typeof(string),
            typeof(SmartLongNameTextBlock));

        /// <summary>
        ///     DependencyProperty for binding SmartLongNameTextBlock ContentTextBlock MaxWidth
        /// </summary>
        public static readonly DependencyProperty ContentMaxWidthProperty = DependencyProperty.Register(
            "ContentMaxWidth",
            typeof(double),
            typeof(SmartLongNameTextBlock));

        /// <summary>
        ///     DependencyProperty for binding SmartLongNameTextBlock button Visibility
        /// </summary>
        public static readonly DependencyProperty ButtonVisibilityProperty = DependencyProperty.Register(
            "ButtonVisibility",
            typeof(Visibility),
            typeof(SmartLongNameTextBlock));

        /// <summary>
        ///     ContentTextBlock Text
        /// </summary>
        public string ContentText
        {
            get => (string)GetValue(ContentTextProperty);
            set => SetValue(ContentTextProperty, value);
        }

        /// <summary>
        ///     ContentTextBlock MaxWidth
        /// </summary>
        public double ContentMaxWidth
        {
            get => (double)GetValue(ContentMaxWidthProperty);
            set => SetValue(ContentMaxWidthProperty, value);
        }

        /// <summary>
        ///     InfoButton visibility
        /// </summary>
        public Visibility ButtonVisibility
        {
            get => (Visibility)GetValue(ButtonVisibilityProperty);
            set => SetValue(ButtonVisibilityProperty, value);
        }
        
        private void Recalculate()
        {
            var measuredTextWidth = TextMeasurementHelper.MeasureTextWidth(
                ContentTextBlock.Text,
                ContentTextBlock.FontFamily,
                ContentTextBlock.FontStyle,
                ContentTextBlock.FontWeight,
                ContentTextBlock.FontStretch,
                ContentTextBlock.FontSize,
                VisualTreeHelper.GetDpi(ContentTextBlock).PixelsPerDip);

            ButtonVisibility = measuredTextWidth > ContentTextBlock.ActualWidth ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ContentTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            Recalculate();
        }
    }
}
