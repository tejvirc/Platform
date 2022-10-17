namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using Hardware.Contracts.Touch;
    using Kernel;

    /// <summary>
    ///     Class definition for an TouchTextBox.  Focusing on the text box will launch the Windows Touch Keyboard.
    /// </summary>
    public class TouchTextBox : TextBox
    {
        private static readonly IEventBus EventBus;

        static TouchTextBox()
        {
            EventBus = ServiceManager.GetInstance().GetService<IEventBus>();
        }

        /// <inheritdoc cref="TextBox"/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            GotFocus += TextBox_GotFocus;
            LostFocus += TextBox_LostFocus;
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardOpenedEvent(true));
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EventBus?.Publish(new OnscreenKeyboardClosedEvent(true));
        }
    }
}
