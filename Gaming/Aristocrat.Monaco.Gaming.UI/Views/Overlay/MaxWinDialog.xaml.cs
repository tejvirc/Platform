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
        public MaxWinDialog(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }
    }
}
