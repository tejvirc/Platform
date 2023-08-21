namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Contracts.Events;
    using Sas.Contracts.SASProperties;

    [TestClass]
    public class SasFeatureSettingsPropertyChangedConsumerTests
    {
        private SasFeatureSettingsPropertyChangedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures());
            _target = new SasFeatureSettingsPropertyChangedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _propertiesManager.Object);
        }

        [DataRow(true, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, DisplayName = "Properties Manager Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool eventBusNull, bool propertiesNull)
        {
            _target = new SasFeatureSettingsPropertyChangedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                propertiesNull ? null : _propertiesManager.Object);
        }

        [TestMethod]
        public void ConsumesBonusEnabledChangedTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = false });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = true, LegacyBonusAllowed = false });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _eventBus.Verify(m => m.Publish(It.IsAny<RestartProtocolEvent>()), Times.Once());
        }

        [TestMethod]
        public void ConsumesLegacyBonusEnabledChangedTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = false });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = true });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _eventBus.Verify(m => m.Publish(It.IsAny<RestartProtocolEvent>()), Times.Once());
        }

        [TestMethod]
        public void ConsumesBothBonusEnabledChangedTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = false });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = true, LegacyBonusAllowed = true });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _eventBus.Verify(m => m.Publish(It.IsAny<RestartProtocolEvent>()), Times.Once());
        }
    }
}