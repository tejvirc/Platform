namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     Class definition for an AlphaNumericTextBox
    /// </summary>
    public class AlphaNumericTextBox : TouchTextBox
    {
        static AlphaNumericTextBox()
        {
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
            TextChanged += AlphaNumericTextBox_TextChanged;
            ContextMenu = null;
        }

        private void AlphaNumericTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                var cursorPosition = tb.SelectionStart;
                if (!SkipRegexCheck)
                {
                    tb.Text = Regex.Replace(
                        tb.Text,
                        IsAlphaNumeric ? "\\W" : AllowNegatives ? "[^\\-0-9]" : "[^0-9]",
                        "");
                }

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
            typeof(AlphaNumericTextBox),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Indicates whether or not the AlphaNumericTextBox allows negative numbers 
        /// </summary>
        public bool AllowNegatives
        {
            get => (bool)GetValue(AllowNegativesProperty);
            set => SetValue(AllowNegativesProperty, value);
        }

        /// <summary>
        ///     Dependency Property For the SkipRegexCheck Property.  True == Will skip Regex check when handling AlphaNumericTextBox_TextChanged
        /// </summary>
        public static readonly DependencyProperty SkipRegexCheckProperty = DependencyProperty.Register(
            "SkipRegexCheck",
            typeof(bool),
            typeof(AlphaNumericTextBox),
            new FrameworkPropertyMetadata(false));

        /// <summary>
        ///     Indicates whether or not the AlphaNumericTextBox skips Regex check when handling AlphaNumericTextBox_TextChanged  
        /// </summary>
        /// <remarks>Intended use to allow the same text input as a TextBox, mainly to utilize the ability to
        /// open/close the on-screen keyboard when the OS is not configured to do so automatically
        /// whenever a TextBox gets/loses focus.</remarks>
        public bool SkipRegexCheck
        {
            get => (bool)GetValue(SkipRegexCheckProperty);
            set => SetValue(SkipRegexCheckProperty, value);
        }

        #endregion
    }
}
