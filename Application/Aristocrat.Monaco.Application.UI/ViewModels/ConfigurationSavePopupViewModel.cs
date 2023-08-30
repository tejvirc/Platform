namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.ComponentModel;
    using ConfigWizard;
    using OperatorMenu;

    public delegate bool CanExecute();
    [CLSCompliant(false)]
    public class ConfigurationSavePopupViewModel  : OperatorMenuSaveViewModelBase, IConfigWizardDialog
    {
        public ConfigurationSavePopupViewModel(
            INotifyPropertyChanged ownerViewModel,
            CanExecute canExecute,
            string windowText,
            string windowInfoText,
            bool isInWizard)
        {
            IsInWizard = isInWizard;
            _canExecute = canExecute;
            WindowText = windowText;
            WindowInfoText = windowInfoText;
            ownerViewModel.PropertyChanged += _ownerViewModel_PropertyChanged;
        }

        private void _ownerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CanSave));
        }

        public override bool CanSave => _canExecute.Invoke();
        public string WindowText { get; set; }
        public string WindowInfoText { get; set; }
        public bool IsInWizard { get; set ; }

        private readonly CanExecute _canExecute;
    }
}
