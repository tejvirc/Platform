namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Hardware.Contracts.Button;
    using Kernel;
    using Monaco.Localization.Properties;
    using MVVM;
    using MVVM.Command;
    using Vgt.Client12.Application.OperatorMenu;

    [CLSCompliant(false)]
    public class OperatorMenuSaveViewModelBase : OperatorMenuPageViewModelBase, IModalDialogSaveViewModel
    {
        private readonly IOperatorMenuLauncher _operatorMenuLauncher;

        private bool? _dialogResult;
        private bool _preventOperatorMenuExit;
        private string _saveButtonText;
        private string _cancelButtonText;

        public OperatorMenuSaveViewModelBase(
            DialogButton buttons = DialogButton.Save | DialogButton.Cancel,
            IEnumerable<IDialogButtonCustomTextItem> customButtonText = null)
        {
            _operatorMenuLauncher = ServiceManager.GetInstance().TryGetService<IOperatorMenuLauncher>();
            EventBus.Subscribe<OperatorMenuExitingEvent>(this, HandleEvent);
            SaveCommand = new ActionCommand<object>(_ => Save());
            CancelCommand = new ActionCommand<object>(_ => Cancel());

            ShowSaveButton = (buttons & DialogButton.Save) == DialogButton.Save;
            ShowCancelButton = (buttons & DialogButton.Cancel) == DialogButton.Cancel;
            if (customButtonText == null)
            {
                SaveButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SaveText);
                CancelButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Cancel);
            }
            else
            {
                foreach (var text in customButtonText)
                {
                    switch (text.Button)
                    {
                        case DialogButton.Save:
                            SaveButtonText = text.Text ?? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SaveText);
                            break;
                        case DialogButton.Cancel:
                            CancelButtonText = text.Text ?? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Cancel);
                            break;
                    }
                }
            }
        }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public bool? DialogResult
        {
            get => _dialogResult;

            protected set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    RaisePropertyChanged(nameof(DialogResult));
                }
            }
        }

        public bool ShowSaveButton { get; set; }

        public bool ShowCancelButton { get; set; }

        public string SaveButtonText
        {
            get => _saveButtonText;
            set => SetProperty(ref _saveButtonText, value);
        }

        public string CancelButtonText
        {
            get => _cancelButtonText;
            set => SetProperty(ref _cancelButtonText, value);
        }

        public virtual bool CanSave => InputEnabled && HasChanges() && !HasErrors;

        public virtual bool CanCancel => true;

        protected virtual bool CloseOnRestrictedAccess => false;

        public virtual bool HasChanges()
        {
            return true;
        }

        public virtual void Save()
        {
            DialogResult = true;
        }

        protected void PreventOperatorMenuExit()
        {
            if (_preventOperatorMenuExit)
            {
                return;
            }

            _preventOperatorMenuExit = true;
            _operatorMenuLauncher?.PreventExit();
        }

        protected void AllowOperatorMenuExit()
        {
            if (!_preventOperatorMenuExit)
            {
                return;
            }

            _preventOperatorMenuExit = false;
            _operatorMenuLauncher?.AllowExit();
        }

        protected override void OnInputEnabledChanged()
        {
            RaisePropertyChanged(nameof(CanSave));
        }

        protected override void OnLoaded()
        {
            EventBus.Subscribe<SystemDownEvent>(this, HandleEvent);
            PreventOperatorMenuExit();
            base.OnLoaded();
        }

        protected override void OnUnloaded()
        {
            AllowOperatorMenuExit();
        }

        protected virtual void Cancel()
        {
            DialogResult = false;
        }

        protected override void OnInputStatusChanged()
        {
            if (CloseOnRestrictedAccess && AccessRestriction != OperatorMenuAccessRestriction.None)
            {
                MvvmHelper.ExecuteOnUI(() => DialogResult = false);
            }
        }

        protected virtual void HandleEvent(SystemDownEvent theEvent)
        {
            if ((theEvent.LogicalId == (int)ButtonLogicalId.Play ||
                 theEvent.LogicalId == (int)ButtonLogicalId.DualPlay) &&
                theEvent.Enabled == false)
            {
                MvvmHelper.ExecuteOnUI(Cancel);
            }
        }

        private void HandleEvent(OperatorMenuExitingEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(Cancel);
        }
    }
}
