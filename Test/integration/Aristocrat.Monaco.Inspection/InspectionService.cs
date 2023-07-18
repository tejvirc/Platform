namespace Aristocrat.Monaco.Inspection
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Automation.Provider;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Effects;
    using System.Xml.Serialization;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.HardwareDiagnostics;
    using Application.Contracts.OperatorMenu;
    using Cabinet.Contracts;
    using Kernel;
    using Kernel.Contracts;
    using log4net;
    using Test.Automation;
    using Timer = System.Timers.Timer;

    public class InspectionService : IInspectionService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly string InspectorName = "QC Inspection Tool";
        private static readonly string AutomationFilename = "AutomationInstructions.xml";
        private static readonly string VersionFilename = "version.txt";
        private static readonly string UnknownVersion = "?.?.?.?";

        private readonly IEventBus _events;
        private readonly IPropertiesManager _properties;
        private readonly Dictionary<HardwareDiagnosticDeviceCategory, InspectionResultData> _results = new ();
        private readonly List<UIElement> _currentDecoratedElements = new ();

        private InspectionAutomationConfiguration _automationConfig;
        private InspectionAutomationConfigurationPageAutomation _currentAutomationPage;
        private int _automationActionCounter;
        private Timer _automationTimer;

        private IInspectionWizard _wizard;
        private IOperatorMenuPageLoader _currentPageLoader;
        private HardwareDiagnosticDeviceCategory _currentCategory = HardwareDiagnosticDeviceCategory.Unknown;
        private string _currentTestCondition;
        private bool _isDecoratedElementsListComplete;
        private bool _isDisposed;

        public string Name => "InspectionSummaryService";

        public ICollection<Type> ServiceTypes => new[] { typeof(IInspectionService) };

        public InspectionService() : this(ServiceManager.GetInstance().GetService<IEventBus>(),
            ServiceManager.GetInstance().GetService<IPropertiesManager>())
        { }

        public InspectionService(IEventBus events, IPropertiesManager properties)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));

            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public void Initialize()
        {
            var version = File.Exists(VersionFilename)
                ? new StreamReader(VersionFilename).ReadToEnd()
                : UnknownVersion;
            var inspectorNameAndVersion = $"{InspectorName}, v{version}";
            Logger.Debug($"Set IsInspectionOnly => true, InspectionNameAndVersion={inspectorNameAndVersion}");
            _properties.SetProperty(KernelConstants.IsInspectionOnly, true);
            _properties.SetProperty(KernelConstants.InspectionNameAndVersion, inspectorNameAndVersion);

            _automationTimer = new Timer { AutoReset = false };
            _automationTimer.Elapsed += (_, _) => StartCurrentAction();

            _automationConfig = (InspectionAutomationConfiguration)new XmlSerializer(typeof(InspectionAutomationConfiguration))
                .Deserialize(new StreamReader(AutomationFilename));
        }

        public ICollection<InspectionResultData> Results => _results.Values;

        public HardwareDiagnosticDeviceCategory SetCurrentPageLoader(IOperatorMenuPageLoader loader)
        {
            _currentPageLoader = loader;
            _currentCategory = DecipherHardwareDiagnosticDeviceCategory(loader.GetType());

            if (!_results.ContainsKey(_currentCategory))
            {
                _results.Add(_currentCategory, new InspectionResultData
                {
                    Category = _currentCategory,
                    Status = InspectionPageStatus.Untested,
                    FirmwareVersions = new List<string>(),
                    FailureMessages = new List<string>()
                });
            }

            _wizard.TestNameText = string.Empty;
            _currentTestCondition = null;
            Logger.Debug($"SetDeviceCategory {CurrentData.Category}.");
            RaiseChangeEvent();

            KillControlDecoration();
            TryAutomationPage(false);

            return _currentCategory;
        }

        public void SetFirmwareVersion(string firmwareVersion)
        {
            if (CurrentData is null)
            {
                return;
            }

            Logger.Debug($"SetFirmwareVersion {CurrentData.Category}/{firmwareVersion}");
            CurrentData.FirmwareVersions.Add(firmwareVersion);
        }

        public void SetWizard(IInspectionWizard wizard) => _wizard = wizard;

        public void ManuallyStartAutoTest()
        {
            TryAutomationPage(true);
        }

        public void SetTestName(string testName)
        {
            if (CurrentData is null || _currentTestCondition == testName)
            {
                return;
            }

            if (CurrentData.Status == InspectionPageStatus.Untested)
            {
                CurrentData.Status = InspectionPageStatus.Good;
            }

            _isDecoratedElementsListComplete = true;
            _wizard.TestNameText = testName;
            _currentTestCondition = testName;
            Logger.Debug($"SetTestName {CurrentData.Category}/{_currentTestCondition}.");
            RaiseChangeEvent();
        }

        public void ReportTestFailure()
        {
            if (string.IsNullOrEmpty(_currentTestCondition) || CurrentData is null)
            {
                return;
            }

            CurrentData.FailureMessages.Add(_currentTestCondition);
            CurrentData.Status = InspectionPageStatus.Bad;
            Logger.Debug($"ReportTestFailure {CurrentData.Category}/{_currentTestCondition}.");
            RaiseChangeEvent();
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // non-managed here
                }

                _automationTimer?.Dispose();
                _events.UnsubscribeAll(this);
                _isDisposed = true;
            }
        }

        private InspectionResultData CurrentData => _results.ContainsKey(_currentCategory) ? _results[_currentCategory] : null;

        private bool IsCurrentPageAutoTestable => _currentAutomationPage is {Action: {Length: > 0}};

        private void RaiseChangeEvent()
        {
            _events.Publish(new InspectionResultsChangedEvent(CurrentData));
        }

        private void TryAutomationPage(bool userRequested)
        {
            _currentAutomationPage = _automationConfig.PageAutomation.ToList()
                .FirstOrDefault(p => (HardwareDiagnosticDeviceCategory)Enum.Parse(typeof(HardwareDiagnosticDeviceCategory), p.category) == CurrentData.Category);

            if (IsCurrentPageAutoTestable &&
                (userRequested || _results[_currentCategory].Status == InspectionPageStatus.Untested))
            {
                SetAutoTestButtonEnable(false);

                _automationActionCounter = 0;
                TryNextAutomationAction(false);
            }
            else
            {
                FinishAutomationPage();
            }
        }

        private void FinishAutomationPage()
        {
            if (_currentPageLoader != null)
            {
                // Some tests use overlays and leave the parent window lurking behind a console.
                if (_currentPageLoader.Page is UserControl pageControl)
                {
                    var window = Window.GetWindow(pageControl);
                    window?.Activate();
                }
            }

            SetAutoTestButtonEnable(IsCurrentPageAutoTestable);

            _currentAutomationPage = null;
        }

        private void TryNextAutomationAction(bool skipDelay)
        {
            if (_automationActionCounter >= _currentAutomationPage.Action.Length)
            {
                FinishAutomationPage();
                return;
            }

            var delay = _currentAutomationPage.Action[_automationActionCounter].waitMs;
            if (skipDelay || delay <= 0)
            {
                delay = 1;
            }

            _automationTimer.Interval = delay;
            _automationTimer.Start();
        }

        private void StartCurrentAction()
        {
            _automationTimer.Stop();

            if (_currentAutomationPage is null)
            {
                return;
            }

            PerformCurrentAction(_currentAutomationPage.Action[_automationActionCounter++]);
        }

        private void FinishCurrentAction(bool performed)
        {
            Logger.Debug($"Action complete: {performed}");
            TryNextAutomationAction(!performed);
        }

        private void PerformCurrentAction(InspectionAutomationConfigurationPageAutomationAction action)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (action.final)
                    {
                        Logger.Debug($"Automation final event after {_automationTimer.Interval}ms");
                        _isDecoratedElementsListComplete = true;
                        _wizard.TestNameText = string.Empty;
                        SetAutoTestButtonEnable(true);
                        KillControlDecoration();
                        return;
                    }

                    Logger.Debug($"Automation event after {_automationTimer.Interval}ms: {_currentCategory}...{action.controlName}({action.parameter}),"
                                 + $" if {action.conditionViewModel}.{(!string.IsNullOrEmpty(action.conditionMethod) ? $"{action.conditionMethod}({action.conditionEnum})" : action.conditionProperty)}");

                    // Check if there's a condition attached to the instruction
                    if (_currentPageLoader.Page is UserControl pageControl && IsViewModelConditionMet(action))
                    {
                        var searchControls = new List<DependencyObject>();
                        if (action.useChildWindows)
                        {
                            var window = Window.GetWindow(pageControl);
                            foreach (Window child in window?.OwnedWindows ?? new WindowCollection())
                            {
                                if (IsWindowConditionMet(child, action))
                                {
                                    searchControls.Add(child);
                                }
                            }
                        }
                        else
                        {
                            searchControls.Add(pageControl);
                        }

                        foreach (var searchControl in searchControls)
                        {
                            // Find the control and invoke it
                            var button = WindowHelper.FindChild<Button>(searchControl, action.controlName);
                            if (button is not null)
                            {
                                Logger.Debug($"Button invoke for {action.controlName}");
                                if (!button.IsEnabled)
                                {
                                    Logger.Debug($"But Button {action.controlName} is disabled");
                                    continue;
                                }

                                var peer = new ButtonAutomationPeer(button);
                                var invokeProvider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                                invokeProvider?.Invoke();
                                DecorateControl(button);
                                continue;
                            }

                            var toggle = WindowHelper.FindChild<ToggleButton>(searchControl, action.controlName);
                            if (toggle is not null)
                            {
                                Logger.Debug($"ToggleButton invoke for {action.controlName}");
                                if (!toggle.IsEnabled)
                                {
                                    Logger.Debug($"But ToggleButton {action.controlName} is disabled");
                                    continue;
                                }

                                var peer = new ToggleButtonAutomationPeer(toggle);
                                var toggleProvider = peer.GetPattern(PatternInterface.Toggle) as IToggleProvider;
                                toggleProvider?.Toggle();
                                DecorateControl(toggle);
                                continue;
                            }

                            var comboBox = WindowHelper.FindChild<ComboBox>(searchControl, action.controlName);
                            if (comboBox is not null)
                            {
                                Logger.Debug($"ComboBox select {action.controlName}|{action.parameter}");
                                if (!comboBox.IsEnabled)
                                {
                                    Logger.Debug($"But ComboBox {action.controlName} is disabled");
                                    continue;
                                }

                                var peer = new ComboBoxAutomationPeer(comboBox);
                                var expander = peer.GetPattern(PatternInterface.ExpandCollapse) as IExpandCollapseProvider;
                                expander?.Expand();

                                if (!SelectorPeerSelect(peer, action))
                                {
                                    SelectorDeselect(comboBox);
                                }

                                expander?.Collapse();
                                continue;
                            }

                            var listBox = WindowHelper.FindChild<ListBox>(searchControl, action.controlName);
                            if (listBox is not null)
                            {
                                Logger.Debug($"ListBox select {action.controlName}|{action.parameter}");
                                if (!listBox.IsEnabled)
                                {
                                    Logger.Debug($"But ListBox {action.controlName} is disabled");
                                    continue;
                                }

                                var peer = new ListBoxAutomationPeer(listBox);

                                if (!SelectorPeerSelect(peer, action))
                                {
                                    SelectorDeselect(listBox);
                                }
                                continue;
                            }

                            Logger.Debug($"Control {action.controlName} not found, or not visible");
                        }

                        FinishCurrentAction(true);
                    }
                });
        }

        private void DecorateControl(UIElement element)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    KillControlDecoration();

                    _currentDecoratedElements.Add(element);
                    element.Effect = new DropShadowEffect
                    {
                        BlurRadius = 40,
                        Color = Colors.SkyBlue,
                        Opacity = 1.0,
                        ShadowDepth = 0
                    };

                    var animation = new DoubleAnimationUsingKeyFrames{ RepeatBehavior = RepeatBehavior.Forever };
                    animation.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400))));
                    animation.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(800))));

                    element.Effect.BeginAnimation(DropShadowEffect.OpacityProperty, animation);
                });
        }

        private void KillControlDecoration()
        {
            if (!_isDecoratedElementsListComplete)
            {
                return;
            }

            _currentDecoratedElements.ForEach(e => e.Effect = null);
            _currentDecoratedElements.Clear();
            _isDecoratedElementsListComplete = false;
        }

        private bool SelectorPeerSelect(
            SelectorAutomationPeer peer,
            InspectionAutomationConfigurationPageAutomationAction action)
        {
            foreach (var child in peer.GetChildren() ?? new List<AutomationPeer>())
            {
                if (child is ListBoxItemAutomationPeer item)
                {
                    if (!IsMatchingListBoxSelection(action, item))
                    {
                        continue;
                    }

                    var selector = item.GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider;
                    selector?.Select();

                    DecorateControl(peer.Owner);

                    return true;
                }
            }

            return false;
        }

        private void SelectorDeselect(Selector selector)
        {
            selector.SelectedItem = null;
        }

        private void SetAutoTestButtonEnable(bool enable)
        {
            if (_wizard is null)
            {
                Logger.Debug("No wizard defined yet");
                return;
            }

            MvvmHelper.ExecuteOnUI(() => _wizard.CanStartAutoTest = enable);
        }

        private bool IsWindowConditionMet(Control control, InspectionAutomationConfigurationPageAutomationAction action)
        {
            const string displayRoleProperty = "DisplayRole";

            if (!action.childWindowMustBeMain)
            {
                return true;
            }

            var viewType = control.GetType();
            var property = viewType.GetProperty(displayRoleProperty,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property is null)
            {
                Logger.Error($"Couldn't find DisplayRole property descriptor on {viewType}");
                return false;
            }

            var propertyVal = (DisplayRole)property.GetValue(control);
            Logger.Debug($"Property DisplayRole is {propertyVal}");
            return propertyVal == DisplayRole.Main;
        }

        private bool IsViewModelConditionMet(InspectionAutomationConfigurationPageAutomationAction action)
        {
            if (!string.IsNullOrEmpty(action.conditionProperty))
            {
                var (vmType, targetObject) = GetConditionTarget(action);
                if (targetObject is null)
                {
                    return false;
                }

                var property = vmType.GetProperty(action.conditionProperty,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property is null)
                {
                    Logger.Error($"Couldn't find condition property descriptor {action.conditionProperty} from {vmType}");
                    return false;
                }

                var propertyVal = (bool)property.GetValue(targetObject);
                Logger.Debug($"Property {action.conditionProperty} is {propertyVal}");
                if (!propertyVal)
                {
                    FinishCurrentAction(false);
                    Logger.Debug("No test; abort");
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(action.conditionMethod))
            {
                var (vmType, targetObject) = GetConditionTarget(action);
                if (targetObject is null)
                {
                    return false;
                }

                var method = vmType.GetMethod(action.conditionMethod,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new [] { typeof(int) }, null);
                if (method is null)
                {
                    Logger.Error($"Couldn't find condition method descriptor {action.conditionMethod}(int) from {vmType}");
                    return false;
                }

                var methodVal = (bool)method.Invoke(targetObject, new object[] { action.conditionEnum });
                Logger.Debug($"Method {action.conditionMethod}({action.conditionEnum}) returns {methodVal}");
                if (!methodVal)
                {
                    FinishCurrentAction(false);
                    Logger.Debug("No test; abort");
                    return false;
                }
            }

            return true;
        }

        private (Type, object) GetConditionTarget(InspectionAutomationConfigurationPageAutomationAction action)
        {
            var vmType = _currentPageLoader.ViewModel.GetType();

            object targetObject = _currentPageLoader.ViewModel;
            if (!string.IsNullOrEmpty(action.conditionViewModel))
            {
                var subViewModelProperty = vmType.GetProperty(action.conditionViewModel,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (subViewModelProperty is null)
                {
                    Logger.Error($"Couldn't find property descriptor {action.conditionViewModel} from {vmType}");
                    return (null, null);
                }

                var propertyObject = subViewModelProperty.GetValue(_currentPageLoader.ViewModel);
                vmType = propertyObject.GetType();
                targetObject = propertyObject;
            }

            return (vmType, targetObject);
        }

        private bool IsMatchingListBoxSelection(InspectionAutomationConfigurationPageAutomationAction action, ListBoxItemAutomationPeer item)
        {
            var itemType = item.Item?.GetType();
            if (itemType is null)
            {
                Logger.Debug("Can't find item or its type");
                return false;
            }

            if (string.IsNullOrEmpty(action.parameterProperty))
            {
                return item.Item.ToString().Contains(action.parameter);
            }

            var property = itemType.GetProperty(
                action.parameterProperty,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property is null)
            {
                Logger.Debug($"Can't find propertyinfo {item.Item.GetType()}.{action.parameterProperty}");
                return false;
            }

            return ((string)property.GetValue(item.Item)).Contains(action.parameter);
        }

        private HardwareDiagnosticDeviceCategory DecipherHardwareDiagnosticDeviceCategory(Type type)
        {
            var shortTypeName = type.Name.ToUpper().Split('.').ToList().Last();
            foreach (HardwareDiagnosticDeviceCategory category in Enum.GetValues(typeof(HardwareDiagnosticDeviceCategory)))
            {
                var categoryName = Enum.GetName(typeof(HardwareDiagnosticDeviceCategory), category) ?? "Unknown";
                if (categoryName.EndsWith("s"))
                {
                    categoryName = categoryName.Substring(0, categoryName.Length - 1);
                }

                if (shortTypeName.StartsWith(categoryName.ToUpper()))
                {
                    return category;
                }
            }

            return HardwareDiagnosticDeviceCategory.Unknown;
        }
    }
}
