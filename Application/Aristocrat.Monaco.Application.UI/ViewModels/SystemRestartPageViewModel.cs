namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     The view model for machine/platform reboot
    /// </summary>
    [CLSCompliant(false)]
    public class SystemRestartPageViewModel : InspectionWizardViewModelBase
    {
        private readonly IDialogService _dialogService;

        /// <summary>
        ///     Creates an instance of SystemRestartPageViewModel
        /// </summary>
        /// <param name="isWizardPage"> Is this a page in the wizard</param>
        public SystemRestartPageViewModel(bool isWizardPage) : base(isWizardPage)
        {
            ResetPlatformCommand = new RelayCommand<object>(ResetPlatform);
            ResetMachineCommand = new RelayCommand<object>(ResetMachine);

            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
        }

        /// <summary>
        ///     Command to reboot the platform
        /// </summary>
        public RelayCommand<object> ResetPlatformCommand { get; }

        /// <summary>
        ///     Command to reboot the machine
        /// </summary>
        public RelayCommand<object> ResetMachineCommand { get; }

        private void ResetPlatform(object obj)
        {
            var result = _dialogService.ShowYesNoDialog(
                this,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConfirmPlatformRebootMessage));
            if (result == true)
            {
                EventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
            }
        }

        private void ResetMachine(object obj)
        {
            var result = _dialogService.ShowYesNoDialog(
                this,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConfirmMachineRebootMessage));
            if (result == true)
            {
                EventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
            }
        }
    }
}
