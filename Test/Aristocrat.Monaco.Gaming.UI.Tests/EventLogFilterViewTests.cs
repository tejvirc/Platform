namespace Aristocrat.Monaco.Gaming.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.TiltLogger;
    using Application.UI.Models;
    using Common;
    using Kernel;
    using Loaders;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;

    [TestClass]
    public class EventLogFilterViewTests
    {
        private Mock<IOperatorMenuConfiguration> _operatorMenuConfiguration;
        private Mock<ITiltLogger> _tiltLogger;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ILogAdaptersService> _logAdapterService;
        private EventLogFilterViewModel _target;
        private dynamic _accessor;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);

            _operatorMenuConfiguration =
                MoqServiceManager.CreateAndAddService<IOperatorMenuConfiguration>(MockBehavior.Strict);
            _operatorMenuConfiguration.Setup(o => o.GetPageName(It.IsAny<IOperatorMenuConfigObject>()))
                .Returns(string.Empty);
            _operatorMenuConfiguration.Setup(o => o.GetSetting<Boolean>(OperatorMenuSetting.UseOperatorCultureForCurrencyFormatting, false))
                .Returns(false);

            _tiltLogger = MoqServiceManager.CreateAndAddService<ITiltLogger>(MockBehavior.Strict);
            _tiltLogger.Setup(t => t.GetEventsSubscribed(It.IsAny<string>())).Returns(1);
            _tiltLogger.Setup(t => t.GetEventsToSubscribe(It.IsAny<string>())).Returns(1);
            _tiltLogger.Setup(t => t.GetMax(It.IsAny<string>())).Returns(500);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _logAdapterService = MoqServiceManager.CreateAndAddService<ILogAdaptersService>(MockBehavior.Strict);
            _logAdapterService.Setup(l => l.GetLogAdapters()).Returns(new List<IEventLogAdapter>());

            if (System.Windows.Application.Current == null)
            {
                Activator.CreateInstance(typeof(System.Windows.Application));
            }

        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void EventLogFilterPageLoaderTest()
        {
            const string expected = "Logs";

            var target = new EventLogFilterPageLoader();

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.PageName);
        }

        [TestMethod]
        public void EventLogFilterViewModelConstructorTest()
        {
            CreateTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void WhenLogsToBeDisplayedIsNullVerifyAtLeastAllOptionsIsAvailable()
        {
            CreateTarget();
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsAllFiltersSelected);
        }

        [TestMethod]
        public void WhenLogsToBeDisplayedIsNotNullVerifySupportedFiltersAreInitialized()
        {
            var supportedLogFilter = SetupAllowedLogTypes();
            CreateTarget();
            Assert.IsNotNull(_target);
            Assert.AreEqual(
                supportedLogFilter.Length,
                _target.EventFilterCollection.Count);
        }

        [TestMethod]
        public void WhenAllFilterIsAppliedAllLogsAllowedToDisplayAreSelected()
        {
            SetupAllowedLogTypes();
            CreateTarget();
            Assert.IsNotNull(_target);
            _target.EventLogCollection = CreateEventLogs();
            _target.IsAllFiltersSelected = true;
            _target.AllFiltersSelectedCommand.Execute(null);
            Assert.AreEqual(_target.EventLogCollection.Count(), _target.FilteredLogCollection.ToList().Count());
        }

        [TestMethod]
        public void WhenSingleFilterIsAppliedOnlySelectedLogsAreVisible()
        {
            SetupAllowedLogTypes();
            CreateTarget();
            Assert.IsNotNull(_target);
            _target.EventLogCollection = CreateEventLogs();

            // Disable property changed event
            _accessor.ClearPropertyChangedDelegate();

            // Unselect "All" option
            _target.IsAllFiltersSelected = false;
            _target.AllFiltersSelectedCommand.Execute(null);

            // Select "BillIn" option
            var eventFilter = SelectFilter(EventLogType.BillIn.GetDescription(typeof(EventLogType)), true);

            Assert.IsNotNull(eventFilter);

            // No logs in filtered log list, where the log/event type is different from the filter selected.
            Assert.IsFalse(_target.FilteredLogCollection.Any(fl => fl.Description.Type != eventFilter.EventType));
        }

        [TestMethod]
        public void WhenMultipleFilterIsAppliedOnlySelectedLogsAreVisible()
        {
            SetupAllowedLogTypes();
            CreateTarget();
            Assert.IsNotNull(_target);
            _target.EventLogCollection = CreateEventLogs();

            // Disable property changed event
            _accessor.ClearPropertyChangedDelegate();

            _target.IsAllFiltersSelected = false;
            _target.AllFiltersSelectedCommand.Execute(null);
            SelectFilter(EventLogType.BillIn.GetDescription(typeof(EventLogType)), true);
            SelectFilter(EventLogType.BonusAward.GetDescription(typeof(EventLogType)), true);

            foreach (var filter in _target.EventFilterCollection)
            {
                if (!filter.IsSelected)
                {
                    var test = _target.FilteredLogCollection.Any(fl => fl.Description.Type == filter.EventType);
                    Assert.IsFalse(test);
                }
            }
        }

        private string[] SetupAllowedLogTypes()
        {
            var supportedLogFilters = new[]
            {
                EventLogType.BillIn.GetDescription(typeof(EventLogType)),
                EventLogType.BonusAward.GetDescription(typeof(EventLogType)),
                EventLogType.VoucherIn.GetDescription(typeof(EventLogType))
            };
            _propertiesManager
                .Setup(p => p.GetProperty(ApplicationConstants.LogTypesAllowedForDisplayKey, It.IsAny<object>()))
                .Returns(supportedLogFilters);
            return supportedLogFilters;
        }

        private EventLogFilterViewModel.EventFilterInfo SelectFilter(string type, bool selected = false)
        {
            var eventFilter = _target.EventFilterCollection.Where(
                ef => ef.EventType == type);
            Assert.IsNotNull(eventFilter);
            var eventFilterInfos = eventFilter.ToList();
            Assert.IsTrue(eventFilterInfos.Any());
            var selectedFilter = eventFilterInfos.First();
            selectedFilter.IsSelected = selected;
            return selectedFilter;
        }

        private void CreateTarget()
        {
            _target = new EventLogFilterViewModel();
            _accessor = new DynamicPrivateObject(_target);
        }

        private ObservableCollection<EventLog> CreateEventLogs()
        {
            const int amount = 10;
            var logSequence = 1;
            var transactionId = 1;
            var logs = new ObservableCollection<EventLog>
            {
                new EventLog(
                    new EventDescription(
                        string.Join(
                            EventLogUtilities.EventDescriptionNameDelimiter,
                            EventLogType.VoucherIn.GetDescription(typeof(EventLogType)),
                            amount.FormattedCurrencyString()),
                        "info",
                        EventLogType.VoucherIn.GetDescription(typeof(EventLogType)),
                        transactionId,
                        DateTime.UtcNow,
                        new (string,string)[]{
                           ("LOGSEQUENCE1", logSequence.ToString()),
                           ("LOGSEQUENCE", logSequence.ToString()),
                           ("AMOUNT", amount.ToString()) })),
                new EventLog(
                    new EventDescription(
                        string.Join(
                            EventLogUtilities.EventDescriptionNameDelimiter,
                            EventLogType.BillIn.GetDescription(typeof(EventLogType)),
                            amount.FormattedCurrencyString()),
                        "info",
                        EventLogType.BillIn.GetDescription(typeof(EventLogType)),
                        ++transactionId,
                        DateTime.UtcNow.AddMinutes(1),
                        new (string,string)[]{
                           ("LOGSEQUENCE1", (logSequence+1).ToString()),
                           ("LOGSEQUENCE", (logSequence+1).ToString()),
                           ("AMOUNT", amount.ToString()) })),
                new EventLog(
                    new EventDescription(
                        string.Join(
                            EventLogUtilities.EventDescriptionNameDelimiter,
                            EventLogType.BonusAward.GetDescription(typeof(EventLogType)),
                            amount.FormattedCurrencyString()),
                        "info",
                        EventLogType.BonusAward.GetDescription(typeof(EventLogType)),
                        ++transactionId,
                        DateTime.UtcNow.AddMinutes(1),
                        new (string,string)[]{
                           ("LOGSEQUENCE1", (logSequence+2).ToString()),
                           ("LOGSEQUENCE", (logSequence+2).ToString()),
                           ("AMOUNT", amount.ToString()) }))
            };
            return logs;
        }
    }
}
