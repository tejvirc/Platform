namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Input;
    using ViewModels;

    [CLSCompliant(false)]
    public sealed partial class SoftwareVerificationPage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SoftwareVerificationPage" /> class.
        /// </summary>
        public SoftwareVerificationPage()
        {
            InitializeComponent();

            Loaded += SoftwareVerificationPage_Loaded;

            // Enforce overwrite mode, not insert mode.
            PropertyInfo textEditorProperty = typeof(TextBox).GetProperty(
                "TextEditor", BindingFlags.NonPublic | BindingFlags.Instance);

            var textEditor = textEditorProperty?.GetValue(HmacTextBox, null);

            // set internal property on the TextEditor
            var overtypeModeProperty = textEditor?.GetType().GetProperty(
                "_OvertypeMode", BindingFlags.NonPublic | BindingFlags.Instance);

            overtypeModeProperty?.SetValue(textEditor, true, null);
        }

        private void SoftwareVerificationPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.Reset();
        }

        public SoftwareVerificationPageViewModel ViewModel
        {
            get => DataContext as SoftwareVerificationPageViewModel;
            set => DataContext = value;
        }

        private void HmacTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                    var caret = HmacTextBox.CaretIndex;

                caret--;

                if (caret % 5 == 4)  // if in a space move back to precedent
                {
                    HmacTextBox.CaretIndex = caret;
                    e.Handled = true;
                    return;
                }

                if (caret < 0)
                {
                    e.Handled = true;
                    return;
                }
                var text = HmacTextBox.Text;
                var newText= text.ReplaceAt(caret, '0').Replace(" ", "");
                ViewModel.FormattedHmacKey = newText;

                HmacTextBox.CaretIndex = caret;

                e.Handled = true;
            }
            // prevent all keys that aren't 0-9, A-F Left/Right Arrow Keys
            else if (!ViewModel.ValidateHmacKey(e.Key) && e.Key != Key.Left && e.Key != Key.Right)
            {
                e.Handled = true;
            }
        }

        // We have to use PreviewTextInput because of the virtual keyboard
        private void HmacTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // The hash has one space for every 4 hex character. So add one space to the length for each 4.
            int hashLength = ViewModel.SelectedAlgorithmType.HexHashLength + (ViewModel.SelectedAlgorithmType.HexHashLength / 4 - 1);

            // No editing while caret is at end of field
            if (HmacTextBox.CaretIndex >= hashLength)
            {
                e.Handled = true;
                return;
            }

            // see if character is valid
            bool valid = false;

            if (e.Text.Length == 1)
            {
                int character = e.Text.ToUpper()[0];

                if (character >= 'A' && character <= 'F' || character >= '0' && character <= '9')
                {
                    valid = true;
                }
            }

            if (!valid)
            {
                e.Handled = true;
                return;
            }

            // don't allow editing in the blanks
            if (HmacTextBox.CaretIndex % 5 == 4)
            {
                HmacTextBox.CaretIndex++;
            }
        }

        private void HandleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (e.Command == ApplicationCommands.Paste) // e.Command == ApplicationCommands.Cut ||  e.Command == ApplicationCommands.Copy
            {
                e.CanExecute = false;
                e.Handled = true;
            }

        }
    }

    public static class HmacTextBoxExtensions
    {
        public static string ReplaceAt(this string input, int index, char newChar)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            char[] chars = input.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }
    }
}