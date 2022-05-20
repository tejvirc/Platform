namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using ConfigWizard;
    using Models;
    using OperatorMenu;

    [CLSCompliant(false)]
    public class ConfigurationSettingSummaryPopupViewModel : OperatorMenuSaveViewModelBase, IConfigWizardDialog
    {
        public ConfigurationSettingSummaryPopupViewModel(
            ObservableCollection<ConfigurationSetting> configurationSettings,
            string windowText,
            string windowInfoText,
            bool isInWizard)
        {
            ConfigurationSettings = configurationSettings;
            WindowText = windowText;
            WindowInfoText = windowInfoText;
            IsInWizard = isInWizard;
        }

        public string WindowText { get; set; }

        public string WindowInfoText { get; set; }

        public ObservableCollection<ConfigurationSetting> ConfigurationSettings { get; }

        public bool IsInWizard { get; set; }
    }
}