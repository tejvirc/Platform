namespace Aristocrat.Monaco.Application.Tests
{
    using Application.NoteAcceptor;
    using Contracts;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.KeySwitch;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using log4net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Timers;
    using Test.Common;
    using Tilt;

    public class StringPlaceholder
    {
        private readonly Regex _expression;

        public StringPlaceholder(string pattern = "")
        {
            _expression = new Regex(pattern);
        }

        public override bool Equals(object obj)
        {
            if (obj is string s)
            {
                return _expression.IsMatch(s);
            }

            return obj != null && obj.Equals(this);
        }

        public override int GetHashCode()
        {
            return _expression.GetHashCode() & base.GetHashCode();
        }

        public static implicit operator string(StringPlaceholder val)
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     This is a test class for EventLogger and is intended
    ///     to contain all EventLogger Unit Tests
    /// </summary>
    [TestClass]
    public class EventLoggerTest
    {
        private const int MaxEventsReceived = 300;
        private const string EventStringDelimiter = " -- ";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private static readonly DateTime TiltTimestamp = new DateTime(2012, 7, 22, 19, 45, 0, 0);

        private readonly IList<string> _backingData = new List<string>();

        private readonly string _expectedTimeValue =
            new DateTime(DateTime.MinValue.Ticks).ToString("G", CultureInfo.CurrentCulture);

        private readonly string[] _oneHundredEvents = new string[100];

        private readonly Mock<IPersistentStorageAccessor> _persistentStorageAccessor =
            new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);

        private readonly Mock<IPersistentStorageAccessor> _persistentStorageAccessorIndex =
            new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);

        private readonly Mock<IPersistentStorageTransaction> _persistentStorageTransaction =
            new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);


        /*
                private int _backingIndex;

        private Mock<IPersistentStorageManager> _persistentStorageManager;

        private Mock<IPropertiesManager> _propertiesManager;

        private TiltLogger _target;

        private PrivateObject _targetPrivateObject;

        private Mock<ITime> _time;
                public TestContext TestContext { get; set; }
        /// <summary>
        ///     Initializes class members and prepares for execution of a TestMethod.
        /// </summary>
        /// <remarks>
        ///     This function is called at the start of every test.
        /// </remarks>
        [TestInitialize]
        public void MyTestInitialize()
        {
            // get rid of extra addin files that are copied during a build
            File.Delete("Arsitocrat.Monaco.Kernel.addin.xml");
            File.Delete("Aristocrat.Monaco.Application.addin.xml");
            File.Delete("PlatformCommandLineArguments.addin.xml");
            File.Delete("USBCommunicator.addin.xml");
            File.Delete("Aristocrat.Monaco.Hardware.addin.xml");
            File.Delete("Aristocrat.Monaco.Hardware.Discovery.addin.xml");
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            Logger.InfoFormat("{0}() initialize-start", TestContext.TestName);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
            _propertiesManager.Setup(mock => mock.GetProperty("ClockFormat", ClockFormat.TwelveHour))
                .Returns(ClockFormat.TwelveHour);

            _time = MoqServiceManager.CreateAndAddService<ITime>(MockBehavior.Strict, true);
            _time.Setup(mock => mock.GetLocationTime(It.IsAny<DateTime>())).Returns(TiltTimestamp);
            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false))
                .Returns(_expectedTimeValue);
            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false))
                .Returns(_expectedTimeValue);
            _time.Setup(mock => mock.TimeZoneInformation).Returns(TimeZoneInfo.Local);
            _time.Setup(mock => mock.GetFormattedLocationTime(It.IsAny<DateTime>())).Returns(_expectedTimeValue);

            _persistentStorageManager =
                MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);

            _persistentStorageTransaction.Setup(mock => mock.Dispose());
            _persistentStorageTransaction.Setup(mock => mock.Commit());

            _target = new TiltLogger();
            _targetPrivateObject = new PrivateObject(_target);

            for (var i = 0; i < _oneHundredEvents.Length; i++)
            {
                _oneHundredEvents[i] = "Not Null." + EventStringDelimiter + "Default" + EventStringDelimiter +
                                       _expectedTimeValue;
            }

            SetupPersistence();
            InvokeTargetMethod("CreatePersistence");

            Logger.InfoFormat("{0}() initialize-end", TestContext.TestName);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            Logger.InfoFormat("{0}() cleanup-start", TestContext.TestName);

            _target = null;

            MoqServiceManager.RemoveInstance();
            AddinManager.Shutdown();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Logger.InfoFormat("{0}() cleanup-end{1}", TestContext.TestName, Environment.NewLine);
        }
        
        /// <summary>A test for ServiceTypes</summary>
        [TestMethod]
        public void ServiceTypesTest()
        {
            var target = new TiltLogger();
            var actual = target.ServiceTypes as IList<Type>;
            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(ITiltLogger), actual[0]);
        }

        /// <summary>A test for Name</summary>
        [TestMethod]
        public void NameTest()
        {
            var target = new TiltLogger();
            Assert.AreEqual("TiltLogger", target.Name);
        }

        /// <summary>A test for Initialized</summary>
        [TestMethod]
        public void InitializedTest()
        {
            Assert.IsFalse(_target.Initialized);
        }

        /// <summary>A test for SubscribeToEvents</summary>
        [TestMethod]
        public void SubscribeToEventsTest()
        {
            var eventsToSubscribe = new List<EventDescription>
            {
                new EventDescription(
                    "Aristocrat.Monaco.Kernel.BaseEvent, Aristocrat.Monaco.Kernel.Contracts",
                    "DNC",
                    DateTime.UtcNow),
                new EventDescription("ThisOneShouldDoNothing", "NotValid", DateTime.UtcNow)
            };
            SetTargetField("_eventsToSubscribe", eventsToSubscribe);

            InvokeTargetMethod("SubscribeToEvents");

            var expected = 1;
            var actual = (int)_targetPrivateObject.GetProperty("TiltsSubscribed");
            Assert.AreEqual(expected, actual);
        }

        /// <summary>A test for ReceiveEvent</summary>
        [TestMethod]
        public void ReceiveEventTest()
        {
            SetupPersistence();
            SetupAccessorProperties(_oneHundredEvents);

            var saedata = new ServiceAddedEvent();
            var sredata = new ServiceRemovedEvent();

            InvokeTargetMethod("ConfigurationParse");

            var saeTime = saedata.Timestamp.ToString("G", CultureInfo.CurrentCulture);
            var sreTime = sredata.Timestamp.ToString("G", CultureInfo.CurrentCulture);
            SetupTransactionSets(1, "ServiceAddedEvent --  -- " + saeTime);
            SetupTransactionSets(2, "ServiceRemovedEvent --  -- " + sreTime);

            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false))
                .Returns(saeTime);
            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false)).Returns(saeTime);
            InvokeTargetMethod("ReceiveEvent", saedata);
            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false))
                .Returns(sreTime);
            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false)).Returns(sreTime);
            _time.Setup(mock => mock.GetFormattedLocationTime(It.IsAny<DateTime>())).Returns(sreTime);
            InvokeTargetMethod("ReceiveEvent", sredata);
            InvokeTargetMethod("ProcessEvents");

            SetTargetStaticProperty("MaximumEvents", 100);
            var eventsReceived = _target.GetEvents().ToList();
            Assert.IsNotNull(eventsReceived.FirstOrDefault(i => i.Name.Contains("ServiceAddedEvent")));
            Assert.IsNotNull(eventsReceived.FirstOrDefault(i => i.Name.Contains("ServiceRemovedEvent")));
        }

        /// <summary>
        ///     A test for ReceiveEvent. Exercise remove_TiltLogAppendedError/Tilt
        /// </summary>
        [TestMethod]
        public void ReceiveEventTest2()
        {
            EventHandler<TiltLogAppendedEventArgs> tiltLogEventHandler = TiltLogAppended;

            _target.TiltLogAppendedTilt += tiltLogEventHandler;

            InvokeTargetMethod("ConfigurationParse");

            _target.TiltLogAppendedTilt -= tiltLogEventHandler;
        }

        /// <summary>A test for ReceiveEvent</summary>
        [TestMethod]
        public void ReceiveNoteAcceptorHardwareErrorEventTest()
        {
            SetupIndexToWriteIndexZero();
            SetupPersistence();

            SetupAccessorProperties(_oneHundredEvents);

            var listOfEventNames = NoteAcceptorEventsDescriptor.FaultTexts;

            InvokeTargetMethod("ConfigurationParse");

            var names = NoteAcceptorEventsDescriptor.FaultTexts;
            var i = 0;
            foreach (var pair in listOfEventNames)
            {
                var eventString = "Note Acceptor " + names[pair.Key] + EventStringDelimiter + "error" +
                                  EventStringDelimiter + _expectedTimeValue;
                SetupTransactionSets(i, eventString);

                InvokeTargetMethod("ReceiveEvent", new HardwareFaultEvent(pair.Key));

                i++;
            }

            InvokeTargetMethod("ProcessEvents");

            SetTargetStaticProperty("MaximumEvents", 100);
            var eventsReceived = _target.GetEvents().ToList();
            foreach (var pair in listOfEventNames)
            {
                Assert.IsNotNull(eventsReceived.FirstOrDefault(m => m.Name.Contains(pair.Value)));
            }
        }

        /// <summary>A test for ReceiveEvent</summary>
        [TestMethod]
        public void ReceiveNoteAcceptorHardwareFaultClearEventTest()
        {
            SetupIndexToWriteIndexZero();
            SetupPersistence();
            SetupAccessorProperties(_oneHundredEvents);

            var listOfEventNames = NoteAcceptorEventsDescriptor.FaultTexts;

            InvokeTargetMethod("ConfigurationParse");

            var names = NoteAcceptorEventsDescriptor.FaultTexts;
            var i = 0;
            foreach (var pair in listOfEventNames)
            {
                var eventString = "Note Acceptor " + names[pair.Key] + " Cleared" + EventStringDelimiter +
                                  "error" + EventStringDelimiter + _expectedTimeValue;
                SetupTransactionSets(i, eventString);

                InvokeTargetMethod("ReceiveEvent", new HardwareFaultClearEvent(pair.Key));
                i++;
            }

            InvokeTargetMethod("ProcessEvents");

            SetTargetStaticProperty("MaximumEvents", 100);
            var eventsReceived = _target.GetEvents().ToList();
            foreach (var pair in listOfEventNames)
            {
                Assert.IsNotNull(eventsReceived.FirstOrDefault(m => m.Name.Contains(pair.Value)));
            }
        }

        /// <summary>A test for Initialize</summary>
        [TestMethod]
        public void InitializeTest()
        {
            SetupIndexToWriteIndexZero();

            SetupPersistence();
            SetupAccessorProperties(_oneHundredEvents);

            _persistentStorageManager.Setup(m => m.VerifyIntegrity(true)).Returns(true);
            SetupTransactionSets(
                0,
                "Software verified" + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);
            SetupTransactionSets(
                1,
                "NVRAM Integrity check - Passed." + EventStringDelimiter + "info" + EventStringDelimiter +
                _expectedTimeValue);

            _target.Initialize();
            Assert.AreEqual(true, _target.Initialized);
            SetTargetStaticProperty("MaximumEvents", 100);

            InvokeTargetMethod("ProcessEvents");

            var eventsReceived = _target.GetEvents().ToList();
            Assert.AreEqual(100, eventsReceived.Count);
            Assert.IsNotNull(eventsReceived.FirstOrDefault(i => i.Name.Contains("Software verified")));
            Assert.IsNotNull(eventsReceived.FirstOrDefault(i => i.Name.Contains("NVRAM Integrity check")));
        }

        /// <summary>A test for Initialize</summary>
        [TestMethod]
        public void InitializeTestWithPersistedData()
        {
            SetupIndexToWriteIndexZero();
            SetupPersistence();

            // Create backing store to hold the two generated events and mark the end of the persisted range.
            var persistedEventStrings = CreateBackingDataStrings("name", "level", DateTime.Now, 50);
            SetupAccessorProperties(persistedEventStrings);

            // 2 for those generated during the bootup process, 50 previously persisted.
            var expectedEventCount = 50;

            _persistentStorageManager.Setup(m => m.VerifyIntegrity(true)).Returns(true);

            SetupTransactionSets(
                0,
                "Software verified" + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);
            SetupTransactionSets(
                1,
                "NVRAM Integrity check - Passed." + EventStringDelimiter + "info" + EventStringDelimiter +
                _expectedTimeValue);

            _target.Initialize();
            Assert.AreEqual(true, _target.Initialized);
            SetTargetStaticProperty("MaximumEvents", 100);

            InvokeTargetMethod("ProcessEvents");

            var eventsReceived = _target.GetEvents().ToList();
            Assert.AreEqual(expectedEventCount, eventsReceived.Count);
            Assert.IsNotNull(
                eventsReceived.FirstOrDefault(i => i.Name.Contains("Software verified")));
            Assert.IsNotNull(
                eventsReceived.FirstOrDefault(i => i.Name.Contains("NVRAM Integrity check")));
        }

        /// <summary>A test for Initialize x2</summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceException), "Cannot initialize more than once.")]
        public void InitializeTwiceThrowsExceptionTest()
        {
            SetupIndexToWriteIndexZero();
            SetupPersistence();
            SetupAccessorProperties(_oneHundredEvents);
            _persistentStorageManager.Setup(m => m.VerifyIntegrity(true)).Returns(true);
            SetupTransactionSets(
                0,
                "Software verified" + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);
            SetupTransactionSets(
                1,
                "NVRAM Integrity check - Passed." + EventStringDelimiter + "info" + EventStringDelimiter +
                _expectedTimeValue);

            _target.Initialize();
            _target.Initialize();
        }

        /// <summary>A test for FromDelimitedString with too few parts</summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Could not parse.")]
        public void FromDelimitedStringTooFewPartsThrowsExceptionTest()
        {
            var tooFewParts = "One" + EventStringDelimiter + "Two";
            InvokeTargetMethod("FromDelimitedString", tooFewParts);
        }

        /// <summary>A test for FromDelimitedString with too many parts</summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Could not parse.")]
        public void FromDelimitedStringTooManyPartsThrowsExceptionTest()
        {
            var tooManyParts = "One" + EventStringDelimiter + "Two" + EventStringDelimiter + "Three" +
                               EventStringDelimiter + "Four";
            InvokeTargetMethod("FromDelimitedString", tooManyParts);
        }

        /// <summary>A test for Dispose with no event bus service</summary>
        [TestMethod]
        public void DisposeNoEventBusServiceTest()
        {
            MoqServiceManager.RemoveService<IEventBus>();
            _target.Dispose();
        }

        /// <summary>A test for GetEventList</summary>
        [TestMethod]
        public void GetEventListTest3()
        {
            var now = DateTime.UtcNow;
            SetupPersistence();
            SetupAccessorProperties(CreateBackingDataStrings("name", "level", now, 2));

            var maxcount = 0;
            var expected = new Collection<EventDescription>();
            var actual = _target.GetEvents(null, maxcount).ToList();
            Assert.AreEqual(expected.Count, actual.Count);

            maxcount = 100;
            expected = new Collection<EventDescription>
            {
                new EventDescription("name0", "level0", Guid.NewGuid(), now),
                new EventDescription("name1", "level1", Guid.NewGuid(), now)
            };
            actual = _target.GetEvents(null, maxcount).ToList();
            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected.Count, actual.Count);

            var filter = new TiltFilterArgs[2];
            filter[0] = new TiltFilterArgs(EventDescriptionProperty.Level, "level0", true);
            filter[1] = new TiltFilterArgs(EventDescriptionProperty.Level, "level1", true);
            expected = new Collection<EventDescription>
            {
                new EventDescription("name0", "level0", Guid.NewGuid(), now),
                new EventDescription("name1", "level1", Guid.NewGuid(), now)
            };
            actual = _target.GetEvents(filter, maxcount).ToList();
            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected.Count, actual.Count);

            filter = new[] { new TiltFilterArgs(EventDescriptionProperty.Level, "level0", false) };
            expected = new Collection<EventDescription>
            {
                new EventDescription("name1", "level1", Guid.NewGuid(), now)
            };
            actual = _target.GetEvents(filter, maxcount).ToList();
            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected.Count, actual.Count);

            filter = new TiltFilterArgs[2];
            filter[0] = new TiltFilterArgs(EventDescriptionProperty.Level, "level0", true);
            filter[1] = new TiltFilterArgs(EventDescriptionProperty.Level, "level1", false);
            expected = new Collection<EventDescription>
            {
                new EventDescription("name0", "level0", Guid.NewGuid(), now)
            };
            actual = _target.GetEvents(filter, maxcount).ToList();
            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected.Count, actual.Count);

            filter = new TiltFilterArgs[1];
            filter[0] = new TiltFilterArgs(EventDescriptionProperty.Level, "level0", true);
            expected = new Collection<EventDescription>
            {
                new EventDescription("name0", "level0", Guid.NewGuid(), now)
            };
            actual = _target.GetEvents(filter, maxcount).ToList();
            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected.Count, actual.Count);

            maxcount = 1;
            expected = new Collection<EventDescription>
            {
                new EventDescription("name0", "level0", Guid.NewGuid(), now)
            };
            actual = _target.GetEvents(null, maxcount).ToList();
            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected.Count, actual.Count);
        }

        /// <summary>A test for GetEventList</summary>
        [TestMethod]
        public void GetEventListNullFilterTest()
        {
            var now = DateTime.UtcNow;
            SetupPersistence();
            SetupAccessorProperties(CreateBackingDataStrings("name", "level", now, 2));

            var expected = new Collection<EventDescription>
            {
                new EventDescription("name0", "level0", Guid.NewGuid(), now),
                new EventDescription("name1", "level1", Guid.NewGuid(), now)
            };
            var actual = _target.GetEvents(null).ToList();

            Assert.AreEqual(expected[0].Name, actual[0].Name);
            Assert.AreEqual(expected[1].Level, actual[1].Level);
        }

        private string[] CreateBackingDataStrings(
            string name,
            string level,
            DateTime? now,
            int count = 1,
            string delimiter = null)
        {
            var values = new string[TiltLogger.MaximumEvents];
            delimiter = delimiter ?? (string)GetTargetStaticField("EventStringDelimiter");
            var index = 0;
            var date = now != null ? $"{now:M / d / yyyy HH:mm:ss tt}" : string.Empty;
            while (index < count)
            {
                values[index] = $"{name}{index}{delimiter}{level}{index}{delimiter}{date}";
                ++index;
            }

            return values;
        }

        /// <summary>A test for GetDefaultFilters.  Are parameters supposed to be tested?</summary>
        [TestMethod]
        public void DefaultFiltersTest()
        {
            var target = new TiltLogger();

            var actual = target.DefaultFilters;

            Assert.AreEqual(true, actual.ContainsKey("All"));
            Assert.AreEqual(true, actual.ContainsKey("Error"));
            Assert.AreEqual(1, actual["Error"].Count);
        }

        /// <summary>A test for Dispose</summary>
        [TestMethod]
        public void DisposeTest()
        {
            var eventbus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            eventbus.Setup(m => m.UnsubscribeAll(It.IsAny<object>())).Verifiable();

            var eventsToSubscribe = new List<EventDescription>
            {
                new EventDescription(
                    "Aristocrat.Monaco.Kernel.BaseEvent, Aristocrat.Monaco.Kernel.Contracts",
                    "DNC",
                    DateTime.UtcNow),
                new EventDescription("ThisOneShouldDoNothing", "NotValid", DateTime.UtcNow)
            };
            _targetPrivateObject.SetField("_eventsToSubscribe", eventsToSubscribe);
            _targetPrivateObject.SetField("_eventUpdateTimer", new Timer());

            _target.Dispose();

            // dispose again to get more coverage
            _target.Dispose();

            Assert.IsTrue((bool)GetTargetField("_disposed"));
            eventbus.Verify();
        }

        /// <summary>A test for ConfigurationParse</summary>
        [TestMethod]
        public void ConfigurationParseTest()
        {
            var expectedName =
                "Aristocrat.Monaco.Accounting.Contracts.CashInExceedsVoucherInLimitEvent, Monaco.Accounting.Contracts";
            var expectedLevel = "info";

            InvokeTargetMethod("ConfigurationParse");

            var eventsToSubscribe = (List<EventDescription>)GetTargetField("_eventsToSubscribe");

            Assert.AreEqual(expectedName, eventsToSubscribe[0].Name);
            Assert.AreEqual(expectedLevel, eventsToSubscribe[1].Level);
            Assert.AreEqual(6, eventsToSubscribe.Count);
        }

        /// <summary>A test for LogToPersistence</summary>
        /// <remarks>
        ///     This is mainly to test that when the string is persisted it is using the system clock time.
        ///     On the actual client this would be the GMT time. It will be converted to the time zone correct
        ///     time before it is displayed but is persisted as based on the raw system clock time.
        /// </remarks>
        [TestMethod]
        public void LogToPersistenceTest()
        {
            SetupPersistence();
            InvokeTargetMethod("ConfigurationParse");
            SetTargetStaticProperty("MaximumEvents", 1);

            var eventArray = new string[1];
            SetupAccessorProperties(eventArray);
            var eventName = "TestEvent";

            SetupTransactionSets(
                0,
                eventName + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);
            InvokeTargetMethod("QueueEvent", new TiltLoggerEvent(eventName));
            InvokeTargetMethod("LogToPersistence");

            var actual = _backingData[0];

            // Get the event field delimiter from the TiltLogger class in case it changes in the future we won't
            // have to update this unit test
            var delimiter = (string)GetTargetStaticField("EventStringDelimiter");

            var expected = string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}{2}{1}{3}",
                eventName,
                delimiter,
                "info",
                _expectedTimeValue);

            Assert.AreEqual(expected, actual);

            MoqServiceManager.RemoveService<IPersistentStorageManager>();
            InvokeTargetMethod("QueueEvent", new TiltLoggerEvent(eventName));
            InvokeTargetMethod("LogToPersistence");
        }

        /// <summary>A test for LogToPersistence with remove oldest event.</summary>
        [TestMethod]
        public void LogToPersistenceWithRemoveOldestEventTest()
        {
            _backingIndex = _oneHundredEvents.Length - 1;
            SetupPersistence();
            SetupAccessorProperties(_oneHundredEvents);
            InvokeTargetMethod("ConfigurationParse");
            SetTargetStaticProperty("MaximumEvents", 100);

            var eventName = "TestEvent";

            SetupTransactionSets(
                0,
                eventName + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);

            InvokeTargetMethod("QueueEvent", new TiltLoggerEvent(eventName));
            InvokeTargetMethod("LogToPersistence");

            Assert.IsTrue(_target.GetEvents().First().Name.Contains(eventName));
        }

        /// <summary>A test for LogToPersistence with rollover.</summary>
        [TestMethod]
        public void LogToPersistenceRolloverTest()
        {
            _backingIndex = 97;
            SetupPersistence();
            SetupAccessorProperties(_oneHundredEvents);
            InvokeTargetMethod("ConfigurationParse");
            SetTargetStaticProperty("MaximumEvents", 100);

            var expected98 = "TestEvent98";
            var expected99 = "TestEvent99";
            var expected0 = "TestEvent0";
            var expected1 = "TestEvent1";

            SetupTransactionSets(
                98,
                expected98 + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);
            SetupTransactionSets(
                99,
                expected99 + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);
            SetupTransactionSets(
                0,
                expected0 + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);
            SetupTransactionSets(
                1,
                expected1 + EventStringDelimiter + "info" + EventStringDelimiter + _expectedTimeValue);

            InvokeTargetMethod("QueueEvent", new TiltLoggerEvent(expected98));
            InvokeTargetMethod("QueueEvent", new TiltLoggerEvent(expected99));
            InvokeTargetMethod("QueueEvent", new TiltLoggerEvent(expected0));
            InvokeTargetMethod("QueueEvent", new TiltLoggerEvent(expected1));
            InvokeTargetMethod("LogToPersistence");
            var eventList = _target.GetEvents().ToList();

            Assert.IsTrue(eventList[3].Name.Contains(expected98));
            Assert.IsTrue(eventList[2].Name.Contains(expected99));
            Assert.IsTrue(eventList[1].Name.Contains(expected0));
            Assert.IsTrue(eventList[0].Name.Contains(expected1));
        }

        /// <summary>A test for TimeUpdated.</summary>
        [TestMethod]
        public void TimeUpdatedTest()
        {
            SetupPersistence();
            var eventArray = new string[MaxEventsReceived];
            var dateTime = DateTime.UtcNow;
            eventArray[0] = "string1" + EventStringDelimiter + "string2" + EventStringDelimiter + dateTime;

            SetupAccessorProperties(eventArray);

            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false))
                .Returns(dateTime.ToString("G", CultureInfo.CurrentCulture));

            _time.Setup(mock => mock.FormatDateTimeString(It.IsAny<DateTime>(), false))
                .Returns(dateTime.ToString("G", CultureInfo.CurrentCulture));

            _time.Setup(mock => mock.GetFormattedLocationTime(It.IsAny<DateTime>()))
                .Returns(dateTime.ToString("G", CultureInfo.CurrentCulture));

            InvokeTargetMethod("ConfigurationParse");

            // For this test to work, the instance has to have the same idea about max event count as we do.
            _target.GetType().GetProperty("MaximumEvents")?.SetValue(_target, MaxEventsReceived);

            for (var index = 0; index <= MaxEventsReceived; index++)
            {
                var nextIndex = (index + 1) % MaxEventsReceived;
                _persistentStorageTransaction
                    .SetupSet(mock => mock[typeof(TiltLogger).Name + "Current", "Index"] = nextIndex)
                    .Callback(
                        (string s, string s2, object o) =>
                        {
                            _persistentStorageAccessorIndex.Setup(mock => mock["Index"]).Returns(o);
                            _persistentStorageAccessor.Setup(mock => mock[It.IsAny<int>(), "Events"])
                                .Returns(eventArray[(int)o]);
                            _persistentStorageTransaction
                                .Setup(mock => mock[typeof(TiltLogger).Name + "Current", "Index"]).Returns(o);
                            _persistentStorageTransaction
                                .Setup(mock => mock[typeof(TiltLogger).Name + "Format", It.IsAny<int>(), "Events"])
                                .Returns(eventArray[(int)o]);
                        });

                _persistentStorageTransaction.SetupSet(
                        mock => mock[typeof(TiltLogger).Name + "Format", nextIndex, "Events"] =
                            "ServiceAddedEvent" + EventStringDelimiter + "" + EventStringDelimiter +
                            dateTime.ToString("G", CultureInfo.CurrentCulture))
                    .Callback(
                        (string s, int i, string s2, object o) => { eventArray[i] = (string)o; });

                var saedata = new ServiceAddedEvent();
                InvokeTargetMethod("ReceiveEvent", saedata);
            }

            InvokeTargetMethod("ProcessEvents");

            var eventsReceived = _target.GetEvents().ToList();
            Assert.AreEqual(MaxEventsReceived, eventsReceived.Count);
        }

        /// <summary>A test for GetEventList when the MaximumEvents to display is more than persistent storage has allocated.</summary>
        [TestMethod]
        [ExpectedException(
            typeof(ArgumentOutOfRangeException),
            "MaximumEvents is more than the capacity of the events.")]
        public void GetEventListTooManyEventsToDisplayTest()
        {
            SetupAccessorProperties(_oneHundredEvents);

            SetupPersistence();

            SetTargetStaticProperty("MaximumEvents", 1000);
            _target.GetEvents();
        }

        /// <summary>
        ///     A test for GetEventList with parameters when the MaximumEvents to display is more than persistent storage has
        ///     allocated.
        /// </summary>
        [TestMethod]
        [ExpectedException(
            typeof(ArgumentOutOfRangeException),
            "MaximumEvents is more than the capacity of the events.")]
        public void GetEventListWithParametersTooManyEventsToDisplayTest()
        {
            SetupAccessorProperties(_oneHundredEvents);

            SetupPersistence();

            SetTargetStaticProperty("MaximumEvents", 1000);
            _target.GetEvents(null, 1000);
        }

        /// <summary>A test for ReceiveEvent with a HardwareError event but not for the note acceptor</summary>
        [TestMethod]
        public void ReceiveEventHardwareErrorNotNoteAcceptorTest()
        {
            SetupIndexToWriteIndexZero();
            SetupAccessorProperties(_oneHundredEvents);
            InvokeTargetMethod("ConfigurationParse");

            SetupPersistence();

            var expectedName = "Note Acceptor Note Acceptor Mechanical Fault";
            SetupTransactionSets(
                0,
                expectedName + EventStringDelimiter + "error" + EventStringDelimiter + _expectedTimeValue);

            InvokeTargetMethod("ReceiveEvent", new HardwareFaultEvent(NoteAcceptorFaultTypes.MechanicalFault));
            InvokeTargetMethod("ProcessEvents");

            SetTargetStaticProperty("MaximumEvents", 100);
            Assert.AreEqual(100, _target.GetEvents().Count());
            Assert.AreEqual(expectedName, _target.GetEvents().First().Name);
        }

        /// <summary>A test for ReceiveEvent with a HardwareErrorClear event but not for the note acceptor</summary>
        [TestMethod]
        public void ReceiveEventHardwareErrorClearNotNoteAcceptorTest()
        {
            SetupIndexToWriteIndexZero();
            SetupAccessorProperties(_oneHundredEvents);
            InvokeTargetMethod("ConfigurationParse");

            SetupPersistence();

            var expectedName = "Note Acceptor Note Acceptor Mechanical Fault Cleared";
            SetupTransactionSets(
                0,
                expectedName + EventStringDelimiter + "error" + EventStringDelimiter + _expectedTimeValue);

            InvokeTargetMethod(
                "ReceiveEvent",
                new HardwareFaultClearEvent(NoteAcceptorFaultTypes.MechanicalFault));
            InvokeTargetMethod("ProcessEvents");

            SetTargetStaticProperty("MaximumEvents", 100);
            Assert.AreEqual(100, _target.GetEvents().Count());
            Assert.AreEqual(expectedName, _target.GetEvents().First().Name);
        }

        /// <summary>A test for LogCriticalMemoryIntegrityCheck with failed integrity.</summary>
        [TestMethod]
        public void LogCriticalMemoryIntegrityCheckFailTest()
        {
            SetupIndexToWriteIndexZero();
            SetupPersistence();
            _persistentStorageManager.Setup(m => m.VerifyIntegrity(true)).Returns(false);

            SetupAccessorProperties(_oneHundredEvents);
            SetupTransactionSets(
                0,
                "NVRAM Integrity check - Failed." + EventStringDelimiter + "info" + EventStringDelimiter +
                _expectedTimeValue);

            InvokeTargetMethod("ConfigurationParse");
            InvokeTargetMethod("LogCriticalMemoryIntegrityCheck");
            InvokeTargetMethod("ProcessEvents");
            SetTargetStaticProperty("MaximumEvents", 100);

            Assert.AreEqual(100, _target.GetEvents().Count());
            Assert.IsTrue(_target.GetEvents().First().Name.Contains(" - Failed."));
        }

        /// <summary>A test for FormatTime with no time service</summary>
        [TestMethod]
        public void FormatTimeNoTimeServiceTest()
        {
            MoqServiceManager.RemoveService<ITime>();

            var actual = (string)InvokeTargetMethod("FormatTime", TiltTimestamp);

            Assert.AreEqual("7/22/2012 7:45:00 PM", actual);
        }

        /// <summary>
        ///     A test for CreatePersistence with new blocks.
        /// </summary>
        [TestMethod]
        public void CreatePersistenceInitialTest()
        {
            var blockName = typeof(TiltLogger).Name + "Format";
            var blockIndexName = typeof(TiltLogger).Name + "Current";

            _persistentStorageManager
                .Setup(mock => mock.CreateBlock(It.IsAny<PersistenceLevel>(), blockIndexName, It.IsAny<int>()))
                .Returns(_persistentStorageAccessorIndex.Object).Verifiable();
            _persistentStorageManager
                .Setup(mock => mock.CreateBlock(It.IsAny<PersistenceLevel>(), blockName, It.IsAny<int>()))
                .Returns(_persistentStorageAccessor.Object).Verifiable();
            _persistentStorageAccessorIndex.SetupSet(mock => mock["Index"] = -1).Verifiable();

            _persistentStorageManager.Setup(mock => mock.BlockExists(blockName)).Returns(false);
            _persistentStorageManager.Setup(mock => mock.BlockExists(blockIndexName)).Returns(false);

            InvokeTargetMethod("CreatePersistence");

            _persistentStorageManager.VerifyAll();
        }


        /// <summary>Calls back when the log is appended.</summary>
        /// <param name="sender">The source of callback.</param>
        /// <param name="e">The argument carried with the call.</param>
        private void TiltLogAppended(object sender, TiltLogAppendedEventArgs e)
        {
            // Do nothing.  Used in a test.
        }

        /// <summary>
        ///     Sets up the pesistence storage block.
        /// </summary>
        private void SetupPersistence()
        {
            var blockName = typeof(TiltLogger).Name + "Format";
            var blockIndexName = typeof(TiltLogger).Name + "Current";
            _persistentStorageManager.Setup(mock => mock.BlockExists(blockName)).Returns(true);
            _persistentStorageManager.Setup(mock => mock.BlockExists(blockIndexName)).Returns(true);
            _persistentStorageManager.Setup(mock => mock.GetBlock(blockName))
                .Returns(_persistentStorageAccessor.Object);
            _persistentStorageManager.Setup(mock => mock.GetBlock(blockIndexName))
                .Returns(_persistentStorageAccessorIndex.Object);
            _persistentStorageAccessor.Setup(mock => mock.StartTransaction())
                .Returns(_persistentStorageTransaction.Object);

            _persistentStorageAccessor.Setup(mock => mock.Commit()).Verifiable();
            _persistentStorageAccessor.Setup(mock => mock.StartUpdate(true)).Returns(true).Verifiable();
        }

        /// <summary>
        ///     Invokes the specified target class, non-static method on the target object
        /// </summary>
        /// <param name="methodName">The name of the method to invoke</param>
        /// <param name="parameters">The optional method parameters</param>
        /// <returns>The value returned by the invoked method, or null if the method does not return a value</returns>
        private object InvokeTargetMethod(string methodName, params object[] parameters)
        {
            return _targetPrivateObject.Invoke(methodName, BindingFlags.NonPublic | BindingFlags.Instance, parameters);
        }

        /// <summary>
        ///     Sets the target object's non-public, non-static field to the supplied value
        /// </summary>
        /// <param name="fieldName">The name of the field to set</param>
        /// <param name="value">The new value for the field</param>
        private void SetTargetField(string fieldName, object value)
        {
            _targetPrivateObject.SetFieldOrProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance, value);
        }

        /// <summary>
        ///     Returns the value of a non-public, non-static field in the target object's class.
        /// </summary>
        /// <param name="fieldName">The name of the field to retrieve</param>
        /// <returns>The value of the field</returns>
        private object GetTargetField(string fieldName)
        {
            return _targetPrivateObject.GetFieldOrProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        ///     Returns the value of a non-public, static or const field in the target object's class.
        /// </summary>
        /// <param name="fieldName">The name of the field to retrieve</param>
        /// <returns>The value of the field</returns>
        private object GetTargetStaticField(string fieldName)
        {
            return _targetPrivateObject.GetFieldOrProperty(
                fieldName,
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        }

        /// <summary>
        ///     Sets the target object's non-public, non-static field to the supplied value
        /// </summary>
        /// <param name="fieldName">The name of the field to set</param>
        /// <param name="value">The new value for the field</param>
        private void SetTargetStaticProperty(string fieldName, object value)
        {
            typeof(TiltLogger).GetProperty(fieldName)?.SetValue(null, value);
        }

        private void SetupAccessorProperties(IList<string> backingData)
        {
            var blockName = typeof(TiltLogger).Name + "Format";
            var blockIndexName = typeof(TiltLogger).Name + "Current";

            _backingData.Clear();
            foreach (var data in backingData)
            {
                _backingData.Add(data);
            }

            _persistentStorageAccessorIndex.Setup(mock => mock["Index"]).Returns(_backingIndex);
            _persistentStorageAccessor.Setup(mock => mock[It.IsAny<int>(), "Events"])
                .Returns((int i, string s) => _backingData[i]);
            _persistentStorageTransaction.Setup(mock => mock[blockIndexName, "Index"]).Returns(_backingIndex);
            _persistentStorageTransaction.Setup(mock => mock[blockName, It.IsAny<int>(), "Events"])
                .Returns((string name, int i, string field) => _backingData[i]);
            _persistentStorageTransaction.Setup(mock => mock.AddBlock(It.IsAny<IPersistentStorageAccessor>()));

            for (var i = 0; i < _backingData.Count; i++)
            {
                SetupTransactionSets(i, _backingData[i]);
            }
        }

        private void SetupTransactionSets(int index, string value = "")
        {
            var blockName = typeof(TiltLogger).Name + "Format";
            var blockIndexName = typeof(TiltLogger).Name + "Current";

            _persistentStorageTransaction.SetupSet(mock => mock[blockIndexName, "Index"] = index).Callback(
                () =>
                {
                    _backingIndex = index;
                    _persistentStorageAccessorIndex.Setup(mock => mock["Index"]).Returns(index);
                    _persistentStorageAccessor.Setup(mock => mock[index, "Events"])
                        .Returns((int i, string s) => _backingData[i]);
                });

            _persistentStorageTransaction.SetupSet(mock => mock[blockName, index, "Events"] = value).Callback(
                () =>
                {
                    _backingData[index] = value;
                    _persistentStorageAccessor.Setup(mock => mock[index, "Events"]).Returns((int i, string s) => value);
                });
        }

        /// <summary>
        ///     Sets _backingIndex to maximum so that LogToPersistence will start writing at index 0
        /// </summary>
        private void SetupIndexToWriteIndexZero()
        {
            // Make sure first set index is 0
            var configurationNodes =
                MonoAddinsHelper.GetSelectedNodes<TiltLoggerConfigurationExtensionNode>(
                    (string)GetTargetField("_tiltLoggerConfigurationExtensionPath"));
            _backingIndex = Convert.ToInt32(
                                MonoAddinsHelper.GetChildNodes<PropertyNode>(configurationNodes.First()).First()
                                    .PropertyValue) - 1;
        }
        */
    }
}
