namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Threading;
    using Accounting.Contracts;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CabinetStateServiceTests
    {
        private const int wrapUpTimeout = 2500;
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenBankIsNullExpectException()
        {
            var service = new CabinetState(null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenDisableManagerIsNullExpectException()
        {
            var bank = new Mock<IBank>();

            var service = new CabinetState(bank.Object, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();

            var service = new CabinetState(bank.Object, disables.Object, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventBusIsNullExpectException()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns((int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void WhenDisposeExpectSuccess()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns((int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            Assert.IsNotNull(service);

            service.Dispose();

            bus.Verify(b => b.UnsubscribeAll(service));
        }

        [TestMethod]
        public void WhenInitializeExpectSuccess()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns((int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);

            // Initialize is call in the ctor
            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            bus.Verify(b => b.Subscribe(It.Is<object>(o => o == service), It.IsAny<Action<BankBalanceChangedEvent>>()));
            bus.Verify(b => b.Subscribe(It.Is<object>(o => o == service), It.IsAny<Action<SystemDisableAddedEvent>>()));
            bus.Verify(
                b => b.Subscribe(It.Is<object>(o => o == service), It.IsAny<Action<SystemDisableRemovedEvent>>()));
            bus.Verify(b => b.Subscribe(It.Is<object>(o => o == service), It.IsAny<Action<GameIdleEvent>>()));
            bus.Verify(b => b.Subscribe(It.Is<object>(o => o == service), It.IsAny<Action<GameSelectedEvent>>()));

            Assert.IsTrue(service.Idle);
        }

        [TestMethod]
        public void WhenHasNonZeroCreditsExpectNotIdle()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns((int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);

            bank.Setup(b => b.QueryBalance()).Returns(1);

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            Assert.IsFalse(service.Idle);
        }

        [TestMethod]
        public void WhenSystemDisabledExpectNotIdle()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns((int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);

            disables.SetupGet(d => d.IsDisabled).Returns(true);
            disables.SetupGet(d => d.IsIdleStateAffected).Returns(true);

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            Assert.IsFalse(service.Idle);
        }

        [TestMethod]
        public void WhenActivityOccursExpectNoLongerIdle()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns((int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);

            Action<BankBalanceChangedEvent> callback = null;
            bus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback(
                    (object subscriber, Action<BankBalanceChangedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            Assert.IsNotNull(callback);
            Assert.IsTrue(service.Idle);

            callback.Invoke(new BankBalanceChangedEvent(0, 0, Guid.Empty));

            Assert.IsFalse(service.Idle);
        }

        [TestMethod]
        public void WhenTimerLapsesExpectIdle()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>())).Returns(500);

            Action<BankBalanceChangedEvent> callback = null;
            bus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback(
                    (object subscriber, Action<BankBalanceChangedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            Assert.IsNotNull(callback);
            Assert.IsTrue(service.Idle);

            callback.Invoke(new BankBalanceChangedEvent(0, 0, Guid.Empty));

            Assert.IsFalse(service.Idle);

            //TODO: Wrap the timer in an interface and inject it so we can mock it
            Thread.Sleep(wrapUpTimeout);

            Assert.IsTrue(service.Idle);
        }

        [TestMethod]
        public void WhenTimerLapsesOutsideIdlePeriodExpectStillNotIdle()
        {
            var bank = new Mock<IBank>();
            var disables = new Mock<ISystemDisableManager>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(p => p.GetProperty(GamingConstants.IdleTimePeriod, It.IsAny<object>()))
                .Returns((int)GamingConstants.DefaultIdleTimeoutPeriod.TotalMilliseconds);

            Action<BankBalanceChangedEvent> callback = null;
            bus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
                .Callback(
                    (object subscriber, Action<BankBalanceChangedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            var service = new CabinetState(bank.Object, disables.Object, properties.Object, bus.Object);

            Assert.IsNotNull(callback);
            Assert.IsTrue(service.Idle);

            callback.Invoke(new BankBalanceChangedEvent(0, 0, Guid.Empty));

            Assert.IsFalse(service.Idle);

            //TODO: Wrap the timer in an interface and inject it so we can mock it
            Thread.Sleep(wrapUpTimeout);

            Assert.IsFalse(service.Idle);
        }
    }
}