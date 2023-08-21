namespace Aristocrat.Monaco.UI.Common.Controls
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for ScrollableMessageBox.xaml
    /// </summary>
    public partial class ScrollableMessageBox
    {
        /// <param name="messages">Messages to display inside scrollable viewers</param>
        /// <param name="title">Window title</param>
        public ScrollableMessageBox(string[] messages, string title)
        {
            InitializeComponent();
            ScrollBoxes.ItemsSource = messages;
            Title = title;
            Topmost = true;
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
