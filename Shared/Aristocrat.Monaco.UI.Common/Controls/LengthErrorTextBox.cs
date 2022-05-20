namespace Aristocrat.Monaco.UI.Common.Controls
{
    using Helpers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Class definition for an LengthErrorTextBox
    /// </summary>
    public class LengthErrorTextBox : TextBox
    {
        private static readonly IEventBus EventBus;

        private int _caretIndex;
        private bool _shiftKeyDown;

        static LengthErrorTextBox()
        {
            EventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LengthErrorTextBox),
                new FrameworkPropertyMetadata(typeof(LengthErrorTextBox)));
        }

        /// <summary>
        ///     OnApplyTemplate handles the required binding to the text control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Bind the text property.
            var textBinding = new Binding
            {
                Path = new PropertyPath("LengthErrorTextBox"),
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(this, TextProperty, textBinding);

            // Disable copy/paste.
            DataObject.AddCopyingHandler(this, PastingEventHandler);
            DataObject.AddPastingHandler(this, PastingEventHandler);

            CaretIndex = _caretIndex = Text.Length;
            PreviewKeyDown += LengthErrorTextBox_PreviewKeyDown;
            PreviewKeyUp += LengthErrorTextBox_PreviewKeyUp;
            PreviewMouseDown += LengthErrorTextBox_PreviewMouseDown;
            PreviewMouseUp += LengthErrorTextBox_PreviewMouseUp;
            GotFocus += TextBox_GotFocus;
            LostFocus += TextBox_LostFocus;
            TextChanged += LengthErrorTextBox_TextChanged;
            ContextMenu = null;
        }

        private void LengthErrorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox tb))
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Left:
                    e.Handled = true;
                    ErrorText = string.Empty;
                    _caretIndex -= (_caretIndex > 0 ? 1 : 0);
                    tb.CaretIndex = _caretIndex;
                    tb.Text = LengthErrorText;
                    tb.Focus();
                    break;
                case Key.Right:
                    e.Handled = true;
                    ErrorText = string.Empty;
                    _caretIndex += (_caretIndex == LengthErrorText.Length ? 0 : 1);
                    tb.CaretIndex = _caretIndex;
                    tb.Text = LengthErrorText;
                    tb.Focus();
                    break;
                case Key.Back:
                    e.Handled = true;
                    ErrorText = string.Empty;
                    if (CaretIndex > 0)
                    {
                        _caretIndex--;
                        LengthErrorText = LengthErrorText.Remove(CaretIndex - 1, 1);
                    }
                    else
                    {
                        _caretIndex = 0;
                        LengthErrorText = string.Empty;
                    }

                    break;
                case Key.Delete:
                    e.Handled = true;
                    ErrorText = string.Empty;
                    if (CaretIndex == LengthErrorText.Length)
                    {
                        _caretIndex = 0;
                        LengthErrorText = string.Empty;
                    }
                    else
                    {
                        LengthErrorText = LengthErrorText.Remove(CaretIndex, 1);
                    }

                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    e.Handled = true;
                    _shiftKeyDown = true;
                    break;
                default:
                    var symbol = GetSymbol(e.Key);
                    if (e.Key.IsNumeric() ||
                        e.Key.IsAlphaNumeric() ||
                        !string.IsNullOrEmpty(symbol))
                    {
                        e.Handled = true;
                        if (MaxTextLength == 0 || LengthErrorText.Length < MaxTextLength)
                        {
                            _caretIndex++;
                            if (ErrorText.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage)))
                            {
                                ErrorText = string.Empty;
                                LengthErrorText = string.IsNullOrEmpty(symbol) ? e.Key.IsNumeric() ? e.Key.GetDigitFromKey().ToString() : e.Key.ToString() : symbol;
                            }
                            else
                            {
                                ErrorText = string.Empty;
                                LengthErrorText = LengthErrorText.Insert(CaretIndex, string.IsNullOrEmpty(symbol) ? e.Key.IsNumeric() ? e.Key.GetDigitFromKey().ToString() : e.Key.ToString() : symbol);
                            }
                        }
                        else
                        {
                            ErrorText = Localizer.For(CultureFor.Operator).FormatString(ResourceKeys.MustBeLessThan, MaxTextLength + 1);
                            tb.Text = LengthErrorText;
                        }
                    }
                    else
                    {
                        e.Handled = false;
                    }

                    break;
            }

            SetErrorText(LengthErrorText);
        }

        private void LengthErrorTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    e.Handled = true;
                    _shiftKeyDown = false;
                    break;
                default:
                    e.Handled = false;
                    break;
            }
        }

        private void LengthErrorTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
            if (sender is TextBox tb)
            {
                _caretIndex = tb.CaretIndex;
                tb.Text = LengthErrorText;
                tb.Focus();
                SetErrorText(tb.Text);
            }
        }

        private void LengthErrorTextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
            if (sender is TextBox tb)
            {
                _caretIndex = tb.CaretIndex;
                tb.Text = LengthErrorText;
                tb.Focus();
                SetErrorText(tb.Text);
            }
        }

        private void LengthErrorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.CaretIndex = _caretIndex < 0 ? 0 : _caretIndex;
                SetErrorText(tb.Text);
            }
        }

        private void SetErrorText(string textBoxValue)
        {
            if (string.IsNullOrEmpty(textBoxValue) && !CanBeEmpty)
            {
                ErrorText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage);
            }
            else if (ErrorText == Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage))
            {
                ErrorText = string.Empty;
            }
        }

        private void PastingEventHandler(object sender, DataObjectEventArgs e)
        {
            // Prevent copy/paste
            e.CancelCommand();
        }

        private string GetSymbol(Key key)
        {
            string symbol = string.Empty;
            switch (key)
            {
                case Key.Oem3:
                    symbol = _shiftKeyDown ? "~" : "`";
                    break;
                case Key.D1:
                    symbol = _shiftKeyDown ? "!" : string.Empty;
                    break;
                case Key.D2:
                    symbol = _shiftKeyDown ? "@" : string.Empty;
                    break;
                case Key.D3:
                    symbol = _shiftKeyDown ? "#" : string.Empty;
                    break;
                case Key.D4:
                    symbol = _shiftKeyDown ? "$" : string.Empty;
                    break;
                case Key.D5:
                    symbol = _shiftKeyDown ? "%" : string.Empty;
                    break;
                case Key.D6:
                    symbol = _shiftKeyDown ? "^" : string.Empty;
                    break;
                case Key.D7:
                    symbol = _shiftKeyDown ? "&" : string.Empty;
                    break;
                case Key.D8:
                    symbol = _shiftKeyDown ? "*" : string.Empty;
                    break;
                case Key.D9:
                    symbol = _shiftKeyDown ? "(" : string.Empty;
                    break;
                case Key.D0:
                    symbol = _shiftKeyDown ? ")" : string.Empty;
                    break;
                case Key.OemMinus:
                    symbol = _shiftKeyDown ? "_" : "-";
                    break;
                case Key.OemPlus:
                    symbol = _shiftKeyDown ? "+" : "=";
                    break;
                case Key.OemOpenBrackets:
                    symbol = _shiftKeyDown ? "{" : "[";
                    break;
                case Key.Oem6:
                    symbol = _shiftKeyDown ? "}" : "]";
                    break;
                case Key.Oem5:
                    symbol = _shiftKeyDown ? "|" : "\\";
                    break;
                case Key.Oem1:
                    symbol = _shiftKeyDown ? ":" : ";";
                    break;
                case Key.OemQuotes:
                    symbol = _shiftKeyDown ? "\"" : "'";
                    break;
                case Key.OemComma:
                    symbol = _shiftKeyDown ? "<" : ",";
                    break;
                case Key.OemPeriod:
                    symbol = _shiftKeyDown ? ">" : ".";
                    break;
                case Key.OemQuestion:
                    symbol = _shiftKeyDown ? "?" : "/";
                    break;
            }

            return symbol;
        }

        #region Dependency Properties

        /// <summary>
        ///     Dependency Property for the value of the LengthErrorText.
        /// </summary>
        public static readonly DependencyProperty LengthErrorTextProperty = DependencyProperty.Register(
            "LengthErrorText",
            typeof(string),
            typeof(LengthErrorTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        /// <summary>
        ///     LengthErrorText holds the value displayed in the LengthErrorTextBox.
        /// </summary>
        public string LengthErrorText
        {
            get => (string)GetValue(LengthErrorTextProperty);
            set => SetValue(LengthErrorTextProperty, value);
        }

        /// <summary>
        ///     Dependency property for ErrorText.
        /// </summary>
        public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register(
            "ErrorText",
            typeof(string),
            typeof(LengthErrorTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     ErrorText holds the value of the error text displayed when setting an error on the LengthErrorTextBox.
        /// </summary>
        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        /// <summary>
        ///     Dependency property for MaxTextLength.
        /// </summary>
        public static readonly DependencyProperty MaxTextLengthProperty = DependencyProperty.Register(
            "MaxTextLength",
            typeof(int),
            typeof(LengthErrorTextBox),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     MaxTextLength holds the value of the maximum number of characters than can be entered into the LengthErrorTextBox.
        /// </summary>
        public int MaxTextLength
        {
            get => (int)GetValue(MaxTextLengthProperty);
            set => SetValue(MaxTextLengthProperty, value);
        }

        /// <summary>
        ///     Dependency property for CanBeEmpty.
        /// </summary>
        public static readonly DependencyProperty CanBeEmptyProperty = DependencyProperty.Register(
            "CanBeEmpty",
            typeof(bool),
            typeof(LengthErrorTextBox),
            new FrameworkPropertyMetadata(true));

        /// <summary>
        ///     Indicates whether or not the AlphaNumericTextBox can be empty 
        /// </summary>
        public bool CanBeEmpty
        {
            get => (bool)GetValue(CanBeEmptyProperty);
            set => SetValue(CanBeEmptyProperty, value);
        }

        private static void OnTextChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var tb = (LengthErrorTextBox)element;
            tb.Text = tb.LengthErrorText;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardOpenedEvent());
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardClosedEvent());
        }

        #endregion
    }
}
