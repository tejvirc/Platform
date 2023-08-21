namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.Events;
    using Mgam.Consumers;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HostOnlineConsumerTest
    {
        private Mock<ISystemDisableManager> _disable;
        private Mock<IPropertiesManager> _properties;
        private Mock<IAudio> _audio;
        private Mock<ITowerLight> _towerLight;
        private readonly Dictionary<Guid, string> _disableKeys = new Dictionary<Guid, string>();
        private HostOnlineConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _disable = new Mock<ISystemDisableManager>();
            _properties = new Mock<IPropertiesManager>();
            _audio = new Mock<IAudio>();
            _towerLight = new Mock<ITowerLight>();
        }

        [DataRow(false, true, true, true, DisplayName = "Null System Disable Manager Object")]
        [DataRow(true, false, true, true, DisplayName = "Null Properties Manager Object")]
        [DataRow(true, true, false, true, DisplayName = "Null Audio Service Object")]
        [DataRow(true, true, true, false, DisplayName = "Null Tower Light Service Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool disableManager,
            bool propertiesManager,
            bool audioService,
            bool towerLightService)
        {
            _target = new HostOnlineConsumer(
                disableManager ? _disable.Object : null,
                propertiesManager ? _properties.Object : null,
                audioService ? _audio.Object : null,
                towerLightService ? _towerLight.Object : null);
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

            _disable.Setup(d => d.Enable(It.IsAny<Guid>()))
                .Callback<Guid>(g => _disableKeys.Remove(g));

            _audio.Setup(a => a.Stop());

            _target.Consume(new HostOnlineEvent("127.0.0.1"));

            Assert.AreEqual(false, _disableKeys.ContainsKey(MgamConstants.HostOfflineGuid));
            _disable.Verify();
        }

        private void CreateNewTarget()
        {
            _target = new HostOnlineConsumer(_disable.Object, _properties.Object, _audio.Object, _towerLight.Object);
        }
    }
}
