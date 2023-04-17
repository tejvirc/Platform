namespace Aristocrat.Monaco.Gaming.UI.Views.Overlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using Aristocrat.Monaco.UI.Common;
    using System.Windows.Interop;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for HandCountTimerDialog.xaml
    /// </summary>
    public partial class HandCountTimerDialog
    {
        public HandCountTimerDialog(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }
    }
}
