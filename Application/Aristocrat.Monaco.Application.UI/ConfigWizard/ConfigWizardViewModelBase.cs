namespace Aristocrat.Monaco.Application.UI.ConfigWizard
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Kernel;
    using Monaco.Localization.Properties;
    using OperatorMenu;

    [CLSCompliant(false)]
    public abstract class ConfigWizardViewModelBase : OperatorMenuPageViewModelBase, IConfigWizardViewModel
    {
        private bool _saved;
        private readonly IDialogService _dialogService;

        protected ConfigWizardViewModelBase(bool isWizardPage = false)
        {
            IsWizardPage = isWizardPage;
            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
        }

        public IAutoConfigurator AutoConfigurator => ServiceManager.GetInstance().TryGetService<IAutoConfigurator>();

        public IConfigWizardNavigator WizardNavigator => ServiceManager.GetInstance().TryGetService<IConfigWizardNavigator>();

        public bool IsWizardPage { get; }

        public override bool TestModeEnabled
        {
            get { return IsWizardPage || base.TestModeEnabled; }
            set => base.TestModeEnabled = value;
        }

        /// <summary>
        /// Has this page been visited (loaded) since the Platform process started?
        /// (Latches true once in <see cref="OnLoaded"/> for the process's lifetime.)
        /// </summary>
        public bool IsVisitedSinceRestart { get; private set; }

        public sealed override bool InputEnabled => IsWizardPage || base.InputEnabled;

        public void Save()
        {
            if (!_saved)
            {
                SaveChanges();
                _saved = true;
            }
        }

        protected abstract void SaveChanges();

        protected override void OnLoaded()
        {
            if (WizardNavigator != null)
            {
                WizardNavigator.IsBackButtonVisible = true;
                SetupNavigation();
            }

            Loaded();

            if (AutoConfigurator != null && AutoConfigurator.AutoConfigurationExists)
            {
                LoadAutoConfiguration();
            }

            _saved = false;
            RaisePropertyChanged(nameof(InputEnabled));
            IsVisitedSinceRestart = true;
        }

        protected override void OnUnloaded()
        {
            Save();
        }

        protected virtual void Loaded()
        {
        }

        protected override void OnInputEnabledChanged()
        {
            RaisePropertyChanged(nameof(InputEnabled));
        }

        protected virtual void SetupNavigation()
        {
            WizardNavigator.CanNavigateForward = true;
            WizardNavigator.IsBackButtonVisible = true;
            WizardNavigator.CanNavigateBackward = true;
        }

        protected virtual void LoadAutoConfiguration()
        {
            SaveChanges();
            WizardNavigator?.NavigateForward();
        }

        protected void SetAddinConfigProperty(string propertyKey, string propertyValue)
        {
            var property = (Dictionary<string, string>)PropertiesManager.GetProperty(
                ApplicationConstants.SelectedConfigurationKey,
                new Dictionary<string, string>());

            // Update the selectable addins property with the current selection.
            if (property.ContainsKey(propertyKey))
            {
                property[propertyKey] = propertyValue;
            }
            else
            {
                property.Add(propertyKey, propertyValue);
            }

            PropertiesManager.SetProperty(ApplicationConstants.SelectedConfigurationKey, property);
        }

        protected void ImportResult(string windowTitle, string windowInfoText)
        {
            _dialogService.ShowDialog(
                this,
                windowTitle,
                DialogButton.Cancel,
                new DialogButtonCustomText
                {
                    {
                        DialogButton.Cancel,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Close)
                    }
                },
                windowInfoText);
        }
    }
}
