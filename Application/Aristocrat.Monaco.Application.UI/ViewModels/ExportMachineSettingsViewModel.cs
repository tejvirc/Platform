namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Settings;
    using Events;
    using Kernel;
    using Models;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using OperatorMenu;
    using Views;

    /// <summary>
    ///     Implements business logic for exporting machine settings.
    /// </summary>
    [CLSCompliant(false)]
    public class ExportMachineSettingsViewModel : OperatorMenuPageViewModelBase
    {
        private readonly IConfigurationSettingsManager _settingsManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _cancellation;

        private bool _isEKeyVerified;
        private bool _isEKeyDriveFound;
        private bool _isSuccess;
        private bool _isError;
        private bool _isInProgress;
        private string _exportSettingsNoteText;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExportMachineSettingsViewModel" /> class.
        /// </summary>
        public ExportMachineSettingsViewModel()
            : this(
                ServiceManager.GetInstance().GetService<IConfigurationSettingsManager>())
        {
            if (!InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            _exportSettingsNoteText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportSettingsNoteText);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExportMachineSettingsViewModel" /> class.
        /// </summary>
        /// <param name="settingsManager"><see cref="IConfigurationSettingsManager"/>.</param>
        private ExportMachineSettingsViewModel(
            IConfigurationSettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _exportSettingsNoteText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportSettingsNoteText);

            ExportCommand = new ActionCommand<object>(_ => Export(), _ => IsEKeyVerified && IsEKeyDriveFound && !IsInProgress);
        }

        /// <summary>
        ///     Gets the export command.
        /// </summary>
        public ActionCommand<object> ExportCommand { get; }

        /// <summary>
        ///     Gets a collection of configuration settings.
        /// </summary>
        public ObservableCollection<ConfigurationSetting> ConfigurationSettings { get; } = new ObservableCollection<ConfigurationSetting>();

        /// <summary>
        ///     Gets a value that indicates if the export is in progress.
        /// </summary>
        public bool IsInProgress
        {
            get => _isInProgress;

            set
            {
                SetProperty(ref _isInProgress, value);
                ExportCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether the export was successful.
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;

            set => SetProperty(ref _isSuccess, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether the export failed.
        /// </summary>
        public bool IsError
        {
            get => _isError;

            set => SetProperty(ref _isError, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether the EKey has been verified.
        /// </summary>
        public bool IsEKeyVerified
        {
            get => _isEKeyVerified;

            set
            {
                SetProperty(ref _isEKeyVerified, value);
                ExportCommand.RaiseCanExecuteChanged();
                UpdateStatusText();
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether the EKey drive has been found.
        /// </summary>
        public bool IsEKeyDriveFound
        {
            get => _isEKeyDriveFound;

            set
            {
                SetProperty(ref _isEKeyDriveFound, value);
                ExportCommand.RaiseCanExecuteChanged();
                UpdateStatusText();
            }
        }

        public string ExportSettingsNoteText
        {
            get => _exportSettingsNoteText;
            set
            {
                if (_exportSettingsNoteText != value)
                {
                    _exportSettingsNoteText = value;
                    RaisePropertyChanged(nameof(ExportSettingsNoteText));
                }
            }
        }

        /// <inheritdoc />
        protected override void OnLoaded()
        {
            EventBus.Subscribe<PropertyChangedEvent>(
                this,
                Handle,
                evt =>
                    evt.PropertyName == ApplicationConstants.EKeyVerified ||
                    evt.PropertyName == ApplicationConstants.EKeyDrive);

            EventBus.Subscribe<ConfigurationSettingsExportedEvent>(this, Handle);

            IsEKeyVerified = PropertiesManager.GetValue(ApplicationConstants.EKeyVerified, false);
            IsEKeyDriveFound = PropertiesManager.GetValue<string>(ApplicationConstants.EKeyDrive, null) != null;

            _cancellation = new CancellationTokenSource();
            EventBus.Subscribe<ConfigurationSettingsSummaryEvent>(this, Handle);
            RequestConfigurationSettingSummary();
        }

        /// <inheritdoc />
        protected override void OnUnloaded()
        {
            if (_cancellation != null && !_cancellation.IsCancellationRequested)
            {
                _cancellation.Cancel();
            }
        }

        /// <inheritdoc />
        protected override void DisposeInternal()
        {
            if (_cancellation != null)
            {
                _cancellation.Dispose();
                _cancellation = null;
            }
        }

        protected override void UpdateStatusText()
        {
            if (!IsEKeyDriveFound)
            {
                EventBus.Publish(
                    new OperatorMenuWarningMessageEvent(
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EKeyDriveNotFound)));
            }
            else if (!IsEKeyVerified)
            {
                EventBus.Publish(
                    new OperatorMenuWarningMessageEvent(
                        string.Format(
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EKeyRequiredFormat),
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportSettingsLabel).ToLower())));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        private void Handle(PropertyChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    IsEKeyVerified = PropertiesManager.GetValue(ApplicationConstants.EKeyVerified, false);
                    IsEKeyDriveFound = PropertiesManager.GetValue<string>(ApplicationConstants.EKeyDrive, null) != null;

                    if (!IsEKeyVerified || !IsEKeyDriveFound)
                    {
                        IsInProgress = false;
                    }
                });
        }

        private void Handle(ConfigurationSettingsExportedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    ConfigurationSettings.AddRange(
                        evt.Settings.Select(x => new ConfigurationSetting
                        {
                            Name = x.Key,
                            Settings = x.Value
                        }));

                    IsInProgress = false;

                    IsSuccess = true;

                    _dialogService.ShowDialog(
                        this,
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportCompleteText),
                        DialogButton.Cancel,
                        new DialogButtonCustomText
                        {
                            {
                                DialogButton.Cancel,
                                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Close)
                            }
                        },
                        Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportMachineDialog2));
                });
        }

        private void Export()
        {
            IsInProgress = true;
            IsError = false;

            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            var viewModel = new ConfigurationSavePopupViewModel(
                this,
                () => IsEKeyVerified && IsEKeyDriveFound,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportSettingsLabel),
                _settingsManager.IsConfigurationImportFilePresent(ConfigurationGroup.Machine)
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportMachineDialog1)
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExportMachineDialog3),
                false);

            var result = dialogService.ShowDialog<ConfigurationSavePopupPage>(
                this,
                viewModel,
                string.Empty,
                DialogButton.Save | DialogButton.Cancel,
                new DialogButtonCustomText
                {
                    { DialogButton.Save, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Yes) },
                    { DialogButton.Cancel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.No) }
                });

            if (!result ?? false)
            {
                IsInProgress = false;
                return;
            }

            ConfigurationSettings.Clear();

            Task.Run(async () => await _settingsManager.Export(ConfigurationGroup.Machine), _cancellation.Token)
                .ContinueWith(
                    task => task.Exception?.Handle(
                        ex =>
                        {
                            IsInProgress = false;
                            IsError = true;
                            Logger.Error(
                                $"Machine settings export failed, {ex.InnerException?.Message ?? ex.Message}",
                                ex);
                            return true;
                        }),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());

            IsInProgress = true;

        }

        private void RequestConfigurationSettingSummary()
        {
            ConfigurationSettings.Clear();
            Task.Run(async () => await _settingsManager.Summary(ConfigurationGroup.Machine), _cancellation.Token)
                .ContinueWith(
                    task => task.Exception?.Handle(
                        ex =>
                        {
                            Logger.Error(
                                $"Machine settings Summary failed, {ex.InnerException?.Message ?? ex.Message}",
                                ex);
                            return true;
                        }),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void Handle(ConfigurationSettingsSummaryEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                 () =>
                 {
                     ConfigurationSettings.AddRange(
                         evt.Settings.Select(x => new ConfigurationSetting { Name = x.Key, Settings = x.Value }));
                 });
            RaisePropertyChanged(nameof(ConfigurationSettings));
        }
    }
}
