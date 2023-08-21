using System.Windows.Controls;
using System.Windows.Input;

namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    /// <summary>
    /// Interaction logic for OperatorMenuMultiPageControl.xaml
    /// </summary>
    public partial class OperatorMenuMultiPageControl : UserControl
    {
        public OperatorMenuMultiPageControl()
        {
            InitializeComponent();
        }

        private void ListBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            // Don't allow arrow key navigation in the listbox
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = true;
            }
        }
    }
}
