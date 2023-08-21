namespace Aristocrat.Monaco.Hardware.Tests.NoteAcceptor
{
    using Contracts.NoteAcceptor;
    using Contracts.Persistence;
    using Hardware.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Contains the unit tests for the NoteAcceptorAdapter class.
    /// </summary>
    [TestClass]
    public class NoteAcceptorAdapterTest
    {

        private Mock<IPersistentStorageManager> _persistence;
        private Mock<IPersistentStorageAccessor> _accessor = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
        private Mock<IEventBus> _eventBus;

        private NoteAcceptorAdapter _target;
        private string _testConfigurationString = "Test,Test,Test,0,0";

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _persistence = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _persistence.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistence.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_accessor.Object);

            _accessor.Setup(m => m.Level).Returns(PersistenceLevel.Transient);
            _accessor.Setup(m => m["Configuration"]).Returns(_testConfigurationString);

            _target = new NoteAcceptorAdapter();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void StartingConfigurationTest()
        {
            Assert.AreEqual(_testConfigurationString, _target.Configuration);
        }

        [TestMethod]
        public void SetConfigurationNoChangeTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<NoteAcceptorChangedEvent>())).Verifiable();

            _target.Configuration = _testConfigurationString;

            _eventBus.Verify(m => m.Publish(It.IsAny<NoteAcceptorChangedEvent>()), Times.Never);
        }

        [TestMethod]
        public void SetConfigurationNewValueTest()
        {
            string newConfiguration = "Different Configuration";
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Default);
            transaction.SetupSet(m => m["Configuration"] = newConfiguration).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();

            _eventBus.Setup(m => m.Publish(It.IsAny<NoteAcceptorChangedEvent>())).Verifiable();
            _accessor.Setup(m => m.StartTransaction()).Returns(transaction.Object);

            _target.Configuration = newConfiguration;

            Assert.AreEqual(newConfiguration, _target.Configuration);
            _eventBus.Verify(m => m.Publish(It.IsAny<NoteAcceptorChangedEvent>()), Times.Once);
            _accessor.Verify();
            transaction.Verify();
        }
    }
}
