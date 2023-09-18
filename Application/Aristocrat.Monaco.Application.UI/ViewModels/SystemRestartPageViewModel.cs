namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Kernel.Contracts;

    /// <summary>
    ///     The view model for system restart and reboot
    /// </summary>
    [CLSCompliant(false)]
    public class SystemRestartPageViewModel : InspectionWizardViewModelBase
    {
        public SystemRestartPageViewModel(bool isWizardPage) : base(isWizardPage)
        {
            ResetPlatformCommand = new RelayCommand<object>(ResetPlatform);
            ResetMachineCommand = new RelayCommand<object>(ResetMachine);
        }
        public RelayCommand<object> ResetPlatformCommand { get; }

        public RelayCommand<object> ResetMachineCommand { get; }

        private void ResetPlatform(object obj)
        {
            EventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        private void ResetMachine(object obj)
        {
            EventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
        }
    }
}
