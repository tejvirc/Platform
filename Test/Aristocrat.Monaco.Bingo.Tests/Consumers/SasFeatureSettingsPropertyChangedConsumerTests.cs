namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
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
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>())).Returns(new SasFeatures());
            _target = new SasFeatureSettingsPropertyChangedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _bingoEventQueue.Object,
                _propertiesManager.Object);
        }

        [DataRow(true, false, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, false, DisplayName = "Event Reporting Service Null")]
        [DataRow(false, false, true, DisplayName = "Properties Manager Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool eventBusNull, bool queueNull, bool propertiesNull)
        {
            _target = new SasFeatureSettingsPropertyChangedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                queueNull ? null : _bingoEventQueue.Object,
                propertiesNull ? null : _propertiesManager.Object);
        }

        [TestMethod]
        public void ConsumesBonusEnabledTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = true });
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled)).Verifiable();

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled), Times.Once());
        }

        [TestMethod]
        public void ConsumesBonusDisabledTest()
        {
            _propertiesManager
                .SetupSequence(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = true })
                .Returns(new SasFeatures { AftBonusAllowed = false });
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>())).Verifiable();

            _target = new SasFeatureSettingsPropertyChangedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _bingoEventQueue.Object,
                _propertiesManager.Object);

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusingDisabled), Times.Once());
        }

        [TestMethod]
        public void ConsumesBonusEnabledChangedTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = false });
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled)).Verifiable();

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = true, LegacyBonusAllowed = false });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled), Times.Once());
            _eventBus.Verify(m => m.Publish(It.IsAny<RestartProtocolEvent>()), Times.Once());
        }

        [TestMethod]
        public void ConsumesLegacyBonusEnabledChangedTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = false });
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled)).Verifiable();

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = true });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled), Times.Never());
            _eventBus.Verify(m => m.Publish(It.IsAny<RestartProtocolEvent>()), Times.Once());
        }

        [TestMethod]
        public void ConsumesBothBonusEnabledChangedTest()
        {
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = false, LegacyBonusAllowed = false });
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled)).Verifiable();

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { AftBonusAllowed = true, LegacyBonusAllowed = true });

            _target.Consume(new PropertyChangedEvent(SasProperties.SasFeatureSettings));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusingEnabled), Times.Once());
            _eventBus.Verify(m => m.Publish(It.IsAny<RestartProtocolEvent>()), Times.Once());
        }
    }
}