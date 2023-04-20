namespace Aristocrat.Monaco.Hardware.Tests.NoteAcceptor
{
    using Contracts.Dfu;
    using Contracts.NoteAcceptor;
    using Contracts.Persistence;
    using Contracts.SerialPorts;
    using Hardware.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Contains the unit tests for the NoteAcceptorAdapter class.
    /// </summary>
    [TestClass]
    public class NoteAcceptorAdapterTest
    {

        private Mock<IPersistentStorageManager> _persistence;
        private Mock<IPersistentStorageAccessor> _accessor = new(MockBehavior.Strict);
        private Mock<IEventBus> _eventBus;

        private NoteAcceptorAdapter _target;
        private string _testConfigurationString = "Test,Test,Test,0,0";

        [TestInitialize]
        public void MyTestInitialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
            _persistence = new Mock<IPersistentStorageManager>(MockBehavior.Strict);
            _persistence.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistence.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_accessor.Object);

            _accessor.Setup(m => m.Level).Returns(PersistenceLevel.Transient);
            _accessor.Setup(m => m["Configuration"]).Returns(_testConfigurationString);

            var component = new Mock<IComponentRegistry>();
            var dfuProvider = new Mock<IDfuProvider>();
            var disableNote = new Mock<IDisabledNotesService>();
            var persistenceProvider = new Mock<IPersistenceProvider>();
            var serialPortService = new Mock<ISerialPortsService>();
            _target = new NoteAcceptorAdapter(
                _eventBus.Object,
                component.Object,
                dfuProvider.Object,
                _persistence.Object,
                disableNote.Object,
                persistenceProvider.Object,
                serialPortService.Object);
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
