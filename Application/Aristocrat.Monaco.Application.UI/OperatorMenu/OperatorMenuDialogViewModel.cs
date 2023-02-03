namespace Aristocrat.Monaco.Application.UI.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.Input;
    using Contracts.OperatorMenu;
    using Toolkit.Mvvm.Extensions;

    [CLSCompliant(false)]
    public class OperatorMenuDialogViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IModalDialogSaveViewModel _viewModel;
        private string _windowText;

        /// <summary>
        ///     Create a window with text and a Save and/or Cancel button
        /// </summary>
        /// <param name="windowText">Content text</param>
        /// <param name="customButtonText">Alternate text for the Dialog buttons button (Apply, Yes, etc.)</param>
        /// <param name="windowInfoText">Secondary Context Text to display additional information if required</param>
        public OperatorMenuDialogViewModel(string windowText, IEnumerable<IDialogButtonCustomTextItem> customButtonText = null, string windowInfoText = null)
            : base(DialogButton.Save | DialogButton.Cancel, customButtonText)
        {
            Initialize(windowText);
            WindowInfoText = windowInfoText;
        }

        public OperatorMenuDialogViewModel(string windowText, DialogButton buttons, IEnumerable<IDialogButtonCustomTextItem> customButtonText = null, string windowInfoText = null)
            : base(buttons, customButtonText)
        {
            Initialize(windowText);
            WindowInfoText = windowInfoText;
        }

        /// <summary>
        ///     Create a dialog with the specified view/viewmodel content and a Save and/or Cancel button
        /// </summary>
        /// <param name="viewModel">Content ViewModel</param>
        /// <param name="windowTitle">Window Title text</param>
        /// <param name="buttons">Dialog Buttons to Show</param>
        /// <param name="customButtonText">Alternate text for the Dialog buttons (Close, No, etc.)</param>
        /// <param name="windowInfoText">Secondary Context Text to display additional information if required</param>
        public OperatorMenuDialogViewModel(IModalDialogSaveViewModel viewModel, string windowTitle,
            DialogButton buttons = DialogButton.Save | DialogButton.Cancel, IEnumerable<IDialogButtonCustomTextItem> customButtonText = null, string windowInfoText = null)
            : base(buttons, customButtonText)
        {
            WindowTitle = windowTitle;
            _viewModel = viewModel;
            WindowInfoText = windowInfoText;
            _viewModel.PropertyChanged += ViewModel_OnPropertyChanged;
        }

        public string WindowTitle { get; }

        public ICommand HandleLoadedCommand { get; set; }

        public string WindowText
        {
            get => _windowText;
            set
            {
                _windowText = value;
                UpdateProperties();
            }
        }

        public bool ShowTextOnly => !string.IsNullOrEmpty(WindowText);
        public string WindowInfoText { get; set; }
        public override bool CanSave => ShowTextOnly || (_viewModel?.CanSave ?? base.CanSave);

        public override void Save()
        {
            _viewModel?.Save();
            base.Save();
        }

        public override bool HasChanges()
        {
            return _viewModel == null || _viewModel.HasChanges();
        }

        private void Initialize(string windowText)
        {
            HandleLoadedCommand = new RelayCommand<object>(HandleLoaded);
            _windowText = windowText;
        }

        private void HandleLoaded(object obj)
        {
            UpdateProperties();
        }

        private void UpdateProperties()
        {
            OnPropertyChanged(nameof(WindowText));
            OnPropertyChanged(nameof(ShowTextOnly));
            OnPropertyChanged(nameof(CanSave));
        }

        private void ViewModel_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Execute.OnUIThread(() => OnPropertyChanged(nameof(CanSave)));

            if (e.PropertyName == nameof(DialogResult))
            {
                Execute.OnUIThread(() => DialogResult = _viewModel.DialogResult);
            }
        }

        protected override void DisposeInternal()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
            }

            base.DisposeInternal();
        }
    }
}
