namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Linq.Expressions;
    using Application.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;
    using Data.EventHandler;
    using Data.Model;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class EventPersistenceManagerTest
    {
        private readonly Mock<IMonacoContextFactory> _contextFactoryMock = new Mock<IMonacoContextFactory>();

        private readonly Mock<IEventHandlerLogRepository> _eventHandlerLogRepoMock =
            new Mock<IEventHandlerLogRepository>();

        private readonly Mock<IEventSubscriptionRepository> _eventSubscriptionRepoMock =
            new Mock<IEventSubscriptionRepository>();

        private readonly Mock<IIdProvider> _idProvider = new Mock<IIdProvider>();

        private readonly Mock<ISupportedEventRepository> _supportedEventsRepoMock =
            new Mock<ISupportedEventRepository>();

        [TestInitialize]
        public void TestInitialization()
        {
            var log = new EventHandlerLog
            {
                HostId = 1,
                DeviceId = 1,
                EventId = 1,
                Id = 1
            };
            var supportedEvent = new SupportedEvent
            {
                DeviceClass = "G2S_test",
                DeviceId = 1,
                Id = 1,
                EventCode = EventCode.G2S_APE001
            };
            var eventSubscription = new EventSubscription
            {
                HostId = 1,
                DeviceId = 1,
                EventCode = EventCode.G2S_APE001,
                Id = 1,
                SubType = EventSubscriptionType.Host
            };
            var forcedSubscription = new EventSubscription
            {
                HostId = 1,
                DeviceId = 1,
                EventCode = EventCode.G2S_APE001,
                Id = 1,
                SubType = EventSubscriptionType.Forced
            };
            var logs = new List<EventHandlerLog> { log };
            var supportedEvents = new List<SupportedEvent> { supportedEvent };
            var eventSubscriptions = new List<EventSubscription> { eventSubscription, forcedSubscription };

            _contextFactoryMock.Setup(a => a.CreateDbContext()).Returns(new MonacoContext("TestConnection"));
            _eventHandlerLogRepoMock.Setup(a => a.Get(It.IsAny<DbContext>(), 1, 1))
                .Returns(log);
            _supportedEventsRepoMock.Setup(a => a.Get(It.IsAny<DbContext>(), EventCode.G2S_APE001, 1))
                .Returns(supportedEvent);
            _eventSubscriptionRepoMock.Setup(a => a.Get(It.IsAny<DbContext>(), EventCode.G2S_APE001, 1, 1))
                .Returns(eventSubscription);
            _eventHandlerLogRepoMock.Setup(
                    a => a.Count(It.IsAny<DbContext>(), It.IsAny<Expression<Func<EventHandlerLog, bool>>>()))
                .Returns(1);

            _eventHandlerLogRepoMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(logs.AsQueryable());
            _supportedEventsRepoMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(supportedEvents.AsQueryable());
            _eventSubscriptionRepoMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(eventSubscriptions.AsQueryable());

            _idProvider.Setup(a => a.GetNextLogSequence<EventHandlerLog>()).Returns(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryRepositoryExpectException()
        {
            var eventPersistence = new EventPersistenceManager(null, null, null, null, null);

            Assert.IsNull(eventPersistence);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventRepoRepositoryExpectException()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(eventPersistence);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventHandlerLogRepositoryExpectException()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(eventPersistence);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventSubscriptionRepositoryExpectException()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                null,
                null,
                null);

            Assert.IsNull(eventPersistence);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullSupportedEventsRepositoryExpectException()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                null,
                null);

            Assert.IsNull(eventPersistence);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullIdProviderExpectException()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                null);

            Assert.IsNull(eventPersistence);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);

            Assert.IsNotNull(eventPersistence);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsRemoveSupportedEventsExpectSuccess()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);
            eventPersistence.RemoveSupportedEvents(1, EventCode.G2S_APE001);
            _contextFactoryMock.Verify(a => a.CreateDbContext());
            _supportedEventsRepoMock.Verify(a => a.Delete(It.IsAny<DbContext>(), EventCode.G2S_APE001, 1));
        }

        [TestMethod]
        public void WhenConstructWithValidParamsAddDefaultEventsExpectSuccess()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);
            eventPersistence.AddDefaultEvents(1);
            _contextFactoryMock.Verify(a => a.CreateDbContext());
            _eventSubscriptionRepoMock.Verify(
                a => a.Get(It.IsAny<DbContext>(), It.IsAny<string>(), 1, 1, EventSubscriptionType.Permanent));
            _eventSubscriptionRepoMock.Verify(a => a.Add(It.IsAny<DbContext>(), It.IsAny<EventSubscription>()));
        }

        [TestMethod]
        public void WhenConstructWithValidParamsRegisteredEventsExpectSuccess()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);
            eventPersistence.RegisteredEvents(
                new List<eventHostSubscription>
                {
                    new eventHostSubscription
                    {
                        deviceClass = "G2S_test",
                        deviceId = 1,
                        eventCode = EventCode.G2S_APE001
                    }
                },
                1);
            _contextFactoryMock.Verify(a => a.CreateDbContext());
            _eventSubscriptionRepoMock.Verify(a => a.GetAll(It.IsAny<DbContext>()));
        }

        [TestMethod]
        public void WhenConstructWithValidParamsAddEventLogExpectSuccess()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);

            eventPersistence.AddEventLog(
                new eventReport
                {
                    eventId = 1,
                    deviceId = 1,
                    deviceClass = "G2S_test",
                    eventCode = EventCode.G2S_APE001,
                    transactionId = 1,
                    eventDateTime = DateTime.Now
                },
                1,
                35);
            eventPersistence.AddEventLog(
                new eventReport
                {
                    eventId = 2,
                    deviceId = 1,
                    deviceClass = "G2S_test",
                    eventCode = EventCode.G2S_APE001,
                    transactionId = 1,
                    eventDateTime = DateTime.Now
                },
                1,
                35);
            _contextFactoryMock.Verify(a => a.CreateDbContext());
            _eventHandlerLogRepoMock.Verify(a => a.Add(It.IsAny<DbContext>(), It.IsAny<EventHandlerLog>()));
        }

        [TestMethod]
        public void WhenAddForcedEventExpectSuccess()
        {
            _eventSubscriptionRepoMock.Setup(
                    a => a.Get(It.IsAny<DbContext>(), It.IsAny<string>(), 1, 1, EventSubscriptionType.Forced))
                .Returns(
                    new EventSubscription
                    {
                        DeviceId = 1,
                        EventCode = EventCode.G2S_APE001,
                        Id = 1,
                        SubType = EventSubscriptionType.Forced
                    });

            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);
            eventPersistence.AddForcedEvent(
                EventCode.G2S_APE001,
                new forcedSubscription { deviceClass = "G2S_test", deviceId = 1, eventCode = EventCode.G2S_APE001 },
                1,
                1);
            eventPersistence.AddForcedEvent(
                EventCode.G2S_APE001,
                new forcedSubscription { deviceClass = "G2S_test", deviceId = 2, eventCode = EventCode.G2S_APE001 },
                1,
                2);
            _contextFactoryMock.Verify(a => a.CreateDbContext());

            _eventSubscriptionRepoMock.Verify(
                a => a.Get(
                    It.IsAny<DbContext>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    EventSubscriptionType.Forced));
            _eventSubscriptionRepoMock.Verify(a => a.Add(It.IsAny<DbContext>(), It.IsAny<EventSubscription>()));
            _eventSubscriptionRepoMock.Verify(a => a.Update(It.IsAny<DbContext>(), It.IsAny<EventSubscription>()));
        }

        [TestMethod]
        public void WhenAddSupportedEventsExpectSuccess()
        {
            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);
            eventPersistence.AddSupportedEvents("G2S_test", 2, EventCode.G2S_APE001);
            _contextFactoryMock.Verify(a => a.CreateDbContext());

            _supportedEventsRepoMock.Verify(a => a.Get(It.IsAny<DbContext>(), It.IsAny<string>(), It.IsAny<int>()));
            _supportedEventsRepoMock.Verify(a => a.Add(It.IsAny<DbContext>(), It.IsAny<SupportedEvent>()));
        }

        [TestMethod]
        public void WhenRemoveRegisteredEventSubscriptionsExpectSuccess()
        {
            _eventSubscriptionRepoMock.Setup(
                    a => a.Get(It.IsAny<DbContext>(), EventCode.G2S_APE001, 1, 1, EventSubscriptionType.Host))
                .Returns(
                    new EventSubscription
                    {
                        HostId = 1,
                        DeviceId = 1,
                        EventCode = EventCode.G2S_APE001,
                        Id = 1,
                        SubType = EventSubscriptionType.Host
                    });

            var eventPersistence = new EventPersistenceManager(
                _contextFactoryMock.Object,
                _eventHandlerLogRepoMock.Object,
                _eventSubscriptionRepoMock.Object,
                _supportedEventsRepoMock.Object,
                _idProvider.Object);
            eventPersistence.RemoveRegisteredEventSubscriptions(
                new List<eventHostSubscription>
                {
                    new eventHostSubscription
                    {
                        deviceClass = "G2S_test",
                        deviceId = 1,
                        eventCode = EventCode.G2S_APE001
                    }
                },
                1);
            _contextFactoryMock.Verify(a => a.CreateDbContext());

            _eventSubscriptionRepoMock.Verify(
                a => a.DeleteAll(It.IsAny<DbContext>(), It.IsAny<IEnumerable<EventSubscription>>()));
        }
    }
}