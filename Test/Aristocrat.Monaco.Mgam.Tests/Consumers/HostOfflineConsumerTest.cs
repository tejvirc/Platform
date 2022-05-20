namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Application.Contracts;
    using Common;
    using Common.Events;
    using Mgam.Consumers;
    using Gaming.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Aristocrat.Monaco.Test.Common;

    [TestClass]
    public class HostOfflineConsumerTest
    {
        private Mock<ISystemDisableManager> _disable;
        private Mock<IPropertiesManager> _properties;
        private Mock<IGamePlayState> _gamePlay;
        private Mock<IAudio> _audio;
        private Mock<ITowerLight> _towerLight;
        private Mock<ITime> _time;
        private readonly Dictionary<Guid, string> _disableKeys = new Dictionary<Guid, string>();
        private HostOfflineConsumer _target;


        [TestInitialize]
        public void MyTestInitialize()
        {
            _disable = new Mock<ISystemDisableManager>();
            _properties = new Mock<IPropertiesManager>();
            _gamePlay = new Mock<IGamePlayState>();
            _audio = new Mock<IAudio>();
            _towerLight = new Mock<ITowerLight>();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _time = new Mock<ITime>();
            _time.Setup(t => t.GetLocationTime(It.IsAny<DateTime>())).Returns(DateTime.Now);
            _disable.Setup(d => d.Disable(It.IsAny<Guid>(), It.IsAny<SystemDisablePriority>(), It.IsAny<Func<string>>(), It.IsAny<Type>()))
                .Callback<Guid, SystemDisablePriority, Func<string>, Type>((g, p, f, t) => _disableKeys[g] = f());
        }

        [DataRow(false, true, true, true, true, true, DisplayName = "Null System Disable Manager Object")]
        [DataRow(true, false, true, true, true, true, DisplayName = "Null Properties Manager Object")]
        [DataRow(true, true, false, true, true, true, DisplayName = "Null Game Play State Object")]
        [DataRow(true, true, true, false, true, true, DisplayName = "Null Audio Service Object")]
        [DataRow(true, true, true, true, false, true, DisplayName = "Null Tower Light Service Object")]
        [DataRow(true, true, true, true, true, false, DisplayName = "Null Time Service Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool disableManager,
            bool propertiesManager,
            bool gamePlay,
            bool audioService,
            bool towerLightService,
            bool timeSerivice)
        {
            _target = new HostOfflineConsumer(
                disableManager ? _disable.Object : null,
                propertiesManager ? _properties.Object : null,
                gamePlay ? _gamePlay.Object : null,
                audioService ? _audio.Object : null,
                towerLightService ? _towerLight.Object : null,
                timeSerivice ? _time.Object : null);
        }

        [TestMethod]
        public void SuccessfulConstructorTest()
        {
            CreateNewTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void TestConsumerSuccess()
        {
            CreateNewTarget();

            _target.Consume(new HostOfflineEvent());

            Assert.AreEqual(true, _disableKeys.ContainsKey(MgamConstants.HostOfflineGuid));
            _disable.Verify();
        }

        [TestMethod]
        public void TestConsumerPlaysSoundFlashesLightsWhenIdle()
        {
            CreateNewTarget();

            _gamePlay.SetupGet(g => g.Idle).Returns(true);
            _properties.Setup(p => p.GetProperty(ApplicationConstants.HostOfflineSoundKey, string.Empty)).Returns("test");
            _properties.Setup(p => p.GetProperty(ApplicationConstants.AlertVolumeKey, MgamConstants.DefaultAlertVolume)).Returns(MgamConstants.DefaultAlertVolume);
            _audio.Setup(a => a.Load(It.IsAny<string>())).Returns(true);
            _audio.Setup(a => a.Play(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null)).Verifiable();
            _towerLight.Setup(t => t.SetFlashState(LightTier.Tier1, FlashState.FastFlash, It.IsAny<TimeSpan>(), false)).Verifiable();

            _target.Consume(new HostOfflineEvent());

            Assert.AreEqual(true, _disableKeys.ContainsKey(MgamConstants.HostOfflineGuid));
            _disable.Verify();
            _audio.Verify();
            _towerLight.Verify();
        }

        private void CreateNewTarget()
        {
            _target = new HostOfflineConsumer(_disable.Object, _properties.Object, _gamePlay.Object, _audio.Object, _towerLight.Object, _time.Object);
        }
    }
}