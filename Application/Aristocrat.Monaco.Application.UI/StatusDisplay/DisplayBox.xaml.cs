namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using System;
    using System.Windows.Controls;

    /// <summary>
    ///     Interaction logic for DisplayBox.xaml
    /// </summary>
    [CLSCompliant(false)]
    public partial class DisplayBox : UserControl
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayBox" /> class.
        /// </summary>
        public DisplayBox()
        {
            InitializeComponent();

            ViewModel = new DisplayBoxViewModel();
        }

        public DisplayBoxViewModel ViewModel
        {
            get => DataContext as DisplayBoxViewModel;
            set => DataContext = value;
        }

        /// <summary>
        ///     Adds the supplied string to the display box and pushes older messages off the display.
        /// </summary>
        /// <param name="text">String to add to the display box.</param>
        //zhg**:StatusDisplay
        public void AddToDisplay(string text)
        {
            ViewModel.AddMessage(text);
        }

        /// <summary>
        ///     Removes the supplied string from the display box.
        /// </summary>
        /// <param name="text">String to remove from the display box.</param>
        public void RemoveFromDisplay(string text)
        {
            ViewModel.RemoveMessage(text);
        }

        /// <summary>
        ///     Removes all strings from the display box
        /// </summary>
        public void RemoveAll()
        {
            ViewModel.RemoveAll();
        }
    }
}