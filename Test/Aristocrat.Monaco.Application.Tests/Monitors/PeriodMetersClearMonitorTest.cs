namespace Aristocrat.Monaco.Application.Tests.Monitors
{
    using System;
    using System.Threading;
    using Application.Monitors;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for PeriodMetersClearMonitor and is intended
    ///     to contain all PeriodMetersClearMonitor Unit Tests
    /// </summary>
    [TestClass]
    public class PeriodMetersClearMonitorTest
    {
        private const double Offset = 3D;

        private Mock<IEventBus> _eventBus;
        private Mock<IMeterManager> _meterManager;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ITime> _time;

        [TestInitialize]
        public void TestInitialization()
        {
            _eventBus = new Mock<IEventBus>();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<TimeUpdatedEvent>>()));

            _meterManager = new Mock<IMeterManager>();

            _time = new Mock<ITime>();
            _time.Setup(m => m.GetLocationTime(It.IsAny<DateTime>())).Returns((DateTime t) => t);

            _propertiesManager = new Mock<IPropertiesManager>();
            _propertiesManager
                .Setup(a => a.GetProperty(ApplicationConstants.AutoClearPeriodMetersText, It.IsAny<bool>()))
                .Returns(true);

            _propertiesManager
                .Setup(a => a.GetProperty(ApplicationConstants.ClearClearPeriodOffsetHoursText, 0D))
                .Returns(() => Offset);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void WhenInitializeExpectSuccess()
        {
            _meterManager.SetupGet(a => a.LastPeriodClear).Returns(DateTime.Now.AddMinutes(1));
            _time.Setup(a => a.GetLocationTime()).Returns(() => DateTime.Now.AddMinutes(1));
            var monitor = new PeriodMetersClearMonitor(
                _eventBus.Object,
                _meterManager.Object,
                _time.Object,
                _propertiesManager.Object);
            monitor.Initialize();
            monitor.Dispose();
        }

        [TestMethod]
        public void WhenOnInitializeAndAlreadyFiredExpectNoChange()
        {
            _meterManager.SetupGet(a => a.LastPeriodClear).Returns(DateTime.Now.AddMinutes(1));
            _time.Setup(a => a.GetLocationTime()).Returns(() => DateTime.Now.AddMinutes(1));
            _time.Setup(a => a.GetLocationTime(It.IsAny<DateTime>())).Returns(() => DateTime.Now.AddMinutes(1));

            var monitor = new PeriodMetersClearMonitor(
                _eventBus.Object,
                _meterManager.Object,
                _time.Object,
                _propertiesManager.Object);
            monitor.Initialize();
            monitor.Dispose();

            _meterManager.Verify(a => a.ClearAllPeriodMeters(), Times.Exactly(0));
        }

        [TestMethod]
        [Ignore]
        public void WhenOnInitializeAndNotFiredExpectClearAllPeriodMeters()
        {
            var updated = false;
            _meterManager.SetupGet(a => a.LastPeriodClear).Returns(
                () =>
                {
                    if (!updated)
                    {
                        updated = true;
                        return DateTime.Now.AddMinutes(-1);
                    }

                    return DateTime.Now;
                });
            _time.Setup(a => a.GetLocationTime()).Returns(() => DateTime.Now.AddMinutes(1));

            var monitor = new PeriodMetersClearMonitor(
                _eventBus.Object,
                _meterManager.Object,
                _time.Object,
                _propertiesManager.Object);
            monitor.Initialize();
            monitor.Dispose();

            _meterManager.Verify(a => a.ClearAllPeriodMeters(), Times.Exactly(1));
        }

        [TestMethod]
        [Ignore]
        public void WhenOnInitializeAndMissedLastNotFiredExpectClearAllPeriodMeters()
        {
            var updated = false;
            _meterManager.SetupGet(a => a.LastPeriodClear).Returns(
                () =>
                {
                    if (!updated)
                    {
                        updated = true;
                        return DateTime.Now.AddMinutes(-1);
                    }

                    return DateTime.Now;
                });
            _time.Setup(a => a.GetLocationTime()).Returns(() => DateTime.Now);

            var monitor = new PeriodMetersClearMonitor(
                _eventBus.Object,
                _meterManager.Object,
                _time.Object,
                _propertiesManager.Object);
            monitor.Initialize();

            monitor.Dispose();

            Thread.Sleep(100);

            _meterManager.Verify(a => a.ClearAllPeriodMeters(), Times.Exactly(1));
        }

        [TestMethod]
        public void WhenIntervalExpiredAndAlreadyFiredExpectNoChange()
        {
            _propertiesManager
                .Setup(a => a.GetProperty(ApplicationConstants.ClearClearPeriodOffsetHoursText, 0D))
                .Returns(() => (DateTime.Now - DateTime.Today).TotalHours);

            var updated = false;
            var meterUpdated = false;
            var last = DateTime.Now;
            _propertiesManager
                .Setup(a => a.GetProperty(ApplicationConstants.ClearClearPeriodOffsetHoursText, 0D))
                .Returns(() => (last - DateTime.Today).TotalHours);

            _meterManager.SetupGet(a => a.LastPeriodClear).Returns(DateTime.Now);

            _time.Setup(a => a.GetLocationTime()).Returns(Time);
            _time.Setup(a => a.GetLocationTime(It.IsAny<DateTime>())).Returns(TimeMeter);

            DateTime TimeMeter()
            {
                if (!meterUpdated)
                {
                    meterUpdated = true;
                    return DateTime.Now.AddMilliseconds(100);
                }

                return last;
            }

            DateTime Time()
            {
                if (!updated)
                {
                    updated = true;
                    return DateTime.Now.AddMilliseconds(100);
                }

                return last;
            }

            var monitor = new PeriodMetersClearMonitor(
                _eventBus.Object,
                _meterManager.Object,
                _time.Object,
                _propertiesManager.Object);
            monitor.Initialize();
            monitor.Dispose();

            Thread.Sleep(100);
            _meterManager.Verify(a => a.ClearAllPeriodMeters(), Times.Exactly(0));
        }
    }
}