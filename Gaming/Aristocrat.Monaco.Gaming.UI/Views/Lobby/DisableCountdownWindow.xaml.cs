namespace Aristocrat.Monaco.Gaming.UI.Views.Lobby
{
    using System;
    using System.Windows.Interop;
    using Monaco.UI.Common;

    /// <summary>
    ///     Interaction logic for DisableCountdownTimerWindow.xaml
    /// </summary>
    public partial class DisableCountdownWindow : BaseWindow
    {
        public DisableCountdownWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Allow clicks/touches on Lobby screen when the timerDialog is shown.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }
    }
}