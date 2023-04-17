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
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for CashoutResetHandCount.xaml
    /// </summary>
    public partial class CashoutResetHandCount : UserControl
    {
        public CashoutResetHandCount(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }
    }
}
