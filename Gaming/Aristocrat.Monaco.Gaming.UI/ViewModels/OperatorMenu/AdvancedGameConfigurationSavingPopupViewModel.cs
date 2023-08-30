namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Threading.Tasks;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Aristocrat.Extensions.CommunityToolkit;

    public class AdvancedGameConfigurationSavingPopupViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly Action _loadedAction;
        private readonly IDialogService _dialogService;

        public AdvancedGameConfigurationSavingPopupViewModel(Action loadedAction, IDialogService dialogService)
        {
            _loadedAction = loadedAction ?? throw new ArgumentNullException(nameof(loadedAction));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            var workTask = Task.Run(() => _loadedAction());
            workTask.ContinueWith((_) => Task.Run(() => MvvmHelper.ExecuteOnUI(() => _dialogService.DismissOpenedDialog())));
        }
    }
}
