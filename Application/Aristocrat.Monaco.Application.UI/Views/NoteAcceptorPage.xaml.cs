namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using ViewModels.NoteAcceptor;

    public sealed partial class NoteAcceptorPage
    {
        public NoteAcceptorPage()
        {
            InitializeComponent();
            DataContext = new NoteAcceptorViewModel(false);
        }

        [CLSCompliant(false)]
        public NoteAcceptorViewModel ViewModel { get; set; }

        private void MoveFocusAwayFromTextBox()
        {
            SelfTestButton.Focus();
        }

        private void NoteAcceptorPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel = (NoteAcceptorViewModel)DataContext;
            ViewModel.PropertyChanged += NoteAcceptor_PropertyChanged;
        }

        private void NoteAcceptor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.ChangeFocus) && ViewModel.ChangeFocus)
            {
                Execute.OnUIThread(MoveFocusAwayFromTextBox);
            }
        }
    }
}
