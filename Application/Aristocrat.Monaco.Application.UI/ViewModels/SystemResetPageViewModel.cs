namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using Microsoft.Toolkit.Mvvm.Input;
    //using MVVM.Command;
    using OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    [CLSCompliant(false)]
    public class SystemResetPageViewModel : OperatorMenuPageViewModelBase
    {
        private string _statusText;
        private bool _partialResetButtonActive;

        public SystemResetPageViewModel()
        {
            PartialResetButtonClickCommand = new RelayCommand<object>(OnPartialResetButtonClickCommand);

            // Disable PartialResetButton if in game round or credits are there in machine
            var gamePlayMonitor = ServiceManager.GetInstance().TryGetService<IOperatorMenuGamePlayMonitor>();
            var balance = PropertiesManager.GetValue(PropertyKey.CurrentBalance, 0L);

            if ((gamePlayMonitor?.InGameRound ?? false) || balance != 0)
            {
                _statusText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PartialResetConditionText);
                _partialResetButtonActive = false;
            }
            else
            {
                _statusText = string.Empty;
                _partialResetButtonActive = true;
            }
        }

        /// <summary>
        ///     Gets or sets status text.
        /// </summary>
        public string StatusText
        {
            get => _statusText;

            set
            {
                _statusText = value;
                RaisePropertyChanged(nameof(StatusText));
                UpdateStatusText();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether PartialResetButton is Active
        /// </summary>
        public bool PartialResetButtonActive
        {
            get => _partialResetButtonActive;

            set
            {
                _partialResetButtonActive = value;
                RaisePropertyChanged(nameof(PartialResetButtonActive));
            }
        }

        /// <summary>
        ///     Gets the command that fires when Reset button clicked
        /// </summary>
        public ICommand PartialResetButtonClickCommand { get; }

        /// <summary>
        ///     User confirmation required to proceed with Reset
        /// </summary>
        public void ConfirmAndExit()
        {
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            var result = dialogService.ShowYesNoDialog(this, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.PartialResetDialogText));

            if (result == true)
            {
                ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>().Close();

                var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
                persistentStorage.Clear(PersistenceLevel.Critical);
            }
        }

        protected override void UpdateStatusText()
        {
            if (!string.IsNullOrEmpty(StatusText))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(StatusText));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        private void OnPartialResetButtonClickCommand(object obj)
        {
            ConfirmAndExit();
        }
    }
}