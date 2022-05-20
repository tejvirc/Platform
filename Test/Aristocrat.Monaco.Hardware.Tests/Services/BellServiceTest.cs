namespace Aristocrat.Monaco.Hardware.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Hardware.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.IO;
    using Aristocrat.Monaco.Hardware.Services;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BellServiceTest
    {
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IEventBus> _bus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IIO> _iio = new Mock<IIO>(MockBehavior.Default);
        private BellService _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateBellService();
        }

        [DataTestMethod]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullIIO,
            bool nullPropertiesManger,
            bool nullEventBus)
        {
            _ = CreateBellService(nullIIO, nullPropertiesManger, nullEventBus);
        }

        [DataTestMethod]
        [DataRow(100, true, true, true, true, true)]
        [DataRow(100, false, true, true, true, false)]
        [DataRow(100, true, false, true, true, false)]
        [DataRow(100, true, true, false, true, true)]
        [DataRow(100, true, true, true, false, true)]
        public async Task RingBellWithDelayTest(
            long durationMs,
            bool isEnabled,
            bool isInitialized,
            bool ringSucceded,
            bool stopSucceded,
            bool expectedReturn)
        {
            if (isInitialized)
            {
                Initialize(isEnabled);
            }

            _iio.Setup(x => x.SetBellState(true)).Returns(ringSucceded);
            _iio.Setup(x => x.SetBellState(false)).Returns(stopSucceded);
            var result = await _target.RingBell(TimeSpan.FromMilliseconds(durationMs));
            Assert.AreEqual(expectedReturn, result);
        }

        [DataTestMethod]
        [DataRow(true, true, true, true)]
        [DataRow(false, true, true, false)]
        [DataRow(true, false, true, false)]
        [DataRow(true, true, false, true)]
        public void RingBellTest(
            bool isEnabled,
            bool isInitialized,
            bool ringSucceded,
            bool expectedReturn)
        {
            if (isInitialized)
            {
                Initialize(isEnabled);
            }

            _iio.Setup(x => x.SetBellState(true)).Returns(ringSucceded);
            var result = _target.RingBell();
            Assert.AreEqual(expectedReturn, result);
            Assert.AreEqual(expectedReturn, _target.IsRinging);
        }

        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(false, true)]
        public void StopBellTest(bool stopSucceded, bool expectedReturn)
        {
            _iio.Setup(x => x.SetBellState(false)).Returns(stopSucceded);
            var result = _target.StopBell();
            Assert.AreEqual(expectedReturn, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task InvalidDurationTest()
        {
            await _target.RingBell(TimeSpan.Zero);
        }

        private void Initialize(bool isEnabled)
        {
            _propertiesManager.Setup(x => x.GetProperty(HardwareConstants.BellEnabledKey, It.IsAny<bool>()))
                .Returns(isEnabled);
            _target.Initialize();
        }

        private BellService CreateBellService(
            bool nullIIO = false,
            bool nullPropertiesManger = false,
            bool nullEventBus = false)
        {
            return new BellService(
                nullIIO ? null : _iio.Object,
                nullPropertiesManger ? null : _propertiesManager.Object,
                nullEventBus ? null : _bus.Object);
        }
    }
}
