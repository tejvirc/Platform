namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using System;
    using System.Windows.Controls;
    using Kernel;

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
        ///     Adds the supplied displayable message to the displayable message collection and adds the string to the display box which pushes older messages off the display.
        /// </summary>
        /// <param name="text">String to add to the display box.</param>
        public void AddToDisplay(DisplayableMessage message)
        {
            ViewModel.AddMessage(message.Message);
            ViewModel.AddDisplayableMessage(message);
        }

        /// <summary>
        ///     Removes the supplied string from the display box.
        /// </summary>
        /// <param name="text">String to remove from the display box.</param>
        public void RemoveFromDisplay(DisplayableMessage message)
        {
            ViewModel.RemoveMessage(message);
        }

        /// <summary>
        ///     Removes all strings from the display box
        /// </summary>
        public void RemoveAll()
        {
            ViewModel.RemoveAll();
        }

        /// <summary>
        ///     Updates all messages currently on display
        /// </summary>
        public void UpdateDisplayMessages()
        {
            ViewModel.UpdateMessages();
        }
    }
}