namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Meters;
    using Data.Model;
    using G2S.Meters;
    using Gaming.Contracts;
    using Hardware.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class MetersSubscriptionManagerTest
    {
        private readonly Mock<IMonacoContextFactory> _contextFactoryMock = new Mock<IMonacoContextFactory>();
        private readonly Mock<IDeviceRegistryService> _deviceRegistryMock = new Mock<IDeviceRegistryService>();
        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();
        private readonly Mock<IGameProvider> _gameProviderMock = new Mock<IGameProvider>();
        private readonly Mock<IMeterManager> _meterManagerMock = new Mock<IMeterManager>();

        private readonly Mock<IMeterSubscriptionRepository> _meterSubscriptionRepositoryMock =
            new Mock<IMeterSubscriptionRepository>();

        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>();
        private readonly Mock<ITaskScheduler> _taskSchedulerMock = new Mock<ITaskScheduler>();

        [TestInitialize]
        public void TestInitialization()
        {
            _contextFactoryMock.Setup(a => a.Create()).Returns(new DbContext("TestConnection"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var manager = new MetersSubscriptionManager(
                null,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            Assert.IsNull(manager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryException()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                null,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            Assert.IsNull(manager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMeterSubscriptionRepositoryException()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                null,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            Assert.IsNull(manager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullTaskSchedulerException()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                null,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            Assert.IsNull(manager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMeterManagerException()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                null,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            Assert.IsNull(manager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderException()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                null,
                _properties.Object,
                _deviceRegistryMock.Object);

            Assert.IsNull(manager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertiesException()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                null,
                _deviceRegistryMock.Object);

            Assert.IsNull(manager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDeviceRegistryException()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                null);

            Assert.IsNull(manager);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsGetsExpectSuccess()
        {
            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            Assert.IsTrue(manager.WageMeters.Count > 0);
            Assert.IsTrue(manager.CurrencyMeters.Count > 0);
            Assert.IsTrue(manager.GameMeters.Count > 0);
            Assert.IsTrue(manager.DeviceMeters.Count > 0);
        }

        [TestMethod]
        public void WhenHandleMeterReportExpectSuccess()
        {
            _meterSubscriptionRepositoryMock.Setup(a => a.Get(It.IsAny<DbContext>(), 1))
                .Returns(
                    new MeterSubscription()
                    {
                        Base = 1,
                        DeviceId = 1,
                        Id = 1,
                        SubType = MetersSubscriptionType.EndOfDay,
                        ClassName = "G2S_test",
                        HostId = 1,
                        MeterDefinition = false,
                        MeterType = MeterType.Currency,
                        OnEndOfDay = true
                    });

            _meterSubscriptionRepositoryMock.Setup(a => a.Get(It.IsAny<DbContext>(), 1))
                .Returns(
                    new MeterSubscription()
                    {
                        Base = 2,
                        DeviceId = 1,
                        Id = 2,
                        SubType = MetersSubscriptionType.Periodic,
                        ClassName = "G2S_test",
                        HostId = 1,
                        MeterDefinition = false,
                        MeterType = MeterType.Currency,
                        OnEndOfDay = false,
                        PeriodInterval = 100
                    });

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            manager.HandleMeterReport(1);
            manager.HandleMeterReport(2);

            _contextFactoryMock.Verify(a => a.Create());
            _meterSubscriptionRepositoryMock.Verify(a => a.Get(It.IsAny<DbContext>(), 1));
            _meterSubscriptionRepositoryMock.Verify(a => a.Get(It.IsAny<DbContext>(), 2));
            _taskSchedulerMock.Verify(
                a => a.ScheduleTask(It.IsAny<MeterReportJob>(), It.IsAny<string>(), It.IsAny<DateTime>()));

            _egmMock.Verify(a => a.GetDevice<IMetersDevice>(1));
            _meterManagerMock.Verify(a => a.CreateSnapshot());
        }

        [TestMethod]
        public void WhenHandleSendEndOfDayMetersExpectSuccess()
        {
            _meterSubscriptionRepositoryMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<MeterSubscription>()
                    {
                        new MeterSubscription()
                        {
                            Base = 1,
                            DeviceId = 1,
                            Id = 1,
                            SubType = MetersSubscriptionType.EndOfDay,
                            ClassName = "G2S_test",
                            HostId = 1,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnNoteDrop = true,
                            OnDoorOpen = true,
                            OnEndOfDay = true
                        }
                    }.AsQueryable());

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            manager.SendEndOfDayMeters(true, true);

            _contextFactoryMock.Verify(a => a.Create());
            _meterSubscriptionRepositoryMock.Verify(a => a.GetAll(It.IsAny<DbContext>()));
            _egmMock.Verify(a => a.GetDevice<IMetersDevice>(1));
            _meterManagerMock.Verify(a => a.CreateSnapshot());
        }

        [TestMethod]
        public void WhenHandleStartExpectSuccess()
        {
            _meterSubscriptionRepositoryMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<MeterSubscription>()
                    {
                        new MeterSubscription()
                        {
                            Base = 100000,
                            DeviceId = 1,
                            Id = 1,
                            SubType = MetersSubscriptionType.EndOfDay,
                            ClassName = "G2S_test",
                            HostId = 1,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnNoteDrop = true,
                            OnDoorOpen = true,
                            OnEndOfDay = true
                        },
                        new MeterSubscription()
                        {
                            Base = 200000,
                            DeviceId = 1,
                            Id = 2,
                            SubType = MetersSubscriptionType.Periodic,
                            ClassName = "G2S_test",
                            HostId = 2,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnEndOfDay = false,
                            PeriodInterval = 100
                        }
                    }.AsQueryable());

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            manager.Start();

            _contextFactoryMock.Verify(a => a.Create());
            _meterSubscriptionRepositoryMock.Verify(a => a.GetAll(It.IsAny<DbContext>()));
            _taskSchedulerMock.Verify(
                a => a.ScheduleTask(It.IsAny<MeterReportJob>(), It.IsAny<string>(), It.IsAny<DateTime>()));
        }

        [TestMethod]
        public void WhenHandleGetMetersExpectSuccess()
        {
            _meterManagerMock.Setup(a => a.CreateSnapshot()).Returns(new Dictionary<string, MeterSnapshot>());

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);
            var meters = new meterInfo();

            manager.GetMeters(
                new getMeterInfo
                {
                    getCurrencyMeters = new []
                        { new getCurrencyMeters { deviceClass = "G2S_all", deviceId = -1, meterDefinitions = true } },
                    getDeviceMeters = new []
                        { new getDeviceMeters { deviceClass = "G2S_all", deviceId = -1, meterDefinitions = false } },
                    getGameDenomMeters = new []
                    {
                        new getGameDenomMeters { deviceClass = "G2S_all", deviceId = -1, meterDefinitions = true }
                    },
                    getWagerMeters = new[]
                        { new getWagerMeters { deviceClass = "G2S_all", deviceId = -1, meterDefinitions = true } }
                },
                meters);

            _meterManagerMock.Verify(a => a.CreateSnapshot());
            Assert.IsNotNull(meters.currencyMeters);
            Assert.IsNotNull(meters.deviceMeters);
            Assert.IsNotNull(meters.gameDenomMeters);
            Assert.IsNotNull(meters.wagerMeters);
        }

        [TestMethod]
        public void WhenHandleGetMeterSubExpectSuccess()
        {
            _meterSubscriptionRepositoryMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<MeterSubscription>()
                    {
                        new MeterSubscription()
                        {
                            Base = 100000,
                            DeviceId = 1,
                            Id = 1,
                            SubType = MetersSubscriptionType.EndOfDay,
                            ClassName = "G2S_test",
                            HostId = 1,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnNoteDrop = true,
                            OnDoorOpen = true,
                            OnEndOfDay = true
                        },
                        new MeterSubscription()
                        {
                            Base = 200000,
                            DeviceId = 1,
                            Id = 2,
                            SubType = MetersSubscriptionType.Periodic,
                            ClassName = "G2S_test",
                            HostId = 2,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnEndOfDay = false,
                            PeriodInterval = 100
                        }
                    }.AsQueryable());

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            manager.GetMeterSub(1, MetersSubscriptionType.EndOfDay);

            _contextFactoryMock.Verify(a => a.Create());
            _meterSubscriptionRepositoryMock.Verify(a => a.GetAll(It.IsAny<DbContext>()));
        }

        [TestMethod]
        public void WhenHandleClearSubscriptionsExpectSuccess()
        {
            _meterSubscriptionRepositoryMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<MeterSubscription>()
                    {
                        new MeterSubscription()
                        {
                            Base = 100000,
                            DeviceId = 1,
                            Id = 1,
                            SubType = MetersSubscriptionType.EndOfDay,
                            ClassName = "G2S_test",
                            HostId = 1,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnNoteDrop = true,
                            OnDoorOpen = true,
                            OnEndOfDay = true
                        },
                        new MeterSubscription()
                        {
                            Base = 200000,
                            DeviceId = 1,
                            Id = 2,
                            SubType = MetersSubscriptionType.Periodic,
                            ClassName = "G2S_test",
                            HostId = 2,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnEndOfDay = false,
                            PeriodInterval = 100
                        }
                    }.AsQueryable());

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            manager.ClearSubscriptions(1, MetersSubscriptionType.EndOfDay);

            _contextFactoryMock.Verify(a => a.Create());
            _meterSubscriptionRepositoryMock.Verify(a => a.GetAll(It.IsAny<DbContext>()));
            _meterSubscriptionRepositoryMock.Verify(
                a => a.Delete(It.IsAny<DbContext>(), It.IsAny<MeterSubscription>()));
        }

        [TestMethod]
        public void WhenHandleSetMetersSubscriptionExpectSuccess()
        {
            _meterSubscriptionRepositoryMock.Setup(a => a.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<MeterSubscription>()
                    {
                        new MeterSubscription()
                        {
                            Base = 100000,
                            DeviceId = 1,
                            Id = 1,
                            SubType = MetersSubscriptionType.EndOfDay,
                            ClassName = "G2S_test",
                            HostId = 1,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnNoteDrop = true,
                            OnDoorOpen = true,
                            OnEndOfDay = true
                        },
                        new MeterSubscription()
                        {
                            Base = 200000,
                            DeviceId = 1,
                            Id = 2,
                            SubType = MetersSubscriptionType.Periodic,
                            ClassName = "G2S_test",
                            HostId = 2,
                            MeterDefinition = false,
                            MeterType = MeterType.Currency,
                            OnEndOfDay = false,
                            PeriodInterval = 100
                        }
                    }.AsQueryable());

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            manager.SetMetersSubscription(
                1,
                MetersSubscriptionType.EndOfDay,
                new List<MeterSubscription>()
                {
                    new MeterSubscription()
                    {
                        Base = 100000,
                        DeviceId = 1,
                        Id = 1,
                        SubType = MetersSubscriptionType.EndOfDay,
                        ClassName = "G2S_test",
                        HostId = 1,
                        MeterDefinition = false,
                        MeterType = MeterType.Currency,
                        OnNoteDrop = true,
                        OnDoorOpen = true,
                        OnEndOfDay = true
                    },
                    new MeterSubscription()
                    {
                        Base = 200000,
                        DeviceId = 2,
                        Id = 2,
                        SubType = MetersSubscriptionType.EndOfDay,
                        ClassName = "G2S_test",
                        HostId = 2,
                        MeterDefinition = false,
                        MeterType = MeterType.Currency,
                        OnEndOfDay = false,
                        PeriodInterval = 100
                    }
                });
            _contextFactoryMock.Verify(a => a.Create());
            _meterSubscriptionRepositoryMock.Verify(a => a.GetAll(It.IsAny<DbContext>()));
            _meterSubscriptionRepositoryMock.Verify(
                a => a.Delete(It.IsAny<DbContext>(), It.IsAny<MeterSubscription>()));
            _meterSubscriptionRepositoryMock.Verify(a => a.Add(It.IsAny<DbContext>(), It.IsAny<MeterSubscription>()));
            _taskSchedulerMock.Verify(
                a => a.ScheduleTask(It.IsAny<MeterReportJob>(), It.IsAny<string>(), It.IsAny<DateTime>()));
        }

        [TestMethod]
        public void WhenHandleSetHandleMeterReportExpectSuccess()
        {
            _meterSubscriptionRepositoryMock.Setup(a => a.Get(It.IsAny<DbContext>(), 1))
                .Returns(
                    new MeterSubscription()
                    {
                        Base = 100000,
                        DeviceId = 1,
                        Id = 1,
                        SubType = MetersSubscriptionType.EndOfDay,
                        ClassName = "G2S_test",
                        HostId = 1,
                        MeterDefinition = false,
                        MeterType = MeterType.Currency,
                        OnNoteDrop = true,
                        OnDoorOpen = true,
                        OnEndOfDay = true
                    });

            var manager = new MetersSubscriptionManager(
                _egmMock.Object,
                _contextFactoryMock.Object,
                _meterSubscriptionRepositoryMock.Object,
                _taskSchedulerMock.Object,
                _meterManagerMock.Object,
                _gameProviderMock.Object,
                _properties.Object,
                _deviceRegistryMock.Object);

            manager.HandleMeterReport(1);

            _contextFactoryMock.Verify(a => a.Create());
            _meterSubscriptionRepositoryMock.Verify(a => a.Get(It.IsAny<DbContext>(), 1));
            _taskSchedulerMock.Verify(
                a => a.ScheduleTask(It.IsAny<MeterReportJob>(), It.IsAny<string>(), It.IsAny<DateTime>()));
            _egmMock.Verify(a => a.GetDevice<IMetersDevice>(1));
            _meterManagerMock.Verify(a => a.CreateSnapshot());
        }
    }
}
