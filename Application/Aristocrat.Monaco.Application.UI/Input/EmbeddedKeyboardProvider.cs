namespace Aristocrat.Monaco.Application.UI.Input
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using MVVM;

    public class EmbeddedKeyboardProvider : IVirtualKeyboardProvider
    {
        private EmbeddedKeyboardWindow _keyboardWindow;

        public void CloseKeyboard()
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                _keyboardWindow?.Close();
            });
        }

        public void OpenKeyboard(object targetControl, CultureInfo culture)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                if (targetControl is not Control target)
                {
                    return;
                }

                if (_keyboardWindow == null)
                {
                    _keyboardWindow = new EmbeddedKeyboardWindow();
                    _keyboardWindow.SetCulture(culture);
                    _keyboardWindow.Closed += KeyboardWindowOnClosed;
                }

                var window = Window.GetWindow(target);
                if (window != null && window != _keyboardWindow.Owner)
                {
                    _keyboardWindow.Owner = window;
                    _keyboardWindow.Left = (window.Width - window.Left - _keyboardWindow.Width) / 2;
                    _keyboardWindow.Top = window.Height - window.Top - _keyboardWindow.Height;
                }

                if (_keyboardWindow.IsVisible && target.IsFocused)
                {
                    return;
                }

                _keyboardWindow.Show();
                target.Focus();
            });
        }

        private void KeyboardWindowOnClosed(object sender, EventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                _keyboardWindow.Closed -= KeyboardWindowOnClosed;
                _keyboardWindow = null;
            });
        }
    }
}
