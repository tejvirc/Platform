namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using Aristocrat.Monaco.UI.Common;
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
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for CashoutResetHandCount.xaml
    /// </summary>
    public partial class CashoutResetHandCount
    {
        private readonly Window _parent;
        public CashoutResetHandCount(Window parent)
        {
            InitializeComponent();

            _parent = parent;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;
            if (isVisible)
            {
                Top = _parent.Top;
                Left = _parent.Left;

                Width = _parent.Width;
                Height = _parent.Height;

                WindowState = _parent.WindowState;
                Cursor = _parent.Cursor;
                return;
            }
        }
    }
}
