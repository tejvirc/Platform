namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Converters;
    using Helpers;

    /// <inheritdoc />
    public class ExpirationTextBox : TextBox
    {
        private static readonly IEventBus EventBus;

        static ExpirationTextBox()
        {
            EventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ExpirationTextBox),
                new FrameworkPropertyMetadata(typeof(ExpirationTextBox)));
        }

        /// <summary>
        ///     Dependency Property for the Number property for the Expiration Text Box
        /// </summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            "Number",
            typeof(int),
            typeof(ExpirationTextBox),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     Dependency Property for the Never Expires Text Property for the Expiration Text Box
        /// </summary>
        public static readonly DependencyProperty NeverExpiresProperty = DependencyProperty.Register(
            "NeverExpires",
            typeof(string),
            typeof(ExpirationTextBox),
            new FrameworkPropertyMetadata(NeverExpiresPropertyChanged));

        /// <summary>
        ///     Dependency Property for the Days Formatter Text Property for the Expiration Text Box
        /// </summary>
        public static readonly DependencyProperty DaysFormatterProperty = DependencyProperty.Register(
            "DaysFormatter",
            typeof(string),
            typeof(ExpirationTextBox),
            new FrameworkPropertyMetadata(DaysFormatterPropertyChanged));

        /// <summary>
        ///     Gets or sets the expiration days to be displayed in the Expiration Text Box
        /// </summary>
        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        /// <summary>
        ///     Gets or sets the Never Expires Text to be displayed in the Expiration Text Box
        /// </summary>
        public string NeverExpires
        {
            get => (string)GetValue(NeverExpiresProperty);
            set => SetValue(NeverExpiresProperty, value);
        }

        /// <summary>
        ///     Gets or sets the Days Formatter Text to be displayed in the Expiration Text Box
        /// </summary>
        public string DaysFormatter
        {
            get => (string)GetValue(DaysFormatterProperty);
            set => SetValue(DaysFormatterProperty, value);
        }

        /// <summary>
        ///     Gets or sets the maximum number of days before expiration.
        /// </summary>
        public int MaxExpirationDays { get; set; } = 9999;

        /// <summary>
        ///     OnApplyTemplate handles the required binding to the text control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Bind Text to Number with the specified StringFormat
            UpdateTextBinding(this, DaysFormatter, NeverExpires);

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

        private void SetNumber(int number)
        {
            var updatedNumber = Number * 10 + number;
            if (updatedNumber > MaxExpirationDays)
            {
                return;
            }

            Number = Number * 10 + number;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (!int.TryParse(Regex.Replace(e.Text, "[^0-9]", ""), out var number))
            {
                // Ignore the data being provided
                return;
            }

            SetNumber(number);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb.Text.Length > 0)
            {
                string text = Regex.Replace(tb.Text, "[^a-zA-Z0-9 ]", "");
                if (!text.Equals(tb.Text))
                {
                    tb.Text = text;
                }
            }

            tb.CaretIndex = tb.Text.Length;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextBinding((TextBox)sender, DaysFormatter, NeverExpires);
            EventBus?.Publish(new OnscreenKeyboardOpenedEvent());
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateTextBinding((TextBox)sender, DaysFormatter, NeverExpires);
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
            if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
            {
                e.Handled = false;
                return;
            }

            if (e.Key.IsNumeric())
            {
                SetNumber(e.Key.GetDigitFromKey());
            }
            else if (e.Key == Key.Back)
            {
                // Remove the right-most digit
                Number /= 10;
            }
            else if (e.Key == Key.Delete)
            {
                Number = 0;
            }

            e.Handled = !IsIgnoredKey(e.Key);
        }

        private bool IsIgnoredKey(Key key)
        {
            return key == Key.Up ||
                   key == Key.Down ||
                   key == Key.Tab ||
                   key == Key.Enter ||
                   key == Key.LeftShift ||
                   key == Key.RightShift;
        }

        private static void NeverExpiresPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new StringFormat
            var textBox = (ExpirationTextBox)obj;
            UpdateTextBinding(textBox, textBox.DaysFormatter, (string)e.NewValue);
        }

        private static void DaysFormatterPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new StringFormat
            var textBox = (ExpirationTextBox)obj;
            UpdateTextBinding(textBox, (string)e.NewValue, textBox.NeverExpires);
        }

        private static void UpdateTextBinding(TextBox textBox, string daysFormatter, string neverExpiresText)
        {
            var textBinding = new Binding
            {
                Path = new PropertyPath("Number"),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = new ExpirationTextValueConvert(daysFormatter, neverExpiresText, !textBox.IsFocused)
            };

            BindingOperations.SetBinding(textBox, TextProperty, textBinding);
        }
    }
}