namespace Aristocrat.Monaco.Application.Tests
{
    using Contracts;
    using Contracts.Operations;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Test.Common;

    [TestClass]
    public class OperatingHoursMonitorTest
    {
        private Mock<IEventBus> _eventBus;
        private OperatingHoursMonitor _monitor;

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<PropertyChangedEvent>>(), It.IsAny<Predicate<PropertyChangedEvent>>()));
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<TimeUpdatedEvent>>()));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _monitor?.Dispose();

            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertiesManagerExpectException()
        {
            var monitor = new OperatingHoursMonitor(null, null, null, null);

            Assert.IsNull(monitor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullDisableManagerExpectException()
        {
            var properties = new Mock<IPropertiesManager>();

            var monitor = new OperatingHoursMonitor(properties.Object, null, null, null);

            Assert.IsNull(monitor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullMessageDisplayExpectException()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();

            var monitor = new OperatingHoursMonitor(properties.Object, disableManager.Object, null, null);

            Assert.IsNull(monitor);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();

            var monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                null,
                null);

            Assert.IsNull(monitor);
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();

            var monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            Assert.IsNotNull(monitor);
        }

        [TestMethod]
        public void WhenInitializeExpectSuccess()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var time = new Mock<ITime>();

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                _eventBus.Object,
                time.Object);

            _monitor.Initialize();
        }

        [TestMethod]
        public void WhenInitWithNoOperatingHoursExpectNoChange()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(DateTime.Now);
            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(Enumerable.Empty<OperatingHours>());

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();

            disableManager.Verify(d => d.Enable(ApplicationConstants.OperatingHoursDisableGuid), Times.Never);
            bus.Verify(m => m.Publish(It.IsAny<OperatingHoursEnabledEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenRunWithScheduleExpectSuccess()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(DateTime.Now);
            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(GetSampleOperatingHours());

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();
        }

        [TestMethod]
        public void WhenCurrentlyDisabledExpectDisable()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(DateTime.Now);
            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(GetSampleOperatingHours(false));

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();

            disableManager.Verify(
                d =>
                    d.Disable(
                        ApplicationConstants.OperatingHoursDisableGuid,
                        SystemDisablePriority.Normal,
                        It.IsAny<Func<string>>(),
                        false, null));
            bus.Verify(b => b.Publish(It.IsAny<OperatingHoursExpiredEvent>()));
        }

        [TestMethod]
        public void WhenCurrentlyEnabledExpectNoChange()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(DateTime.Now);
            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(GetSampleOperatingHours(true));

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();

            disableManager.Verify(d => d.Enable(ApplicationConstants.OperatingHoursDisableGuid), Times.Never);
            bus.Verify(b => b.Publish(It.IsAny<OperatingHoursEnabledEvent>()), Times.Never);
        }

        [TestMethod]
        public void WhenTransitionToEnabledExpectEnable()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(Time);
            DateTime Time()
            {
                return DateTime.Now;
            }
            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(GetSampleOperatingHours(false));

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();

            disableManager.Verify(
                d =>
                    d.Disable(
                        ApplicationConstants.OperatingHoursDisableGuid,
                        SystemDisablePriority.Normal,
                        It.IsAny<Func<string>>(),
                        false, null));

            Thread.Sleep(250);

            disableManager.Verify(d => d.Enable(ApplicationConstants.OperatingHoursDisableGuid));
            bus.Verify(b => b.Publish(It.IsAny<OperatingHoursEnabledEvent>()));
        }

        [TestMethod]
        public void WhenTransitionToDisabledExpectDisable()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(Time);
            DateTime Time()
            {
                return DateTime.Now;
            }
            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(GetSampleOperatingHours(true));

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();

            Thread.Sleep(5000);

            disableManager.Verify(
                d =>
                    d.Disable(
                        ApplicationConstants.OperatingHoursDisableGuid,
                        SystemDisablePriority.Normal,
                        It.IsAny<Func<string>>(),
                        false, null));
            bus.Verify(b => b.Publish(It.IsAny<OperatingHoursExpiredEvent>()));
        }

        [TestMethod]
        public void WhenDisabledYesterdayExpectDisable()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(DateTime.Now);
            var now = DateTime.Now;

            var yesterday = now.Date - TimeSpan.FromDays(1);

            var operatingHours = new List<OperatingHours>
            {
                new OperatingHours
                {
                    Day = yesterday.DayOfWeek,
                    Enabled = false,
                    Time = (int)(yesterday - yesterday.Date).TotalMilliseconds
                },
                new OperatingHours
                {
                    Day = now.DayOfWeek,
                    Enabled = true,
                    Time = (int)(now - now.Date + TimeSpan.FromMilliseconds(100)).TotalMilliseconds
                }
            };

            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(operatingHours);

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();

            disableManager.Verify(
                d =>
                    d.Disable(
                        ApplicationConstants.OperatingHoursDisableGuid,
                        SystemDisablePriority.Normal,
                        It.IsAny<Func<string>>(),
                        false, null));
            bus.Verify(b => b.Publish(It.IsAny<OperatingHoursExpiredEvent>()));
        }

        [TestMethod]
        public void WhenChangeTomorrowExpectSuccess()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();
            var bus = new Mock<IEventBus>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(DateTime.Now);
            var now = DateTime.Now;

            var tomorrow = now.Date + TimeSpan.FromHours(25);

            var operatingHours = new List<OperatingHours>
            {
                new OperatingHours
                {
                    Day = now.DayOfWeek,
                    Enabled = true,
                    Time = (int)(now - now.Date - TimeSpan.FromMinutes(1)).TotalMilliseconds
                },
                new OperatingHours
                {
                    Day = tomorrow.DayOfWeek,
                    Enabled = false,
                    Time = (int)(tomorrow - tomorrow.Date).TotalMilliseconds
                }
            };

            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(operatingHours);

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                bus.Object,
                time.Object);

            _monitor.Initialize();
        }

        [TestMethod]
        public void WhenHandleUnwantedPropertyChangeExpectNothing()
        {
            var properties = new Mock<IPropertiesManager>();
            var disableManager = new Mock<ISystemDisableManager>();

            var operatingHours = new List<OperatingHours>();
            var time = new Mock<ITime>();
            time.Setup(t => t.GetLocationTime()).Returns(DateTime.Now);
            properties.Setup(p => p.GetProperty(ApplicationConstants.OperatingHours, It.IsAny<object>()))
                .Returns(operatingHours);

            _monitor = new OperatingHoursMonitor(
                properties.Object,
                disableManager.Object,
                _eventBus.Object,
                time.Object);

            _monitor.Initialize();

            operatingHours.AddRange(GetSampleOperatingHours(false));

            disableManager.Verify(
                d =>
                    d.Disable(
                        ApplicationConstants.OperatingHoursDisableGuid,
                        SystemDisablePriority.Normal,
                        It.IsAny<Func<string>>(), null),
                Times.Never);
            _eventBus.Verify(b => b.Publish(It.IsAny<OperatingHoursExpiredEvent>()), Times.Never);
        }

        private static IEnumerable<OperatingHours> GetSampleOperatingHours()
        {
            var operatingHours = new List<OperatingHours>();

            // Enabled between 8:00AM and 11:00PM
            operatingHours.AddRange(
                Enumerable.Range(0, 7)
                    .Select(
                        day =>
                            new OperatingHours
                            {
                                Day = (DayOfWeek)day,
                                Time = (int)TimeSpan.FromHours(8).TotalMilliseconds,
                                Enabled = true
                            }));

            operatingHours.AddRange(
                Enumerable.Range(0, 7)
                    .Select(
                        day =>
                            new OperatingHours
                            {
                                Day = (DayOfWeek)day,
                                Time = (int)TimeSpan.FromHours(23).TotalMilliseconds,
                                Enabled = false
                            }));

            return operatingHours;
        }

        private static IEnumerable<OperatingHours> GetSampleOperatingHours(bool currentState)
        {
            var now = DateTime.Now;

            return new List<OperatingHours>
            {
                new OperatingHours
                {
                    Day = now.DayOfWeek,
                    Enabled = currentState,
                    Time = (int)(now - now.Date - TimeSpan.FromMinutes(1)).TotalMilliseconds
                },
                new OperatingHours
                {
                    Day = now.DayOfWeek,
                    Enabled = !currentState,
                    Time = (int)(now - now.Date + TimeSpan.FromMilliseconds(100)).TotalMilliseconds
                }
            };
        }
    }
}