namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using Application.Contracts.Localization;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     Class definition for an AlphaNumericTextBox
    /// </summary>
    public class AlphaNumericTextBox : TextBox
    {
        private static readonly IEventBus EventBus;

        static AlphaNumericTextBox()
        {
            EventBus = ServiceManager.GetInstance().GetService<IEventBus>();

            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AlphaNumericTextBox),
                new FrameworkPropertyMetadata(typeof(AlphaNumericTextBox)));
        }

        /// <summary>
        ///     OnApplyTemplate handles the required binding to the text control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Disable copy/paste.
            DataObject.AddCopyingHandler(this, PastingEventHandler);
            DataObject.AddPastingHandler(this, PastingEventHandler);

            CaretIndex = Text.Length;
            GotFocus += TextBox_GotFocus;
            LostFocus += TextBox_LostFocus;
            TextChanged += AlphaNumericTextBox_TextChanged;
            ContextMenu = null;
        }

        private void AlphaNumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                var cursorPosition = tb.SelectionStart;
                tb.Text = Regex.Replace(
                    tb.Text,
                    IsAlphaNumeric ? "[^a-zA-Z0-9]" : AllowNegatives ? "[^\\-0-9]" : "[^0-9]",
                    "");
                tb.CaretIndex = cursorPosition;
                SetErrorText(tb.Text);
            }
        }

        private void SetErrorText(string textBoxValue)
        {
            if (string.IsNullOrEmpty(textBoxValue))
            {
                ErrorText = CanBeEmpty ? string.Empty : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EmptyStringNotAllowErrorMessage);
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardOpenedEvent());
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardClosedEvent());
        }

        #region Dependency Properties

        /// <summary>
        ///     Dependency property for ErrorText.
        /// </summary>
        public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register(
            "ErrorText",
            typeof(string),
            typeof(AlphaNumericTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        ///     ErrorText holds the value of the error text displayed when setting an error on the AlphaNumericTextBox.
        /// </summary>
        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            set => SetValue(ErrorTextProperty, value);
        }

        /// <summary>
        ///     Dependency property for IsAlphaNumeric.  True == Alpha Numeric, False = Numeric.
        /// </summary>
        public static readonly DependencyProperty IsAlphaNumericProperty = DependencyProperty.Register(
            "IsAlphaNumeric",
            typeof(bool),
            typeof(AlphaNumericTextBox),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Indicates whether or not the AlphaNumericTextBox supports alpha numeric input or only numeric input.
        /// </summary>
        public bool IsAlphaNumeric
        {
            get => (bool)GetValue(IsAlphaNumericProperty);
            set => SetValue(IsAlphaNumericProperty, value);
        }

        /// <summary>
        ///     Dependency property for CanBeEmpty.
        /// </summary>
        public static readonly DependencyProperty CanBeEmptyProperty = DependencyProperty.Register(
            "CanBeEmpty",
            typeof(bool),
            typeof(AlphaNumericTextBox),
            new FrameworkPropertyMetadata(true));

        /// <summary>
        ///     Indicates whether or not the AlphaNumericTextBox can be empty 
        /// </summary>
        public bool CanBeEmpty
        {
            get => (bool)GetValue(CanBeEmptyProperty);
            set => SetValue(CanBeEmptyProperty, value);
        }



        /// <summary>
        ///     Dependency Property For the AllowNegatives Property.  True == Allow Negative Numbers
        /// </summary>
        public static readonly DependencyProperty AllowNegativesProperty = DependencyProperty.Register(
            "AllowNegatives",
            typeof(bool),
            typeof(CurrencyTextBox),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Indicates whether or not the AlphaNumericTextBox allows negative numbers 
        /// </summary>
        public bool AllowNegatives
        {
            get => (bool)GetValue(AllowNegativesProperty);
            set => SetValue(AllowNegativesProperty, value);
        }

        #endregion
    }
}
