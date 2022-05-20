namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Converters;
    using Helpers;

    /// <inheritdoc />
    public class PercentageTextBox : TextBox
    {
        private static readonly IEventBus EventBus;

        static PercentageTextBox()
        {
            EventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PercentageTextBox),
                new FrameworkPropertyMetadata(typeof(PercentageTextBox)));
        }

        /// <summary>
        ///     The <see cref="DependencyProperty"/> for <see cref="Number"/>
        /// </summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(decimal),
            typeof(PercentageTextBox),
            new FrameworkPropertyMetadata(0.0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     The <see cref="DependencyProperty"/> for <see cref="ZeroPercentage"/>
        /// </summary>
        public static readonly DependencyProperty ZeroPercentageProperty = DependencyProperty.Register(
            nameof(ZeroPercentage),
            typeof(string),
            typeof(PercentageTextBox),
            new FrameworkPropertyMetadata(ZeroPercentagePropertyChanged));

        /// <summary>
        ///     The <see cref="DependencyProperty"/> for <see cref="EditingFormatter"/>
        /// </summary>
        public static readonly DependencyProperty EditingFormatterProperty = DependencyProperty.Register(
            nameof(EditingFormatter),
            typeof(string),
            typeof(PercentageTextBox),
            new FrameworkPropertyMetadata(EditingFormatterPropertyChanged));

        /// <summary>
        ///     The <see cref="DependencyProperty"/> for <see cref="DisplayFormatter"/>
        /// </summary>
        public static readonly DependencyProperty DisplayFormatterProperty = DependencyProperty.Register(
            nameof(DisplayFormatter),
            typeof(string),
            typeof(PercentageTextBox),
            new FrameworkPropertyMetadata(DisplayFormatterPropertyChanged));

        /// <summary>
        ///     The <see cref="DependencyProperty"/> for <see cref="PreventNegatives"/>. True == Prevent Negative Numbers
        /// </summary>
        public static readonly DependencyProperty PreventNegativesProperty = DependencyProperty.Register(
            nameof(PreventNegatives),
            typeof(bool),
            typeof(PercentageTextBox),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        ///     The <see cref="DependencyProperty"/> for <see cref="MaximumValue"/>
        /// </summary>
        public static readonly DependencyProperty MaximumValueProperty = DependencyProperty.Register(
            nameof(MaximumValue),
            typeof(decimal),
            typeof(PercentageTextBox),
            new FrameworkPropertyMetadata(9999.99M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     The maximum value for this control
        /// </summary>
        public decimal MaximumValue
        {
            get => (decimal)GetValue(MaximumValueProperty);
            set => SetValue(MaximumValueProperty, value);
        }

        /// <summary>
        /// </summary>
        public bool PreventNegatives
        {
            get => (bool)GetValue(PreventNegativesProperty);
            set => SetValue(PreventNegativesProperty, value);
        }
        /// <summary>
        ///     Gets or sets the percentage to be displayed in the Percentage Text Box
        /// </summary>
        public decimal Number
        {
            get => (decimal)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        /// <summary>
        ///     Gets or sets the Zero percentage Text to be displayed in the Percentage Text Box
        /// </summary>
        public string ZeroPercentage
        {
            get => (string)GetValue(ZeroPercentageProperty);
            set => SetValue(ZeroPercentageProperty, value);
        }

        /// <summary>
        ///     Gets or sets the Percentage Formatter Text to be displayed in the Percentage Text Box
        /// </summary>
        public string EditingFormatter
        {
            get => (string)GetValue(EditingFormatterProperty);
            set => SetValue(EditingFormatterProperty, value);
        }

        /// <summary>
        ///     Gets or sets the Percentage Formatter Text to be displayed in the Percentage Text Box
        /// </summary>
        public string DisplayFormatter
        {
            get => (string)GetValue(DisplayFormatterProperty);
            set => SetValue(DisplayFormatterProperty, value);
        }

        /// <summary>
        ///     OnApplyTemplate handles the required binding to the text control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Bind Text to Number with the specified StringFormat
            UpdateTextBinding(this, EditingFormatter, DisplayFormatter, ZeroPercentage);

            // Disable copy/paste
            DataObject.AddCopyingHandler(this, PastingEventHandler);
            DataObject.AddPastingHandler(this, PastingEventHandler);

            CaretIndex = Text.Length;
            PreviewMouseDown += TextBox_PreviewMouseDown;
            PreviewMouseUp += TextBox_PreviewMouseUp;
            TextChanged += TextBox_TextChanged;
            GotFocus += TextBox_GotFocus;
            LostFocus += TextBox_LostFocus;
            PreviewTextInput += TextBox_PreviewTextInput;
            PreviewKeyDown += TextBox_PreviewKeyDown;
            var inputScope = new InputScope();
            inputScope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            InputScope = inputScope;

            ContextMenu = null;
        }

        private void SetNumber(decimal number)
        {
            var result = Number < 0
                ? Number * 10M - number / 100M
                : Number * 10M + number / 100M;

            if (Math.Abs(result) <= MaximumValue)
            {
                Number = result;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (!decimal.TryParse(e.Text, out var number))
            {
                // Ignore the data being provided
                return;
            }

            SetNumber(number);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            tb.CaretIndex = tb.Text.Length;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextBinding((TextBox)sender, EditingFormatter, DisplayFormatter, ZeroPercentage);
            EventBus?.Publish(new OnscreenKeyboardOpenedEvent());
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextBinding((TextBox)sender, EditingFormatter, DisplayFormatter, ZeroPercentage);
            EventBus?.Publish(new OnscreenKeyboardClosedEvent());
        }

        private static void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            ((TextBox)sender).Focus();
        }

        private static void TextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            ((TextBox)sender).Focus();
        }

        private static void PastingEventHandler(object sender, DataObjectEventArgs e)
        {
            // Prevent copy/paste
            e.CancelCommand();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.IsNumeric())
            {
                if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
                {
                    e.Handled = false;
                    return;
                }

                SetNumber(e.Key.GetDigitFromKey());
            }
            else if (e.Key == Key.Back)
            {
                // Remove the right-most digit
                Number = (Number - Number % 0.1M) / 10M;
            }
            else if (e.Key == Key.Delete)
            {
                Number = 0M;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                if (!PreventNegatives)
                {
                    Number *= -1;
                }
            }

            e.Handled = !IsIgnoredKey(e.Key);
        }

        private static bool IsIgnoredKey(Key key)
        {
            return key == Key.Up ||
                   key == Key.Down ||
                   key == Key.Tab ||
                   key == Key.Enter ||
                   key == Key.LeftShift ||
                   key == Key.RightShift;
        }

        private static void ZeroPercentagePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new ZeroPercentage
            var textBox = (PercentageTextBox)obj;
            UpdateTextBinding(textBox, textBox.EditingFormatter, textBox.DisplayFormatter, (string)e.NewValue);
        }

        private static void EditingFormatterPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new EditingFormat
            var textBox = (PercentageTextBox)obj;
            UpdateTextBinding(textBox, (string)e.NewValue, textBox.DisplayFormatter, textBox.ZeroPercentage);
        }

        private static void DisplayFormatterPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new DisplayFormat
            var textBox = (PercentageTextBox)obj;
            UpdateTextBinding(textBox, textBox.EditingFormatter, (string)e.NewValue, textBox.ZeroPercentage);
        }

        private static void UpdateTextBinding(TextBox textBox, string editingFormatter, string displayFormatter, string zeroPercentageText)
        {
            var textBinding = new Binding
            {
                Path = new PropertyPath(nameof(Number)),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new PercentageTextValueConvert(editingFormatter, displayFormatter, zeroPercentageText, !textBox.IsFocused)
            };

            BindingOperations.SetBinding(textBox, TextProperty, textBinding);
        }
    }
}