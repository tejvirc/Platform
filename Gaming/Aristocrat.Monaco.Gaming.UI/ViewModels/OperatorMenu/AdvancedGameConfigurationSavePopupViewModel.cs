namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.ComponentModel;
    using Application.UI.OperatorMenu;

    public delegate bool CanExecute();
    public class AdvancedGameConfigurationSavePopupViewModel : OperatorMenuSaveViewModelBase
    {
        public AdvancedGameConfigurationSavePopupViewModel(
            INotifyPropertyChanged ownerViewModel,
            CanExecute canExecute,
            string windowText,
            string windowInfoText)
        {
            _canExecute = canExecute;
            WindowText = windowText;
            WindowInfoText = windowInfoText;
            ownerViewModel.PropertyChanged += _ownerViewModel_PropertyChanged;
        }

        private void _ownerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanSave));
        }

        public override bool CanSave => _canExecute.Invoke();

        public string WindowText { get; set; }

        public string WindowInfoText { get; set; }

        private readonly CanExecute _canExecute;
    }
}