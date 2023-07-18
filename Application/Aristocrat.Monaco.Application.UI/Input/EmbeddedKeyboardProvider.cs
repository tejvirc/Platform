namespace Aristocrat.Monaco.Application.UI.Input
{
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class EmbeddedKeyboardProvider: IVirtualKeyboardProvider
    {
        private EmbeddedKeyboardWindow _keyboardWindow;

        public void CloseKeyboard()
        {
            Execute.OnUIThread(() =>
            {
                _keyboardWindow?.Close();
            });
        }

        public void OpenKeyboard(object targetControl)
        {
            Execute.OnUIThread(() =>
            {
                if (targetControl is not Control target)
                {
                    return;
                }

                if (_keyboardWindow == null)
                {
                    _keyboardWindow = new EmbeddedKeyboardWindow();
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
            Execute.OnUIThread(() =>
            {
                _keyboardWindow.Closed -= KeyboardWindowOnClosed;
                _keyboardWindow = null;
            });
        }
    }
}
