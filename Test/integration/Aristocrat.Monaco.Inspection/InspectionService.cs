namespace Aristocrat.Monaco.Inspection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Application.Contracts.ConfigWizard;
    using Application.Contracts.HardwareDiagnostics;
    using Kernel;
    using Kernel.Contracts;
    using log4net;

    public class InspectionService : IInspectionService, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private static readonly string InspectorName = "QC Inspection Tool";

        private readonly IEventBus _events;
        private readonly IPropertiesManager _properties;
        private readonly Dictionary<HardwareDiagnosticDeviceCategory, InspectionResultData> _results = new Dictionary<HardwareDiagnosticDeviceCategory, InspectionResultData>();

        private HardwareDiagnosticDeviceCategory _currentCategory = HardwareDiagnosticDeviceCategory.Unknown;
        private string _currentTestCondition;
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
            var inspectorNameAndVersion = $"{InspectorName}, v{Assembly.GetExecutingAssembly().GetName().Version}";
            Logger.Debug($"Set IsInspectionOnly => true, InspectionNameAndVersion={inspectorNameAndVersion}");
            _properties.SetProperty(KernelConstants.IsInspectionOnly, true);
            _properties.SetProperty(KernelConstants.InspectionNameAndVersion, inspectorNameAndVersion);
        }

        public ICollection<InspectionResultData> Results => _results.Values;

        public void SetDeviceCategory(HardwareDiagnosticDeviceCategory category)
        {
            _currentCategory = category;

            if (!_results.ContainsKey(_currentCategory))
            {
                _results.Add(category, new InspectionResultData
                {
                    Category = _currentCategory,
                    Status = InspectionPageStatus.Untested,
                    FirmwareVersion = string.Empty,
                    FailureMessages = new List<string>()
                });
            }

            _currentTestCondition = null;
            Logger.Debug($"SetDeviceCategory {CurrentData.Category}.");
            RaiseChangeEvent();
        }

        public void SetFirmwareVersion(string firmwareVersion)
        {
            if (CurrentData is null)
            {
                return;
            }

            Logger.Debug($"SetFirmwareVersion {CurrentData.Category}/{firmwareVersion}");
            CurrentData.FirmwareVersion = firmwareVersion;
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

                _events.UnsubscribeAll(this);
                _isDisposed = true;
            }
        }

        private InspectionResultData CurrentData => _results.ContainsKey(_currentCategory) ? _results[_currentCategory] : null;

        private void RaiseChangeEvent()
        {
            _events.Publish(new InspectionResultsChangedEvent(CurrentData));
        }
    }
}
