namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Gaming.Progressives;
    using Consumers;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class LinkedProgressiveConnectedConsumerTests
    {
        private readonly Mock<IProgressiveErrorProvider> _errorProvider =
            new Mock<IProgressiveErrorProvider>(MockBehavior.Default);

        private readonly Mock<IProgressiveGameProvider> _progressiveGameProvider =
            new Mock<IProgressiveGameProvider>(MockBehavior.Default);

        private readonly Mock<IPersistentStorageManager> _persistenceStorage =
            new Mock<IPersistentStorageManager>(MockBehavior.Default);

        private LinkedProgressiveConnectedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _target = new LinkedProgressiveConnectedConsumer(
                _errorProvider.Object,
                _progressiveGameProvider.Object,
                _persistenceStorage.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        public void NullConstructorTest(bool nullErrorProvider, bool nullProgressiveGameProvider, bool nullPersistenceStorage)
        {
            _target = new LinkedProgressiveConnectedConsumer(
                nullErrorProvider ? null : _errorProvider.Object,
                nullProgressiveGameProvider ? null : _progressiveGameProvider.Object,
                nullPersistenceStorage ? null : _persistenceStorage.Object);
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
            _persistenceStorage.Setup(x => x.ScopedTransaction()).Returns(scope.Object);
            _target.Consume(new LinkedProgressiveConnectedEvent(levels));

            _errorProvider.Verify(x => x.ClearProgressiveDisconnectedError(levels), Times.Once);
            _progressiveGameProvider.Verify(x => x.UpdateLinkedProgressiveLevels(levels), Times.Once);
        }
    }
}