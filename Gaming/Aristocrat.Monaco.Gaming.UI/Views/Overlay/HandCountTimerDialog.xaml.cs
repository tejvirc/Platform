namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using Aristocrat.Monaco.UI.Common;
    using MahApps.Metro.Controls;

    /// <summary>
    /// Interaction logic for HandCountTimerDialog.xaml
    /// </summary>
    public partial class HandCountTimerDialog
    {
        private readonly Window _parent;
        public HandCountTimerDialog(Window parent)
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;

            _parent = parent;
        }

        private void HandCountTimerDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
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
