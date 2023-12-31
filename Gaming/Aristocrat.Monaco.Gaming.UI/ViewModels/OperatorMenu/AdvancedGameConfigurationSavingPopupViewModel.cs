﻿namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using MVVM;

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
            _loadedAction.BeginInvoke(
                ar =>
                {
                    MvvmHelper.ExecuteOnUI(() => _dialogService.DismissOpenedDialog());
                },
                this);
        }
    }
}