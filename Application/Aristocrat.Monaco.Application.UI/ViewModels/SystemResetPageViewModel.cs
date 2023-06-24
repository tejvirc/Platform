﻿namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Windows.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM.Command;
    using OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    [CLSCompliant(false)]
    public class SystemResetPageViewModel : OperatorMenuPageViewModelBase
    {
        private string _statusText;

        public SystemResetPageViewModel()
        {
            PartialResetButtonClickCommand = new ActionCommand<object>(OnPartialResetButtonClickCommand);
            FullResetButtonClickCommand = new ActionCommand<object>(OnFullResetButtonClickCommand);
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
        ///     Gets the command that fires when Reset button clicked
        /// </summary>
        public ICommand PartialResetButtonClickCommand { get; }

        /// <summary>
        ///     Gets the command that fires when Reset button clicked
        /// </summary>
        public ICommand FullResetButtonClickCommand { get; }

        /// <summary>
        ///     User confirmation required to proceed with Partial Reset
        /// </summary>
        public void ConfirmPartialResetAndExit()
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

        /// <summary>
        ///     User confirmation required to proceed with Full Reset
        /// </summary>
        public void ConfirmFullResetAndExit()
        {
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            var result = dialogService.ShowYesNoDialog(this, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FullResetDialogText));

            if (result == true)
            {
                ServiceManager.GetInstance().GetService<IOperatorMenuLauncher>().Close();

                var persistentStorage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
                persistentStorage.Clear(PersistenceLevel.Static);
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
            ConfirmPartialResetAndExit();
        }

        private void OnFullResetButtonClickCommand(object obj)
        {
            ConfirmFullResetAndExit();
        }
    }
}