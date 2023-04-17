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
    using Contracts.HardwareDiagnostics;
    using Contracts.Localization;
    using Contracts.OperatorMenu;
    using Events;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Kernel.Contracts;
    using OperatorMenu;
    using Monaco.Localization.Properties;
    using MVVM;
    using MVVM.Command;

    [CLSCompliant(false)]
    public class InspectionPageViewModel : InspectionWizardViewModelBase, IConfigWizardNavigator, IInspectionWizard, IService
    {
        private readonly string _inspectionWizardsGroupName = "InspectionWizardPages";
        private readonly string _wizardsExtensionPath = "/Application/Inspection/Wizards";
        private readonly Collection<IOperatorMenuPageLoader> _wizardPages = new ();
        private readonly OperatorMenuPrintHandler _operatorMenuPrintHandler;

        private string _reportText;
        private InspectionPageStatus _reportStatus;

        private int _lastWizardSelectedIndex;
        private bool _onFinishedPage;
        private bool _canNavigateForward;
        private bool _canNavigateBackward;
        private bool _canStartAutoTest;
        private bool _isBackButtonVisible;
        private bool _isClearConfigVisible;
        private bool _isReportFailureVisible;
        private string _testNameText;
        private string _pageTitle;
        private string _nextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NextButtonText);
        private bool _nextButtonFocused;
        private IOperatorMenuPageLoader _currentPageLoader;

        public InspectionPageViewModel() : base(false)
        {
            var serviceManager = ServiceManager.GetInstance();

            var existing = serviceManager.TryGetService<IConfigWizardNavigator>();
            if (existing != null)
            {
                serviceManager.RemoveService((IService)existing);
            }

            serviceManager.AddService(this);

            _lastWizardSelectedIndex = 0;
            PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, 0);

            _operatorMenuPrintHandler = new OperatorMenuPrintHandler();
            EventBus.Subscribe<OperatorMenuPrintJobEvent>(this, HandleOperatorMenuPrintJob);
            EventBus.Subscribe<InspectionResultsChangedEvent>(this, Handle);

            PrintButtonCommand = new ActionCommand<object>(PrintButton_Click);
            ClearConfigButtonClicked = new ActionCommand<object>(ClearConfigButton_Click);
            ReportButtonClicked = new ActionCommand<object>(ReportButton_Click);
            AutoTestButtonClicked = new ActionCommand<object>(AutoTestButton_Click, _ => CanStartAutoTest);
            BackButtonClicked = new ActionCommand<object>(BackButton_Click, _ => CanNavigateBackward);
            NextButtonClicked = new ActionCommand<object>(NextButton_Click, _ => CanNavigateForward);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(BeginConfigWizardPages));
        }

        public ICommand PrintButtonCommand { get; set; }

        public ICommand ClearConfigButtonClicked { get; }

        public ICommand ReportButtonClicked { get; }

        public ICommand AutoTestButtonClicked { get; }

        public ICommand BackButtonClicked { get; }

        public ICommand NextButtonClicked { get; }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value, nameof(PageTitle));
        }

        public string NextButtonText
        {
            get => _nextButtonText;
            set => SetProperty(ref _nextButtonText, value, nameof(NextButtonText));
        }

        public bool NextButtonFocused
        {
            get => _nextButtonFocused;
            set => SetProperty(ref _nextButtonFocused, value, nameof(NextButtonFocused));
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

                SetProperty(ref _currentPageLoader, value, nameof(CurrentPage));

                if (_currentPageLoader != null)
                {
                    PageTitle = _currentPageLoader.PageName;

                    var category = Inspection?.SetCurrentPageLoader(_currentPageLoader) ?? HardwareDiagnosticDeviceCategory.Unknown;
                    IsReportFailureVisible = category != HardwareDiagnosticDeviceCategory.Unknown && category != HardwareDiagnosticDeviceCategory.Machine;
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
                SetProperty(ref _canNavigateForward, value, nameof(CanNavigateForward));
                if (NextButtonClicked is IActionCommand actionCommand)
                {
                    MvvmHelper.ExecuteOnUI(() => actionCommand.RaiseCanExecuteChanged());
                }
            }
        }

        /// <inheritdoc/>
        public bool CanNavigateBackward
        {
            get => _canNavigateBackward;
            set
            {
                SetProperty(ref _canNavigateBackward, value, nameof(CanNavigateBackward));
                if (BackButtonClicked is IActionCommand actionCommand)
                {
                    MvvmHelper.ExecuteOnUI(() => actionCommand.RaiseCanExecuteChanged());
                }
            }
        }

        public bool CanStartAutoTest
        {
            get => _canStartAutoTest;
            set
            {
                SetProperty(ref _canStartAutoTest, value, nameof(CanStartAutoTest));
                if (AutoTestButtonClicked is IActionCommand actionCommand)
                {
                    MvvmHelper.ExecuteOnUI(() => actionCommand.RaiseCanExecuteChanged());
                }

            }
        }

        public string TestNameText
        {
            get => _testNameText;
            set => SetProperty(ref _testNameText, value, nameof(TestNameText));
        }

        public bool IsBackButtonVisible
        {
            get => _isBackButtonVisible;
            set => SetProperty(ref _isBackButtonVisible, value, nameof(IsBackButtonVisible));
        }

        public bool IsClearConfigVisible
        {
            get => _isClearConfigVisible;
            set => SetProperty(ref _isClearConfigVisible, value, nameof(IsClearConfigVisible));
        }

        public bool IsReportFailureVisible
        {
            get => _isReportFailureVisible;
            set => SetProperty(ref _isReportFailureVisible, value, nameof(IsReportFailureVisible));
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
        ///     wizard reporter page.
        /// </summary>
        public void NavigateForward()
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                new Action<object>(NextButton_Click),
                null);
        }

        public string ReportText
        {
            get => _reportText;
            set => SetProperty(ref _reportText, value, nameof(ReportText));
        }

        public InspectionPageStatus ReportStatus
        {
            get => _reportStatus;
            set => SetProperty(ref _reportStatus, value, nameof(ReportStatus));
        }

        protected override void SaveChanges()
        {
        }

        private void Handle(InspectionResultsChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(() =>
            {
                if (evt.InspectionResult is null)
                {
                    ReportText = string.Empty;
                    return;
                }

                switch (evt.InspectionResult.Status)
                {
                    case InspectionPageStatus.Untested:
                        ReportText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Untested);
                        break;
                    case InspectionPageStatus.Good:
                        ReportText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OKText);
                        break;
                    case InspectionPageStatus.Bad:
                        ReportText = evt.InspectionResult.CombinedTestFailures;
                        break;
                }

                ReportStatus = evt.InspectionResult.Status;
            });
        }

        private void HandleOperatorMenuPrintJob(OperatorMenuPrintJobEvent printJob)
        {
            _operatorMenuPrintHandler?.PrintTickets(printJob.TicketsToPrint);
        }

        private void BackButton_Click(object o)
        {
            // We don't know exactly how a click got in here when the index was 0
            // but it appears that the back button enabled property is controlled in lots of places
            // To protect us from crashing, prevent us from responding if the index is already 0.
            if (_lastWizardSelectedIndex == 0)
            {
                return;
            }

            Logger.Debug($"Back button click {_lastWizardSelectedIndex} to {_lastWizardSelectedIndex - 1}...");

            _lastWizardSelectedIndex--;

            // Disable this button as each page will need to determine when it can change pages forward
            CanNavigateForward = false;
            CanNavigateBackward = _lastWizardSelectedIndex > 0;
            PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, _lastWizardSelectedIndex);

            Logger.Debug(
                $"Navigating back to wizard page {_wizardPages[_lastWizardSelectedIndex].PageName} page...");
            CurrentPageLoader = _wizardPages[_lastWizardSelectedIndex];

            if (_onFinishedPage)
            {
                _onFinishedPage = false;
                NextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NextButtonText);
            }

            IsClearConfigVisible = !_isReportFailureVisible && !_onFinishedPage;
        }

        private void NextButton_Click(object o)
        {
            if (_currentPageLoader != null && _currentPageLoader.ViewModel is IConfigWizardViewModel vm)
            {
                vm.Save();
            }

            Logger.Debug($"Next button click {_lastWizardSelectedIndex} to {_lastWizardSelectedIndex + 1}...");

            _lastWizardSelectedIndex++;

            // Disable this button as each page will need to determine when it can change pages forward
            CanNavigateForward = false;
            CanNavigateBackward = _lastWizardSelectedIndex > 0;

            PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, _lastWizardSelectedIndex);
            IsBackButtonVisible = true;

            if (_onFinishedPage)
            {
                Logger.Debug("Navigated to 'Finished' page.");
                Finished();
            }
            else
            {
                HandleWizardPageNextClick();
            }

            IsClearConfigVisible = !_isReportFailureVisible && !_onFinishedPage;
        }

        private void ClearConfigButton_Click(object o)
        {
            var storage = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            storage.Clear(PersistenceLevel.Static);

            EventBus.Publish(new ExitRequestedEvent(ExitAction.Restart));
        }

        private void PrintButton_Click(object obj)
        {
            EventBus.Publish(new PrintButtonClickedEvent());
        }

        private void ReportButton_Click(object o)
        {
            Inspection?.ReportTestFailure();
        }

        private void AutoTestButton_Click(object o)
        {
            Inspection?.ManuallyStartAutoTest();
        }

        private void LoadLayer(string extensionPath)
        {
            Logger.Info($"Loading layer {extensionPath}");

            var group = AddinConfigurationGroupNode.Get(_inspectionWizardsGroupName);
            var nodes = MonoAddinsHelper.GetConfiguredExtensionNodes<WizardConfigTypeExtensionNode>(
                new List<AddinConfigurationGroupNode>{group}, extensionPath, false);

            // no need to continue if we don't have any wizards to configure
            if (nodes.Count == 0)
            {
                return;
            }

            // create instances of the wizard components
            foreach (var node in nodes.OrderBy(n => n.Order))
            {
                var instance = node.CreateInstance();

                Logger.Debug($"Loading wizard {node.Type}");

                if (instance is IOperatorMenuPageLoader loader)
                {
                    loader.IsWizardPage = true;
                    loader.Initialize();
                    if (loader.IsVisible)
                    {
                        Logger.Info($"Adding wizard page (IOperatorMenuPageLoader) {loader.PageName}");
                        _wizardPages.Add(loader);
                    }
                }
            }

            Logger.Info($"Loading layer {extensionPath} - complete!");
        }

        private void LoadWizards()
        {
            Logger.Info("Loading wizards...");
            _wizardPages.Clear();

            LoadLayer(_wizardsExtensionPath);

            var completion = new InspectionSummaryPageLoader { IsWizardPage = true };
            completion.Initialize();
            _wizardPages.Add(completion);

            CanNavigateForward = false;
            CanNavigateBackward = false;
            CanStartAutoTest = false;
            NextButtonFocused = true;
            IsReportFailureVisible = false;
            IsClearConfigVisible = true;

            Inspection.SetWizard(this);
            Logger.Info("Loading wizards - complete!");
        }

        private void HandleWizardPageNextClick()
        {
            if (_lastWizardSelectedIndex >= _wizardPages.Count)
            {
                _lastWizardSelectedIndex = _wizardPages.Count - 1;
            }

            Logger.Debug($"Navigating forward to wizard page {_wizardPages[_lastWizardSelectedIndex].PageName}...");
            if (_lastWizardSelectedIndex < _wizardPages.Count)
            {
                CurrentPageLoader = _wizardPages[_lastWizardSelectedIndex];
            }

            if (_wizardPages[_lastWizardSelectedIndex] is InspectionSummaryPageLoader)
            {
                NextButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FinishedButtonContent);
                CanNavigateForward = false;
                CanNavigateBackward = false;
                _onFinishedPage = true;
            }

            NextButtonFocused = true;
        }

        private void BeginConfigWizardPages()
        {
            LoadWizards();

            PropertiesManager.SetProperty(ApplicationConstants.ConfigWizardLastPageViewedIndex, 0);

            HandleWizardPageNextClick();
        }

        private void Finished()
        {
            EventBus.Publish(new ExitRequestedEvent(ExitAction.ShutDown));
        }
    }
}
