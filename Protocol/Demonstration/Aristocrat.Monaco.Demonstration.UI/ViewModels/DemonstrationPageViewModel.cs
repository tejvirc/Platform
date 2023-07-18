namespace Aristocrat.Monaco.Demonstration.UI.ViewModels
{
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Kernel;
    using System.Windows.Input;
    using Application.Contracts.Localization;
    using Localization.Properties;
    using Vgt.Client12.Application.OperatorMenu;

    public sealed class DemonstrationPageViewModel : OperatorMenuPageViewModelBase
    {
        public DemonstrationPageViewModel()
        {
            DemonstrationExitButtonClickCommand = new RelayCommand<object>(OnExitButtonClickCommand);
        }

        /// <summary>
        ///     Gets the command that fires when exit button clicked.
        /// </summary>
        public ICommand DemonstrationExitButtonClickCommand { get; }

        /// <summary>
        ///     Asks the user to confirm the exit from demonstration and then initiates meters cleanup and reboot to retail.
        /// </summary>
        public void ConfirmAndExit()
        {
            var serviceManager = ServiceManager.GetInstance();
            var dialogService = serviceManager.GetService<IDialogService>();

            var result = dialogService.ShowYesNoDialog(
                this,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DemonstrationExitConfirmMessage));


            if (result != true)
            {
                Logger.Debug("Demonstration: Exit Cancelled");
                return;
            }

            Logger.Debug("Demonstration: Starting exit process...");

            EventBus.Publish(new DemonstrationExitingEvent());

            serviceManager.GetService<IOperatorMenuLauncher>().Close();
        }

        private void OnExitButtonClickCommand(object obj)
        {
            ConfirmAndExit();
        }
    }
}
