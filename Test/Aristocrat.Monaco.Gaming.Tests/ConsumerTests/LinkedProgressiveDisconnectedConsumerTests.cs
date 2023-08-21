namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using Consumers;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Application.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using Aristocrat.Monaco.Gaming.Progressives;

    [TestClass]
    public class LinkedProgressiveDisconnectedConsumerTests
    {
        private readonly Mock<IProgressiveErrorProvider> _errorProvider =
            new Mock<IProgressiveErrorProvider>(MockBehavior.Default);

        private readonly Mock<IProgressiveGameProvider> _progressiveGameProvider =
            new Mock<IProgressiveGameProvider>(MockBehavior.Default);

        private readonly Mock<IPersistentStorageManager> _persistenceStorage =
            new Mock<IPersistentStorageManager>(MockBehavior.Default);

        private readonly Mock<IMeterManager> _meterManager =
            new Mock<IMeterManager>(MockBehavior.Default);

        private LinkedProgressiveDisconnectedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _target = new LinkedProgressiveDisconnectedConsumer(
                _errorProvider.Object,
                _progressiveGameProvider.Object,
                _persistenceStorage.Object,
                _meterManager.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [DataTestMethod]
        public void NullConstructorTest(bool nullErrorProvider, bool nullProgressiveGameProvider, bool nullPersistenceStorage, bool nullMeterManager)
        {
            _target = new LinkedProgressiveDisconnectedConsumer(
                nullErrorProvider ? null : _errorProvider.Object,
                nullProgressiveGameProvider ? null : _progressiveGameProvider.Object,
                nullPersistenceStorage ? null : _persistenceStorage.Object,
                nullMeterManager ? null : _meterManager.Object);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            var levels = new List<IViewableLinkedProgressiveLevel>();
            for (var i = 0; i < 10; ++i)
            {
                levels.Add(new LinkedProgressiveLevel());
            }

            var scope = new Mock<IScopedTransaction>(MockBehavior.Default);
            var meter = new Mock<IMeter>(MockBehavior.Default);
            _persistenceStorage.Setup(x => x.ScopedTransaction()).Returns(scope.Object);
            _meterManager.Setup(x => x.GetMeter(ApplicationMeters.ProgressiveDisconnectCount)).Returns(meter.Object);
            _target.Consume(new LinkedProgressiveDisconnectedEvent(levels));

            meter.Verify(x => x.Increment(1), Times.Once);
            _errorProvider.Verify(x => x.ReportProgressiveDisconnectedError(levels), Times.Once);
            _progressiveGameProvider.Verify(x => x.UpdateLinkedProgressiveLevels(levels), Times.Once);
        }
    }
}