namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Contracts;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Settings;
    using Events;
    using Kernel;
    using Models;
    using Monaco.Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using Toolkit.Mvvm.Extensions;

    [CLSCompliant(false)]
    public class ImportMachineSettingsViewModel : ConfigWizardViewModelBase
    {
        private readonly IConfigurationSettingsManager _settingsManager;
        private readonly IDialogService _dialogService;

        private CancellationTokenSource _cancellation;

        private bool _isEKeyVerified;
        private bool _isEKeyDriveFound;
        private bool _isSuccess;
        private bool _isError;
        private bool _isInProgress;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportMachineSettingsViewModel" /> class.
        /// </summary>
        public ImportMachineSettingsViewModel()
            : this(
                ServiceManager.GetInstance().GetService<IConfigurationSettingsManager>())
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportMachineSettingsViewModel" /> class.
        /// </summary>
        /// <param name="settingsManager"><see cref="IConfigurationSettingsManager"/>.</param>
        private ImportMachineSettingsViewModel(
            IConfigurationSettingsManager settingsManager)
            : base(true)
        {
            _settingsManager = settingsManager;

            ImportCommand = new RelayCommand<object>(_ => Import(), _ => IsEKeyVerified && IsEKeyDriveFound && !IsInProgress);

            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
        }

        /// <summary>
        ///     Gets the import command.
        /// </summary>
        public RelayCommand<object> ImportCommand { get; }

        /// <summary>
        ///     Gets a collection of configuration settings.
        /// </summary>
        public ObservableCollection<ConfigurationSetting> ConfigurationSettings { get; } = new ObservableCollection<ConfigurationSetting>();

        /// <summary>
        ///     Gets a value that indicates if the import is in progress.
        /// </summary>
        public bool IsInProgress
        {
            get => _isInProgress;

            set
            {
                SetProperty(ref _isInProgress, value);
                ImportCommand.NotifyCanExecuteChanged();
            }
        }

        /// <summary>
        ///     Gets a value that indicates whether the import was successful.
        /// </summary>
        public bool IsSuccess
        {
            get => _isSuccess;

            set => SetProperty(ref _isSuccess, value);
        }

        /// <summary>
        ///     Gets a value that indicates whether the import failed.
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
                ImportCommand.NotifyCanExecuteChanged();
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
                ImportCommand.NotifyCanExecuteChanged();
                UpdateStatusText();
            }
        }

        protected override void SaveChanges()
        {
        }

        /// <inheritdoc />
        protected override void Loaded()
        {
            EventBus.Subscribe<PropertyChangedEvent>(
                this,
                Handle,
                evt =>
                    evt.PropertyName == ApplicationConstants.EKeyVerified ||
                    evt.PropertyName == ApplicationConstants.EKeyDrive);

            EventBus.Subscribe<ConfigurationSettingsImportedEvent>(this, Handle);

            IsEKeyVerified = PropertiesManager.GetValue(ApplicationConstants.EKeyVerified, false);
            IsEKeyDriveFound = PropertiesManager.GetValue<string>(ApplicationConstants.EKeyDrive, null) != null;

            _cancellation = new CancellationTokenSource();
        }

        /// <inheritdoc />
        protected override void OnUnloaded()
        {
            if (_cancellation != null && !_cancellation.IsCancellationRequested)
                _cancellation.Cancel();
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
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportSettingsLabel).ToLower())));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        private void Handle(PropertyChangedEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    IsEKeyVerified = PropertiesManager.GetValue(ApplicationConstants.EKeyVerified, false);
                    IsEKeyDriveFound = PropertiesManager.GetValue<string>(ApplicationConstants.EKeyDrive, null) != null;
                });
        }

        private void Handle(ConfigurationSettingsImportedEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    IsInProgress = false;

                    if (IsError)
                    {
                        IsSuccess = false;
                        ImportResult(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailed),
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportFailedMachineSettings));
                    }
                    else
                    {
                        ConfigurationSettings.AddRange(
                            evt.Settings.Select(x => new ConfigurationSetting { Name = x.Key, Settings = x.Value }));
                        IsInProgress = false;
                        IsSuccess = true;
                        ImportResult(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportCompleteText),
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettingsOverwrite));
                    }
                });
        }

        private void Import()
        {
            var result = _dialogService.ShowYesNoDialog(
                this,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportSettingsLabel),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImportMachineSettings));

            if (result == true)
            {
                IsInProgress = true;
                IsError = false;

                ConfigurationSettings.Clear();

                Task.Run(async () => await _settingsManager.Import(ConfigurationGroup.Machine), _cancellation.Token)
                    .ContinueWith(
                        task => task.Exception?.Handle(
                            ex =>
                            {
                                IsInProgress = false;
                                IsError = true;
                                Logger.Error(
                                    $"Machine settings import failed, {ex.InnerException?.Message ?? ex.Message}",
                                    ex);
                                return true;
                            }),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnFaulted,
                        TaskScheduler.FromCurrentSynchronizationContext());

                IsInProgress = true;
            }
        }
    }
}
