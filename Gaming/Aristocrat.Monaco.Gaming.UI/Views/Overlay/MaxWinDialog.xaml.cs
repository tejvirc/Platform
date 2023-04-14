namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using Aristocrat.Monaco.UI.Common;
    using System;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// Interaction logic for MaxWinDialog.xaml
    /// </summary>
    public partial class MaxWinDialog
    {
        private readonly Window _parent;

        public MaxWinDialog(Window parent)
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;

            _parent = parent;
        }

        private void MaxWinDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;
            if (isVisible)
            {
                Top = _parent.Top;
                Left = _parent.Left;

                Width = _parent.Width;
                Height = _parent.Height;

                WindowState = _parent.WindowState;
                return;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var hwnd = new WindowInteropHelper(this).Handle;
            WindowsServices.SetWindowExTransparent(hwnd);
        }
    }
}
