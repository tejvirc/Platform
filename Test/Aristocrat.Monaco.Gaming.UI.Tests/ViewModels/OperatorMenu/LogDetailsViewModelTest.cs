namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Vouchers;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.TiltLogger;
    using Application.UI.Models;
    using Aristocrat.Monaco.Hardware.Contracts.Ticket;
    using Castle.Core.Internal;
    using Common;
    using Contracts;
    using Contracts.Progressives;
    using Gaming.Contracts.Progressives.SharedSap;
    using Hardware.Contracts.Door;
    using Kernel;
    using Localization.Properties;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class LogDetailsViewModelTest
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IEventBus> _eventBus;
        private Mock<IDoorService> _doorService;
        private Mock<ITime> _time;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IProgressiveLevelProvider> _levelProvider;
        private Mock<ISharedSapProvider> _sharedSAPProvider;
        private LogDetailsViewModel _target;
        private EventLog _eventLog;
        private EventLog _eventLogMaxLogSequence;
        private const long MaxLogSequence = 2;
        private List<EventDescription> _eventDescriptions;
        private List<IGameDetail> _gameDetail;
        private List<ProgressiveLevel> _levels = new List<ProgressiveLevel> { new ProgressiveLevel
        {
            ProgressiveId = 1,
            LevelId = 1,
            LevelType = ProgressiveLevelType.Sap,
            GameId = 1,
            Denomination = new List<long>{ 1 }
        }};

        [TestInitialize]
        public void MyTestInitialize()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            AddinManager.Initialize(currentDirectory, currentDirectory);
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict);
            _time.Setup(t => t.GetFormattedLocationTime(It.IsAny<DateTime>(), It.IsAny<string>())).Returns("11/11/2020");

            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Strict);

            _doorService = MoqServiceManager.CreateAndAddService<IDoorService>(MockBehavior.Strict);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<object>()))
                .Returns<string, object>((s, o) => o);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _eventBus.Setup(e => e.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuExitingEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<LogDetailsViewModel>(), It.IsAny<Action<ClosedEvent>>()));
            _eventBus.Setup(e => e.Subscribe(It.IsAny<LogDetailsViewModel>(), It.IsAny<Action<OpenEvent>>()));
            _eventDescriptions = MockEventLogAdapter.GetSampleEvents();
            _eventLog = new EventLog(_eventDescriptions.ElementAt(0));
            _eventLogMaxLogSequence = new EventLog(_eventDescriptions.ElementAt(1));

            _gameDetail = MockGameInfo.GetMockGameDetailInfo().ToList();
            _gameProvider = MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Strict);
            _gameProvider.Setup(p => p.GetGame(It.IsAny<int>()))
                .Returns(_gameDetail[0]);

            _sharedSAPProvider = MoqServiceManager.CreateAndAddService<ISharedSapProvider>(MockBehavior.Default);

            _levelProvider = MoqServiceManager.CreateAndAddService<IProgressiveLevelProvider>(MockBehavior.Strict);
            _levelProvider.Setup(p => p.GetProgressiveLevels())
                .Returns(_levels);

            CurrencyExtensions.SetCultureInfo(CultureInfo.CurrentCulture);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            if (AddinManager.IsInitialized)
            {
                try
                {
                    AddinManager.Shutdown();
                }
                catch (InvalidOperationException)
                {
                    // temporarily swallow exception
                }
            }

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void LogDetailsViewModelConstructorTest()
        {
            CreateTarget();
            Assert.IsNotNull(_target);
        }


        [TestMethod]
        public void WhenLogIsNotPrintableReprintButtonDisabled()
        {
            var eventLogAdapter = new Mock<IEventLogAdapter>();
            _target = new LogDetailsViewModel(_eventLog, eventLogAdapter.Object);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsReprintButtonVisible);
        }


        [TestMethod]
        public void WhenLogIsPrintableButReprintLoggedVoucherBehaviorIsNoneReprintButtonDisabled()
        {
            var eventLogAdapter = new MockEventLogAdapter(true);
            _target = new LogDetailsViewModel(_eventLog, eventLogAdapter);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsReprintButtonVisible);
        }

        [TestMethod]
        public void WhenLogIsPrintableButReprintBehaviorIsLastReprintButtonDisabledIfLastLogNotSelected()
        {
            var eventLogAdapter = new MockEventLogAdapter(true);
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, It.IsAny<string>()))
                .Returns("Last");
            _target = new LogDetailsViewModel(_eventLog, eventLogAdapter);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IsMostRecentRowSelected);
            Assert.IsFalse(_target.IsReprintButtonVisible);
        }

        [TestMethod]
        public void WhenLogIsPrintableButReprintBehaviorIsLastReprintButtonEnabledIfLastLogSelected()
        {
            var eventLogAdapter = new MockEventLogAdapter(true);
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, It.IsAny<string>()))
                .Returns("Last");
            _target = new LogDetailsViewModel(_eventLogMaxLogSequence, eventLogAdapter);

            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsMostRecentRowSelected);
            Assert.IsTrue(_target.IsReprintButtonVisible);
        }

        [TestMethod]
        public void WhenLogIsPrintableAndReprintLoggedVoucherBehaviorIsAnyReprintButtonEnabled()
        {
            var eventLogAdapter = new MockEventLogAdapter(true);
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, It.IsAny<string>()))
                .Returns("Any");
            _target = new LogDetailsViewModel(_eventLog, eventLogAdapter);
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsReprintButtonVisible);
        }

        [TestMethod]
        public void WhenReprintBehaviorIsAnyAndDoorRequirementsAreEmptyReprintNotDisabledDueToDoor()
        {
            var eventLogAdapter = new MockEventLogAdapter(true);
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, It.IsAny<string>()))
                .Returns("Any");
            _target = new LogDetailsViewModel(_eventLog, eventLogAdapter);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.ReprintDisabledDueToDoor);
            Assert.AreEqual(string.Empty, _target.DoorStatusText);
        }

        [TestMethod]
        public void WhenReprintBehaviorIsAnyAndDoorRequirementsAreNotEmptyReprintIsDisabledDueToDoorIfDoorClosed()
        {
            var eventLogAdapter = new MockEventLogAdapter(true);
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, It.IsAny<string>()))
                .Returns("Any");
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement, It.IsAny<string>()))
                .Returns("Main");
            _doorService.Setup(d => d.GetDoorClosed((int)DoorLogicalId.Main)).Returns(true);
            _doorService.Setup(d => d.GetDoorName((int)DoorLogicalId.Main)).Returns("Main");
            _target = new LogDetailsViewModel(_eventLog, eventLogAdapter);
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.ReprintDisabledDueToDoor);
            Assert.IsFalse(_target.DoorStatusText.IsNullOrEmpty());
        }

        [TestMethod]
        public void WhenReprintBehaviorIsAnyAndDoorRequirementsAreNotEmptyReprintIsNotDisabledDueToDoorIfDoorIsOpen()
        {
            var eventLogAdapter = new MockEventLogAdapter(true);
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherBehavior, It.IsAny<string>()))
                .Returns("Any");
            _propertiesManager.Setup(p => p.GetProperty(AccountingConstants.ReprintLoggedVoucherDoorOpenRequirement, It.IsAny<string>()))
                .Returns("Main");
            _doorService.Setup(d => d.GetDoorClosed((int)DoorLogicalId.Main)).Returns(false);
            _doorService.Setup(d => d.GetDoorName((int)DoorLogicalId.Main)).Returns("Main");
            _target = new LogDetailsViewModel(_eventLog, eventLogAdapter);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.ReprintDisabledDueToDoor);
            Assert.IsTrue(_target.DoorStatusText.IsNullOrEmpty());
        }

        [TestMethod]
        public void VerifyAdditionalInfoElementCount()
        {
            var sampleEvents = GetSampleEventsWithAdditionalInfo();
            foreach (var evt in sampleEvents)
            {
                var eventLog = new EventLog(evt);
                var _ = new LogDetailsViewModel(eventLog, null);
            }

        }
        private void CreateTarget()
        {
            _target = new LogDetailsViewModel(_eventLog, null);

        }

        private class MockEventLogAdapter : IEventLogAdapter, ILogTicketPrintable
        {
            private readonly bool _printable;
            public MockEventLogAdapter(bool printable)
            {
                _printable = printable;
            }

            public IEnumerable<EventDescription> GetEventLogs()
            {
                return GetSampleEvents();
            }

            public long GetMaxLogSequence()
            {
                return GetEventLogs().OrderByDescending(d => d.TransactionId).First().TransactionId;
            }

            public string LogType => "dummy";

            public IEnumerable<Ticket> GenerateLogTickets(IEnumerable<long> transactionIDs)
            {
                return null;
            }

            public Ticket GetSelectedTicket(EventDescription selectedRow)
            {
                return null;
            }

            public bool IsReprintSupported()
            {
                return _printable;
            }

            public Ticket GenerateReprintTicket(long transactionId)
            {
                return null;
            }

            public static List<EventDescription> GetSampleEvents()
            {
                return new List<EventDescription>()
                {
                    new EventDescription(
                        string.Join(
                            EventLogUtilities.EventDescriptionNameDelimiter,
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherIn),
                            "10"),
                        "info",
                        "Voucher In",
                        1,
                        DateTime.UtcNow,
                        new (string, string)[] {
                            ("TicketNumber","100"),
                            ("Amount","10"),
                            ("TypeOfAccount","XYZ"),
                            ("ValidationNumber","ABCD"),
                            ("Status","Completed"),
                            ("Details",VoucherInExceptionCode.None.GetDescription(typeof(VoucherInExceptionCode)))}){ LogSequence = 1},
                    new EventDescription(
                        string.Join(
                            EventLogUtilities.EventDescriptionNameDelimiter,
                            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherIn),
                            "10"),
                        "info",
                        EventLogType.VoucherIn.GetDescription(typeof(EventLogType)),
                        MaxLogSequence,
                        DateTime.UtcNow,
                        new (string, string)[] {
                            ("TicketNumber","101"),
                            ("Amount","10"),
                            ("TypeOfAccount","XYZ"),
                            ("ValidationNumber","ABCD"),
                            ("Status","Completed"),
                            ("Details",VoucherInExceptionCode.None.GetDescription(typeof(VoucherInExceptionCode))) }){ LogSequence =MaxLogSequence},
                };
            }
        }


        private List<EventDescription> GetSampleEventsWithAdditionalInfo()
        {
            var events = new List<EventDescription>();
            long transactionId = 2;
            var amountString = 100;
            var logSequence = 100;
            var voucherSequence = 100;
            var intString = 1;
            var transactionDateTime = DateTime.UtcNow;

            // Sample Bill Event
            var additionalInfo = new (string, string)[] {
                ("Denomination",intString.ToString()), // Denomination,
                ("AcceptedTime",transactionDateTime.AddHours(transactionId).ToString()), // Accepted? time
                ("Amount",amountString.ToString()),
                ("State","StateString"),
                ("Exception","ExceptionString") };

            var name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BillIn),
                amountString.ToString(),
                "StateString");
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.BillIn.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime, // Inserted time
                additionalInfo));

            // Sample handpay Event
            additionalInfo = new (string, string)[] {
                ("ReceiptSequence",voucherSequence.ToString()), // ReceiptSequence
                ("Type",Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Handpay)),
                ("Amount",amountString.ToString()),
                ("Barcode","BarcodeValidationString"), //Barcode
                ("State","StateString"),
                ("Printed","true") };
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                "Handpay",
                "AmountFormattedString");
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.Handpay.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo){ LogSequence = ++logSequence });

            // Sample Voucher In Event
            additionalInfo = new (string, string)[] {
                ("VoucherSequence",voucherSequence.ToString()), //VoucherSequence
                ("amount",amountString.ToString()), //Amount
                ("TypeOfAccount","TypeOfAccount"), //TypeOfAccount
                ("Barcode","BarcodeValidationString"), //Barcode
                ("Status","StatusText"),
                ("Exception","ExceptionString") };
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherIn),
                amountString.ToString()); // Amount formatted
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.VoucherIn.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo)
            { LogSequence = ++logSequence });

            // Sample Voucher Out Event
            additionalInfo = new (string, string)[] {
                ("voucherSequence",voucherSequence.ToString()), //VoucherSequence
                ("amount",amountString.ToString()), //Amount
                ("TypeOfAccount","TypeOfAccount"), //TypeOfAccount
                ("Barcode","BarcodeValidationString"), //Barcode
                ("HostAcknowledged","HostAcknowledgedString") };
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.VoucherOut),
                $"#{voucherSequence}",
                amountString.ToString());// Amount Formatted
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.VoucherOut.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo){ LogSequence = ++logSequence });

            //Sample Bonus award event
            additionalInfo = new (string, string)[] {
                ("PaidAmount",amountString.ToString()), //PaidAmount,
                ("BonusMode","BonusMode"), //BonusMode
                ("PayMethod","PayMethod"), //PayMethod
                ("SentByHost","SentByHost"), //SentByHost
                ("CashableAmount","CashableAmount"), //CashableAmount
                ("NonCashAmount","NonCashAmount"), //NonCashAmount
                ("PromoAmount","PromoAmount"), //PromoAmount
                ("BonusState","BonusState"), //BonusState
                ("FailedError","FailedError") }; //FailedError
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BonusAward),
                amountString.ToString()); //PaidAmount Formatted
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.BonusAward.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo));

            // Sample Transfer In Event
            additionalInfo = new (string, string)[] {
                ("DeviceId",intString.ToString()),//DeviceIdString
                ("RequestId",intString.ToString()),//RequestIdString,
                ("Amount",amountString.ToString()),
                ("",amountString.ToString()),
                ("",amountString.ToString()),
                ("Status","StatusString")};
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransferIn),
                amountString.ToString());
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.TransferIn.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo){ LogSequence = intString });

            // Sample Transfer Out Event
            additionalInfo = new (string, string)[] {
                ("DeviceId",intString.ToString()), //DeviceId
                ("RequestId",intString.ToString()), //RequestId
                ("CashableAmount",amountString.ToString()), //CashableAmount
                ("PromoAmount",amountString.ToString()), //PromoAmount
                ("NonCashAmount",amountString.ToString()), //NonCashAmount
                ("Status","StatusString") };
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TransferOut),
                amountString.ToString()//FormattedCurrencyString
            );
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.TransferOut.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo){ LogSequence = logSequence });

            // Sample SoftwareChange/Altered media Event
            additionalInfo = new (string, string)[] {
                ("MediaType","MediaTypeString"), //MediaType
                ("ReasonForChange","ReasonForChangeString"), //ReasonForChange
                ("Authentication","AuthenticationString")}; //Authentication
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SoftwareChange),
                "MediaTypeString" + " " + "ReasonForChangeString");
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.SoftwareChange.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo));

            // Sample jackpot/progressive Event
            additionalInfo = new (string, string)[] {
                ("PaidAmount",amountString.ToString()), //PaidAmount
                ("LevelId",intString.ToString()), // LevelId
                ("ProgId",intString.ToString()), // ProgId
                ("ThemeName","ThemeNameString"), //ThemeName
                ("",intString.ToString()), //GameId
                ("DenomId",intString.ToString()), //DenomId
                ("ValueSequence",intString.ToString()), // ValueSequence
                ("ResetValue",intString.ToString()), // ResetValue
                ("amount",amountString.ToString()),
                ("ProgressiveLevelType",((int)ProgressiveLevelType.Sap).ToString()),
                ("ValueAmount",intString.ToString())}; //ValueAmount

            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Progressive),
                amountString.ToString()); //FormattedCurrencyString
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.SoftwareChange.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo));


            // Sample keyed credits Event
            additionalInfo = new (string, string)[] {
                ("amount",amountString.ToString()),
                ("AccountType","AccountTypeString"),
                ("KeyedType","KeyedTypeString") };
            name = string.Join(
                EventLogUtilities.EventDescriptionNameDelimiter,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.KeyedCredit),
                "NameString",
                amountString.ToString());
            events.Add(new EventDescription(
                name,
                "info",
                EventLogType.KeyedCredit.GetDescription(typeof(EventLogType)),
                ++transactionId,
                transactionDateTime.AddHours(transactionId),
                additionalInfo));

            return events;
        }
    }
}
