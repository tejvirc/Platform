namespace Aristocrat.Monaco.Gaming.Lobby.Controls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Shapes;

    /// <summary>
    ///     This class generates a Geometry from a block of text in a specific font, weight, etc.
    ///     and renders it to WPF as a shape.
    /// </summary>
    public class TextPath : Shape
    {
        /// <summary>
        ///     FontFamilyProperty
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
            typeof(TextPath),
            new FrameworkPropertyMetadata(
                SystemFonts.MessageFontFamily,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.Inherits,
                OnPropertyChanged));

        /// <summary>
        ///     FontSizeProperty
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
            typeof(TextPath),
            new FrameworkPropertyMetadata(
                SystemFonts.MessageFontSize,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnPropertyChanged));

        /// <summary>
        ///     FontStretchProperty
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
            typeof(TextPath),
            new FrameworkPropertyMetadata(
                TextElement.FontStretchProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.Inherits,
                OnPropertyChanged));

        /// <summary>
        ///     FontStyleProperty
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
            typeof(TextPath),
            new FrameworkPropertyMetadata(
                SystemFonts.MessageFontStyle,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.Inherits,
                OnPropertyChanged));

        /// <summary>
        ///     FontWeightProperty
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
            typeof(TextPath),
            new FrameworkPropertyMetadata(
                SystemFonts.MessageFontWeight,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.Inherits,
                OnPropertyChanged));

        /// <summary>
        ///     OriginPointProperty
        /// </summary>
        public static readonly DependencyProperty OriginPointProperty = DependencyProperty.Register(
            nameof(Origin),
            typeof(Point),
            typeof(TextPath),
            new FrameworkPropertyMetadata(
                new Point(0, 0),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnPropertyChanged));

        /// <summary>
        ///     TextProperty
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(TextPath),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnPropertyChanged));

        private Geometry? _textGeometry;

        /// <summary>
        ///     Gets or sets the FontFamily.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [Localizability(LocalizationCategory.Font)]
        [TypeConverter(typeof(FontFamilyConverter))]
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        /// <summary>
        ///     Gets or sets the FontSize.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(FontSizeConverter))]
        [Localizability(LocalizationCategory.None)]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        /// <summary>
        ///     Gets or sets the FontStretch.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(FontStretchConverter))]
        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        /// <summary>
        ///     Gets or sets the FontStyle.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(FontStyleConverter))]
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        /// <summary>
        ///     Gets or sets the FontWeight.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(FontWeightConverter))]
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        /// <summary>
        ///     Gets or sets the Origin.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [TypeConverter(typeof(PointConverter))]
        public Point Origin
        {
            get => (Point)GetValue(OriginPointProperty);
            set => SetValue(OriginPointProperty, value);
        }

        /// <summary>
        ///     Gets or sets the Text.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        ///     Gets the DefiningGeometry.
        /// </summary>
        protected override Geometry DefiningGeometry => _textGeometry ?? Geometry.Empty;

        /// <summary>
        ///     MeasureOverride
        /// </summary>
        /// <param name="availableSize">availableSize</param>
        /// <returns>size</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_textGeometry == null)
            {
                CreateTextGeometry();
            }

            Debug.Assert(_textGeometry != null);
            if (_textGeometry.Bounds == Rect.Empty)
            {
                return new Size(0, 0);
            }

            return new Size(
                Math.Min(availableSize.Width, _textGeometry.Bounds.Width),
                Math.Min(availableSize.Height, _textGeometry.Bounds.Height));
        }

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextPath)d).CreateTextGeometry();
        }

        private void CreateTextGeometry()
        {
            var formattedText = new FormattedText(
                Text,
                Thread.CurrentThread.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                96.0);
            if (MaxWidth > 0 && !double.IsInfinity(MaxWidth))
            {
                formattedText.MaxTextWidth = MaxWidth;
            }

            _textGeometry = formattedText.BuildGeometry(Origin);

            InvalidateVisual();
        }
    }
}
