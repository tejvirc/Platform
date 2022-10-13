namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Application.Contracts.Extensions;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Helpers;

    /// <summary>
    /// </summary>
    public class CurrencyTextBox : TextBox
    {
        private static readonly IEventBus EventBus;

        /// <summary>
        ///     Dependency Property for the Number value of the Currency Text Box
        /// </summary>
        public static readonly DependencyProperty WholeCurrencyProperty = DependencyProperty.Register(
            nameof(WholeCurrency),
            typeof(bool),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Dependency Property for the Number value of the Currency Text Box
        /// </summary>
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number),
            typeof(decimal),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(0M, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnNumberChanged));

        /// <summary>
        ///     Dependency Property for the Formatted Number value of the Currency Text Box
        /// </summary>
        public static readonly DependencyProperty FormattedNumberProperty = DependencyProperty.Register(
            nameof(FormattedNumber),
            typeof(string),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        ///     Dependency Property for the StringFormat for the Currency Text Box
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
            nameof(StringFormat),
            typeof(string),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata("C", StringFormatPropertyChanged));

        /// <summary>
        ///     Dependency Property For the PreventNegatives Property.  True == Prevent Negative Numbers
        /// </summary>
        public static readonly DependencyProperty PreventNegativesProperty = DependencyProperty.Register(
            nameof(PreventNegatives),
            typeof(bool),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(false));

        static CurrencyTextBox()
        {
            EventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CurrencyTextBox),
                new FrameworkPropertyMetadata(typeof(CurrencyTextBox)));
        }

        /// <summary>
        ///     Constructor adds an event handler for Loaded so we can re-render the currency
        ///     and amount each time the control is displayed.
        /// </summary>
        public CurrencyTextBox()
        {
            Loaded += (_, _) =>
            {
                UpdateFormattedNumber();
            };
        }

        /// <summary>
        ///     Number holds the value displayed in the Currency Text Box
        /// </summary>
        public decimal Number
        {
            get => (decimal)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        /// <summary>
        ///     indicates if it's whole currency
        /// </summary>
        public bool WholeCurrency
        {
            get => (bool)GetValue(WholeCurrencyProperty);
            set => SetValue(WholeCurrencyProperty, value);
        }

        /// <summary>
        ///     Number holds the value displayed in the Currency Text Box
        /// </summary>
        public string FormattedNumber
        {
            get => (string)GetValue(FormattedNumberProperty);
            set => SetValue(FormattedNumberProperty, value);
        }

        /// <summary>
        ///     StringFormat for the Currency Text Box
        /// </summary>
        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }

        /// <summary>
        ///     Prevents Negative values from being set
        /// </summary>
        public bool PreventNegatives
        {
            get => (bool)GetValue(PreventNegativesProperty);
            set => SetValue(PreventNegativesProperty, value);
        }

        /// <summary>
        ///     OnApplyTemplate handles the required binding to the text control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Bind Text to Number with the specified StringFormat
            var textBinding = new Binding
            {
                Path = new PropertyPath(nameof(FormattedNumber)),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(this, TextProperty, textBinding);

            // Disable copy/paste
            DataObject.AddCopyingHandler(this, PastingEventHandler);
            DataObject.AddPastingHandler(this, PastingEventHandler);

            MaxLength = 9;
            CaretIndex = Text.Length;
            PreviewKeyDown += TextBox_PreviewKeyDown;
            PreviewMouseDown += TextBox_PreviewMouseDown;
            PreviewMouseUp += TextBox_PreviewMouseUp;
            GotFocus += TextBox_GotFocus;
            LostFocus += TextBox_LostFocus;
            TextChanged += TextBox_TextChanged;
            PreviewTextInput += CurrencyTextBox_PreviewTextInput;
            ContextMenu = null;

            var inputScope = new InputScope();
            inputScope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            InputScope = inputScope;
        }

        /// <summary>
        /// </summary>
        public void UpdateFormattedNumber()
        {
            FormattedNumber = Number.FormattedCurrencyString();
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

        private static void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            (sender as TextBox)?.Focus();
        }

        private static void TextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Prevent changing the caret index
            e.Handled = true;
            (sender as TextBox)?.Focus();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardOpenedEvent());
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardClosedEvent());
        }

        private static void OnNumberChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var ctb = (CurrencyTextBox)element;
            ctb.UpdateFormattedNumber();
        }

        private static void StringFormatPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            // Update the Text binding with the new StringFormat
            var textBinding = new Binding
            {
                Path = new PropertyPath(nameof(Number)),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                StringFormat = (string)e.NewValue
            };

            BindingOperations.SetBinding(obj, TextProperty, textBinding);
        }

        private void SetNumber(decimal number)
        {
            // VLT-6626 : set the max length of the text box to prevent crashes
            if (Number.ToString("N2").Length > 20)
            {
                return;
            }

            // Push the new number from the right
            int denom = WholeCurrency ? 1 : (int)CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit;

            if (Number < 0)
            {
                Number = Number * 10M - number / denom;
            }
            else
            {
                Number = Number * 10M + number / denom;
            }
        }

        // VLT-6757 : when caps lock is on the virtual keypad does not hit
        // the TextBox_PreviewKeyDown event it instead comes through the PreviewTextInput
        private void CurrencyTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (int.TryParse(e.Text, out var number))
            {
                SetNumber(number);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                return;
            }

            if (Number < 0 && tb.GetBindingExpression(TextProperty)?.ParentBinding.StringFormat == "C")
            {
                // If a negative number and a StringFormat of "C" is used, then
                // place the caret before the closing paren.
                tb.CaretIndex = tb.Text.Length - 1;
            }
            else
            {
                // Keep the caret at the end
                tb.CaretIndex = tb.Text.Length;
            }
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers != ModifierKeys.None)
            {
                e.Handled = false;
            }
            else if (e.Key.IsNumeric())
            {
                e.Handled = true;
                SetNumber(e.Key.GetDigitFromKey());
            }
            else if (e.Key == Key.Back)
            {
                e.Handled = true;

                // Remove the right-most digit
                decimal denom = WholeCurrency ? 10 : 1 / CurrencyExtensions.CurrencyMinorUnitsPerMajorUnit * 10;

                Number = (Number - Number % denom) / 10M;
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;

                Number = 0M;
            }
            else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                e.Handled = true;
                if (!PreventNegatives)
                {
                    Number *= -1;
                }
            }
            else if (IsIgnoredKey(e.Key))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void PastingEventHandler(object sender, DataObjectEventArgs e)
        {
            // Prevent copy/paste
            e.CancelCommand();
        }
    }
}