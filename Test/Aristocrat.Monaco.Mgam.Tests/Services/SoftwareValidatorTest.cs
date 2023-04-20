namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Mgam.Client.Protocol;
    using Aristocrat.Mgam.Client.Services.Directory;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Mgam.Services.Security;
    using Mgam.Services.SoftwareValidation;
    using Hardware.Contracts.IO;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Protocol.Common.Installer;
    using Test.Common;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Mgam.Services.CreditValidators;
    using Hardware.Contracts.SerialPorts;

    [TestClass]
    public class SoftwareValidatorTest
    {
        private Mock<ILogger<SoftwareValidator>> _logger;
        private Mock<IEventBus> _eventBus;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<IEgm> _egm;
        private Mock<IIO> _ioService;
        private Mock<IXmlMessageSerializer> _xmlMessageSerializer;
        private Mock<IPathMapper> _pathMapper;
        private Mock<IFileSystemProvider> _fileSystemProvider;
        private Mock<IInstallerService> _installerService;
        private Mock<IComponentRegistry> _componentRegistry;
        private Mock<IChecksumCalculator> _checksumCalculator;
        private Mock<IDirectory> _directory;
        private Mock<ICertificateService> _certificates;
        private Mock<IGameHistory> _gameHistory;
        private Mock<ICashOut> _bank;

        private SoftwareValidator _target;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MoqServiceManager.AddService(new Mock<IPrinter>());
            MoqServiceManager.AddService(new Mock<INoteAcceptor>());

            _logger = new Mock<ILogger<SoftwareValidator>>();
            _eventBus = new Mock<IEventBus>();
            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            _egm = new Mock<IEgm>();
            _directory = new Mock<IDirectory>();
            _egm.Setup(e => e.GetService<IDirectory>()).Returns(_directory.Object);
            _ioService = new Mock<IIO>();
            var serialPortService = new Mock<ISerialPortsService>();
            _ioService.SetupGet(i => i.DeviceConfiguration)
                .Returns(new Device(serialPortService.Object) { Manufacturer = "ATI" });
            _xmlMessageSerializer = new Mock<IXmlMessageSerializer>();
            _pathMapper = new Mock<IPathMapper>();
            _fileSystemProvider = new Mock<IFileSystemProvider>();
            _installerService = new Mock<IInstallerService>();
            _componentRegistry = new Mock<IComponentRegistry>();
            _checksumCalculator = new Mock<IChecksumCalculator>();
            _certificates = new Mock<ICertificateService>();
            _gameHistory = new Mock<IGameHistory>();
            _bank = new Mock<ICashOut>();

            _target = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullLoggerExpectException()
        {
            var validator = new SoftwareValidator(
                null,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                null,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullUnitOfWorkFactoryExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                null,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                null,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullIoServiceExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                null,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullXmlMessageSerializerExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                null,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPathMapperExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                null,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullFileSystemProviderExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                null,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullInstallerServiceExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                null,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullComponentRegistryExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                null,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullChecksumCalculatorExpectException()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                null,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNull(validator);
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            var validator = new SoftwareValidator(
                _logger.Object,
                _eventBus.Object,
                _unitOfWorkFactory.Object,
                _egm.Object,
                _ioService.Object,
                _xmlMessageSerializer.Object,
                _pathMapper.Object,
                _fileSystemProvider.Object,
                _installerService.Object,
                _componentRegistry.Object,
                _checksumCalculator.Object,
                _certificates.Object,
                _gameHistory.Object,
                _bank.Object);

            Assert.IsNotNull(validator);
        }

        [TestMethod]
        public void ValidateSuccess()
        {
            _directory.Setup(
                    d => d.LocateXadf(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Func<RequestXadfResponse, Task>>()))
                .Verifiable();
            _target.Validate();

            //TODO - convert host repository extension method(s) to interface that can be mocked
            //_directory.Verify();
        }

        //TODO - invoke callback and add more tests
    }
}