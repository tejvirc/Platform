namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;
    using ConfigWizard;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Input;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Contracts.Tickets;
    using Events;
    using Hardware.Contracts;
    using Hardware.Contracts.Button;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Hardware.Contracts.Touch;
    using Kernel;
    using Kernel.Contracts;
    using Monaco.Localization.Properties;
    using Mono.Addins;
    using MVVM;
    using MVVM.Command;
    using OperatorMenu;
    using Views;

    [CLSCompliant(false)]
    public class ConfigSelectionPageViewModel : OperatorMenuPageViewModelBase, IConfigWizardNavigator, IService
    {
        private const string ImportMachineSettingsPropertyProvidersExtensionPath = "/Gaming/ImportMachineSettings/PropertyProviders";

        private readonly string _configExtensionPath = "/Application/Config/";
        private readonly string _wizardsExtensionPath = "/Application/Config/Wizards";
        private readonly bool _restartWhenFinished;

        private readonly Collection<IOperatorMenuPageLoader> _selectableConfigurationPages = new Collection<IOperatorMenuPageLoader>();
        private readonly Collection<IOperatorMenuPageLoader> _wizardPages = new Collection<IOperatorMenuPageLoader>();

        private readonly IServiceManager _serviceManager;
        private readonly ICabinetDetectionService _cabinetDetectionService;
        private readonly ISerialTouchService _serialTouchService;
        private readonly ISerialTouchCalibration _serialTouchCalibrationService;
        private readonly ITouchCalibration _touchCalibrationService;

        private int _lastWizardSelectedIndex;
        private bool _onFinishedPage;
        private bool _selectablePagesDone;
        private bool _wizardsAdded;
        private bool _canNavigateForward;
        private bool _canNavigateBackward;
        private bool _isBackButtonVisible;
        private string _pageTitle;
        private string _nextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NextButtonText);
        private bool _nextButtonFocused;
        private IOperatorMenuPageLoader _currentPageLoader;
        private bool _calibrationPending;
        private bool _serialTouchCalibrated;
        private TouchCalibrationErrorViewModel _errorViewModel;

        public ConfigSelectionPageViewModel()
        {
            _serviceManager = ServiceManager.GetInstance();
            _cabinetDetectionService = _serviceManager.GetService<ICabinetDetectionService>();
            _serialTouchService = _serviceManager.GetService<ISerialTouchService>();
            _serialTouchCalibrationService = _serviceManager.GetService<ISerialTouchCalibration>();
            _touchCalibrationService = _serviceManager.GetService<ITouchCalibration>();

            var existing = _serviceManager.TryGetService<IConfigWizardNavigator>();
            if (existing != null)
            {
                _serviceManager.RemoveService((IService)existing);
            }

            _serviceManager.AddService(this);

            _lastWizardSelectedIndex = PropertiesManager.GetValue(ApplicationConstants.ConfigWizardLastPageViewedIndex, 0);
            _selectablePagesDone = PropertiesManager.GetValue(ApplicationConstants.ConfigWizardSelectionPagesDone, false);
            var copyrightAccepted = (bool)PropertiesManager.GetProperty(ApplicationConstants.LegalCopyrightAcceptedKey, false);

            _selectableConfigurationPages.Add(new LegalCopyrightPageLoader());

            var configurations = MonoAddinsHelper.SelectableConfigurations;
            foreach (var configuration in configurations)
            {
                var nodes = MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(_configExtensionPath + configuration);
                foreach (var node in nodes)
                {
                    var wizard = (IOperatorMenuPageLoader)node.CreateInstance();
                    wizard.IsWizardPage = true;
                    wizard.Initialize();
                    _selectableConfigurationPages.Add(wizard);
                }
            }

            if (_selectablePagesDone && copyrightAccepted)
            {
                Logger.Debug("Skipping selection pages and going to first wizard page...");
                EventBus.Subscribe<PreConfigBootCompleteEvent>(this, HandlePreConfigBootCompleteEvent);
                EventBus.Publish(new AddinConfigurationCompleteEvent());
            }
            else
            {
                if (!copyrightAccepted)
                {
                    _lastWizardSelectedIndex = 0;
                    _nextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Accept);
                }

                Logger.Debug($"Navigating to {_selectableConfigurationPages[_lastWizardSelectedIndex].PageName} selection page...");

                //zhg: load Terms and Conditions Page
                _currentPageLoader = _selectableConfigurationPages[_lastWizardSelectedIndex];
                _pageTitle = _currentPageLoader?.PageName;
            }

            EventBus.Subscribe<OperatorMenuPageLoadedEvent>(this,
                _ =>
                {
                    if (_calibrationPending)
                    {
                        MvvmHelper.ExecuteOnUI(InvokeCalibration);
                    }
                });

            EventBus.Subscribe<SystemDownEvent>(this, HandleSystemDownEvent);

            // We're forcing touch screen mapping.  After doing so, we're going to force a restart
            _restartWhenFinished = !_serviceManager.GetService<ICabinetDetectionService>().TouchscreensMapped;

            BackButtonClicked = new ActionCommand<object>(BackButton_Click);
            NextButtonClicked = new ActionCommand<object>(NextButton_Click);
        }

        public ICommand BackButtonClicked { get; }

        public ICommand NextButtonClicked { get; }

        public string PageTitle
        {
            get => _pageTitle;
            set
            {
                _pageTitle = value;
                RaisePropertyChanged(nameof(PageTitle));
            }
        }

        public string NextButtonText
        {
            get => _nextButtonText;
            set
            {
                _nextButtonText = value;
                RaisePropertyChanged(nameof(NextButtonText));
            }
        }

        public bool NextButtonFocused
        {
            get => _nextButtonFocused;
            set
            {
                _nextButtonFocused = value;
                RaisePropertyChanged(nameof(NextButtonFocused));
            }
        }

        public IOperatorMenuPageLoader CurrentPageLoader
        {
            get => _currentPageLoader;
            set
            {
                if (_currentPageLoader != null && _currentPageLoader.ViewModel is IConfigWizardViewModel vm)
                {
                    vm.Save();
                }

                _currentPageLoader = value;
                RaisePropertyChanged(nameof(CurrentPage));

                if (_currentPageLoader != null)
                {
                    PageTitle = _currentPageLoader.PageName;
                }

                if (_lastWizardSelectedIndex > 0)
                {
                    NextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NextButtonText);
                }
            }
        }

        public IOperatorMenuPage CurrentPage => CurrentPageLoader?.Page;

        /// <inheritdoc/>
        public bool CanNavigateForward
        {
            get => _canNavigateForward;
            set
            {
                if (_canNavigateForward != value)
                {
                    _canNavigateForward = value;
                    RaisePropertyChanged(nameof(CanNavigateForward));
                }
            }
        }

        /// <inheritdoc/>
        public bool CanNavigateBackward
        {
            get => _canNavigateBackward;
            set
            {
                if (_canNavigateBackward != value)
                {
                    _canNavigateBackward = value;
                    RaisePropertyChanged(nameof(CanNavigateBackward));
                }
            }
        }

        public bool IsBackButtonVisible
        {
            get => _isBackButtonVisible;
            set
            {
                if (_isBackButtonVisible != value)
                {
                    _isBackButtonVisible = value;
                    RaisePropertyChanged(nameof(IsBackButtonVisible));
                }
            }
        }

        /// <inheritdoc/>
        public string Name => "BasePage";

        public ObservableCollection<string> Languages { get; } = new ObservableCollection<string>();

        /// <summary>
        ///     Gets the service types of the service
        /// </summary>
        public ICollection<Type> ServiceTypes => new[] { typeof(IConfigWizardNavigator) };

        /// <summary>
        ///     Use this to cause the configuration wizard to navigate to the next page.  This function should only be used
        ///     in special cases where navigation needs to be automated.  Navigation is normally handled by configuration
        ///     wizard parent page.
        /// </summary>
        public void NavigateForward()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<object>(NextButton_Click),
                null);
        }

        private void BackButton_Click(object o)
        {
            // VLT-6565:  We don't know exactly how a click got in here when the index was 0
            // but it appears that the back button enabled property is controlled in lots of places
            // To protect us from crashing I am going to prevent us from responding if the index is already 0.
            if (_lastWizardSelectedIndex == 0)
            {
                return;
            }

            // Disable these buttons as each page will need to determine if it can change pages
            CanNavigateForward = false;
            CanNavigateBackward = false;

            Logger.DebugFormat($"Back button click {_lastWizardSelectedIndex} to {_lastWizardSelectedIndex - 1}...");

            _lastWizardSelectedIndex--;
            PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, _lastWizardSelectedIndex);

            if (_selectablePagesDone)
            {
                Logger.DebugFormat(
                    $"Navigating back to wizard page {_wizardPages[_lastWizardSelectedIndex].PageName} page...");
                CurrentPageLoader = _wizardPages[_lastWizardSelectedIndex];
            }
            else
            {
                Logger.DebugFormat(
                    $"Navigating back to selectable configuration page {_selectableConfigurationPages[_lastWizardSelectedIndex].PageName} page...");
                CurrentPageLoader = _selectableConfigurationPages[_lastWizardSelectedIndex];
            }

            if (_onFinishedPage)
            {
                _onFinishedPage = false;
                NextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NextButtonText);
            }
        }

        private void NextButton_Click(object o)
        {
            // Disable these buttons as each page will need to determine if it can change pages
            CanNavigateForward = false;
            CanNavigateBackward = false;

            Logger.DebugFormat($"Next button click {_lastWizardSelectedIndex} to {_lastWizardSelectedIndex + 1}...");

            _lastWizardSelectedIndex++;
            PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, _lastWizardSelectedIndex);
            IsBackButtonVisible = true;

            if (CurrentPageLoader?.ViewModel is LegalCopyrightPageViewModel copyrightPage)
            {
                copyrightPage.AcceptCopyrightTerms();
            }

            if (!_selectablePagesDone)
            {
                HandleSelectableConfigurationPageNextClick();
                return;
            }

            if (_onFinishedPage)
            {
                Logger.Debug("Navigated to \"Finished\" page.");
                Finished();
            }
            else
            {
                HandleWizardPageNextClick();
            }
        }

        private void Finished(bool mapDisplays = true)
        {
            var configurator = _serviceManager.GetService<IAutoConfigurator>();
            var touchScreensMapped = _serviceManager.GetService<ICabinetDetectionService>().TouchscreensMapped;

            if (mapDisplays && !configurator.AutoConfigurationExists && !touchScreensMapped)
            {
                InvokeCalibration();
                return;
            }

            // Set configuration completed.
            PropertiesManager.SetProperty(ApplicationConstants.IsInitialConfigurationComplete, true);

            Logger.Debug("Checking if we need to print an identity ticket...");
            if ((bool)PropertiesManager.GetProperty(ApplicationConstants.CabinetPrintIdentity, false))
            {
                var printer = _serviceManager.TryGetService<IPrinter>();
                if (printer != null)
                {
                    Logger.Debug("Printing identity ticket.");
                    Print(OperatorMenuPrintData.Main);
                }
                else
                {
                    Logger.Warn("No IPrinter service available to print identity ticket.");
                }
            }
            else
            {
                Logger.Debug("Identity ticket print property not set.");
            }

            // Attach our finished event to the Selection Window.
            EventBus.Publish(new CloseConfigWindowEvent());

            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var machineSettingsImported = propertiesManager.GetValue(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);
            if (machineSettingsImported != ImportMachineSettings.None)
            {
                if (!machineSettingsImported.HasFlag(ImportMachineSettings.CabinetFeaturesPropertiesLoaded))
                {
                    var cabinetFeaturesPropertiesProvider = new CabinetFeaturesPropertiesProvider();
                }

                if (!machineSettingsImported.HasFlag(ImportMachineSettings.ApplicationConfigurationPropertiesLoaded))
                {
                    var applicationFeaturesProperty = new ApplicationConfigurationPropertiesProvider();
                }

                if (!machineSettingsImported.HasFlag(ImportMachineSettings.GamingPropertiesLoaded))
                {
                    var nodes = MonoAddinsHelper.GetSelectedNodes<TypeExtensionNode>(ImportMachineSettingsPropertyProvidersExtensionPath);
                    foreach (var node in nodes)
                    {
                        var importMachineSettingsPropertyProvider = (IPropertyProvider)node.CreateInstance();
                    }
                }

                propertiesManager.SetProperty(ApplicationConstants.MachineSettingsReimport, false);
                propertiesManager.SetProperty(ApplicationConstants.MachineSettingsReimported, false);
                propertiesManager.SetProperty(ApplicationConstants.MachineSettingsImported, ImportMachineSettings.None);
            }

            if (_restartWhenFinished)
            {
                if (!configurator.AutoConfigurationExists)
                {
                    // This was deferred to avoid any problems with the broadcast and WPF
                    _cabinetDetectionService?.MapTouchscreens();
                }

                EventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
            }
        }

        private void LoadLayer(string extensionPath)
        {
            Logger.InfoFormat($"Loading layer {extensionPath}");

            var nodes = MonoAddinsHelper.GetSelectedNodes<WizardConfigTypeExtensionNode>(extensionPath);

            // no need to continue if we don't have any wizards to configure
            if (nodes.Count == 0)
            {
                return;
            }

            // create instances of the wizard components
            foreach (var node in nodes.OrderBy(n => n.Order))
            {
                var instance = node.CreateInstance();

                Logger.DebugFormat($"Loading wizard {node.Type}");

                if (instance is IComponentWizard wizard)
                {
                    foreach (var page in wizard.WizardPages)
                    {
                        page.Initialize();
                        Logger.InfoFormat($"Adding wizard page {page.PageName}");
                        _wizardPages.Add(page);
                    }
                }
                else if (instance is IOperatorMenuPageLoader loader)
                {
                    loader.IsWizardPage = true;
                    loader.Initialize();
                    if (loader.IsVisible)
                    {
                        Logger.InfoFormat($"Adding wizard page {loader.PageName}");
                        _wizardPages.Add(loader);
                    }
                }
            }

            Logger.InfoFormat($"Loading layer {extensionPath} - complete!");
        }

        private void LoadWizards()
        {
            Logger.Info("Loading wizards...");
            _wizardsAdded = true;
            _wizardPages.Clear();

            LoadLayer(_wizardsExtensionPath);

            _wizardPages.Add(new CompletionPageLoader());

            CanNavigateForward = true;
            CanNavigateBackward = false;
            NextButtonFocused = true;
            Logger.Info("Loading wizards - complete!");
        }

        private void HandleSelectableConfigurationPageNextClick()
        {
            // If more selectable addin configuration pages exist, go to the next
            if (_lastWizardSelectedIndex < _selectableConfigurationPages.Count)
            {
                Logger.DebugFormat(
                    $"Navigating forward to wizard page {_selectableConfigurationPages[_lastWizardSelectedIndex].PageName}...");
                CurrentPageLoader = _selectableConfigurationPages[_lastWizardSelectedIndex];
                return;
            }

            // Post the event that signals that addin configuration is complete
            EventBus.Subscribe<PreConfigBootCompleteEvent>(this, HandlePreConfigBootCompleteEvent);
            EventBus.Publish(new AddinConfigurationCompleteEvent());
        }

        private void HandleWizardPageNextClick()
        {
            if (_lastWizardSelectedIndex >= _wizardPages.Count)
            {
                _lastWizardSelectedIndex = _wizardPages.Count - 1;
            }

            Logger.DebugFormat($"Navigating forward to wizard page {_wizardPages[_lastWizardSelectedIndex].PageName}...");
            if (_lastWizardSelectedIndex < _wizardPages.Count)
            {
                CurrentPageLoader = _wizardPages[_lastWizardSelectedIndex];
            }

            if (_wizardPages[_lastWizardSelectedIndex] is CompletionPageLoader)
            {
                NextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FinishedButtonContent);
                CanNavigateForward = true;
                CanNavigateBackward = true;
                _onFinishedPage = true;
            }

            NextButtonFocused = true;
        }

        private void HandlePreConfigBootCompleteEvent(IEvent theEvent)
        {
            EventBus.Unsubscribe<PreConfigBootCompleteEvent>(this);

            if (!_wizardsAdded)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(BeginConfigWizardPages));
            }
        }

        private void BeginConfigWizardPages()
        {
            //zhg: add configwizard pages
            LoadWizards();

            if (!_selectablePagesDone)
            {
                _lastWizardSelectedIndex = 0;
                _selectablePagesDone = true;

                PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, 0);
                PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardSelectionPagesDone, true);
            }

            // This is 'second' first page in the Wizard, where Hardware Selection starts
            // The 'Back' button is disabled at this point, but needs to be visible
            IsBackButtonVisible = true;

            HandleWizardPageNextClick();
        }

        private void HandleSystemDownEvent(SystemDownEvent obj)
        {
            if (CurrentPageLoader.ViewModel.IsLoaded)
            {
                if (_errorViewModel != null)
                {
                    MvvmHelper.ExecuteOnUI(() => _errorViewModel?.Close());
                }
                else
                {
                    if (PropertiesManager.GetValue(HardwareConstants.SerialTouchDisabled, "false") == "true" ||
                        _serialTouchCalibrationService.IsCalibrating)
                    {
                        return;
                    }

                    _serialTouchCalibrated = false;
                    MvvmHelper.ExecuteOnUI(InvokeCalibration);
                }
            }
            else
            {
                _calibrationPending = true;
            }
        }

        private void InvokeCalibration()
        {
            _calibrationPending = false;

            if (!_serialTouchCalibrated && _cabinetDetectionService.ExpectedDisplayDevicesWithSerialTouch != null)
            {
                if (_serialTouchCalibrationService.IsCalibrating)
                {
                    _serialTouchCalibrationService.CalibrateNextDevice();
                }
                else
                {
                    EventBus.Subscribe<SerialTouchCalibrationCompletedEvent>(this, OnSerialTouchCalibrationCompleted);
                    _serialTouchCalibrationService.BeginCalibration();
                }

                return;
            }

            if (_touchCalibrationService.IsCalibrating)
            {
                _touchCalibrationService.CalibrateNextDevice();
            }
            else
            {
                EventBus.Subscribe<TouchCalibrationCompletedEvent>(this, OnTouchCalibrationCompleted);
                _touchCalibrationService.BeginCalibration();
            }
        }

        private void OnSerialTouchCalibrationCompleted(SerialTouchCalibrationCompletedEvent e)
        {
            if (_serialTouchService.PendingCalibration)
            {
                Logger.Info("Requesting reboot with pending serial touch calibration.");
                EventBus.Publish(new ExitRequestedEvent(ExitAction.Reboot));
                return;
            }

            _serialTouchCalibrated = !_serialTouchCalibrationService.IsCalibrating;
            if (_serialTouchCalibrated)
            {
                EventBus.Unsubscribe<SerialTouchCalibrationCompletedEvent>(this);
            }

            InvokeCalibration();
        }

        private void OnTouchCalibrationCompleted(TouchCalibrationCompletedEvent e)
        {
            EventBus.Unsubscribe<TouchCalibrationCompletedEvent>(this);

            var success = e.Success;
            if (!success)
            {
                var propertyManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
                success = (bool)propertyManager.GetProperty(ApplicationConstants.IgnoreTouchCalibration, false);
            }

            if (success)
            {
                if (_onFinishedPage)
                {
                    Finished(false);
                }
            }
            else
            {
                Logger.Error($"Touch Screen Calibration Error: {e.Error}");
                var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
                _errorViewModel = new TouchCalibrationErrorViewModel { IsInWizard = true };

                MvvmHelper.ExecuteOnUI(
                    () =>
                    {
                        dialogService.ShowDialog<TouchCalibrationErrorView>(
                            this,
                            _errorViewModel,
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TouchCalibrationErrorTitle),
                            DialogButton.None);

                        _errorViewModel = null;
                        InvokeCalibration();
                    });
            }
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IIdentityTicketCreator>();
            return TicketToList(ticketCreator?.CreateIdentityTicket());
        }
    }
}
