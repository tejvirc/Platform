namespace Aristocrat.Monaco.Hardware.Tests.Services
{
    using System;
    using Contracts.IO;
    using Contracts.TowerLight;
    using Hardware.Services;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TowerLightServiceTests
    {
        private TowerLightService _underTest;
        private Mock<IIO> _iio;
        private Mock<IEventBus> _eventBus;

        [TestInitialize]
        public void Setup()
        {
            _iio = new Mock<IIO>();
            _eventBus = new Mock<IEventBus>();
            _underTest = new TowerLightService(_iio.Object, _eventBus.Object);
            _underTest.Reset();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _underTest.Dispose();
        }

        [TestMethod]
        [DataRow(LightTier.Tier1, FlashState.SlowFlashReversed, false, 0)]
        [DataRow(LightTier.Tier1, FlashState.SlowFlash, true, 0)]
        [DataRow(LightTier.Tier1, FlashState.MediumFlashReversed, false, 0)]
        [DataRow(LightTier.Tier1, FlashState.MediumFlash, true, 0)]
        [DataRow(LightTier.Tier1, FlashState.FastFlash, true, 0)]
        [DataRow(LightTier.Tier1, FlashState.Off, false, -1)]
        [DataRow(LightTier.Tier1, FlashState.On, true, -1)]
        public void GivenTierSignalWhenSetTowerLightThenExpectedOnOff(
            LightTier tier,
            FlashState signal,
            bool isOn,
            int duration)
        {
            _iio.Setup(x => x.SetTowerLight((int)tier, isOn)).Returns(true);
            if (signal == FlashState.Off)
            {
                // set to on first
                _underTest.SetFlashState(tier, FlashState.On, new TimeSpan(0, 0, 0, 0, -1));
                _iio.ResetCalls();
            }

            _underTest.SetFlashState(tier, signal, new TimeSpan(0, 0, 0, 0, duration));
            _iio.Verify(x => x.SetTowerLight((int)tier, isOn), Times.Once);
            _eventBus.Verify(
                x =>
                    x.Publish(It.Is<TowerLightOnEvent>(p => p.LightTier == tier && p.FlashState == signal)),
                isOn ? Times.Once() : Times.Never());
            _eventBus.Verify(
                x =>
                    x.Publish(It.Is<TowerLightOffEvent>(p => p.LightTier == tier && p.FlashState == signal)),
                !isOn ? Times.Once() : Times.Never());
        }
    }
}