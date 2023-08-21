namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using Microsoft.EntityFrameworkCore;
    using System.Threading;
    using Application.Contracts.Authentication;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Storage;
    using Hardware.Contracts;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.Printer;
    using Kernel.Contracts.Components;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class VerifyComponentHandlerTest
    {
        private Mock<IAuthenticationService> _componentHashServiceMock;

        private Mock<IComponentRegistry> _componentRepositoryMock;

        private Mock<IMonacoContextFactory> _contextFactoryMock;

        private Mock<IDeviceRegistryService> _deviceRegistryServiceMock;
        private GatComponentVerification _gatComponentVerification;

        private Mock<IGatComponentVerificationRepository> _gatComponentVerificationRepository;

        [TestInitialize]
        public void Initialize()
        {
            _gatComponentVerification = new GatComponentVerification(1);
            _componentHashServiceMock = new Mock<IAuthenticationService>();
            _deviceRegistryServiceMock = new Mock<IDeviceRegistryService>();
            _componentRepositoryMock = new Mock<IComponentRegistry>();
            _gatComponentVerificationRepository = new Mock<IGatComponentVerificationRepository>();
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
        }

        [TestMethod]
        public void WhenVerifyComponentWithPrinterComponentPathExpectSuccess()
        {
            var component = new Component
            {
                FileSystemType = FileSystemType.Stream,
                Path = "printer"
            };

            var printerMock = new Mock<IPrinter>();
            _deviceRegistryServiceMock.Setup(m => m.GetDevice<IPrinter>()).Returns(printerMock.Object);
            _componentRepositoryMock.SetupGet(m => m.Components).Returns(new[] { component });
            var handler = CreateHandler();
            handler.VerifyComponent();

            Thread.Sleep(1000);
        }

        [TestMethod]
        public void WhenVerifyComponentWithNoteAcceptorComponentPathExpectSuccess()
        {
            var component = new Component
            {
                FileSystemType = FileSystemType.Stream,
                Path = "noteAcceptor"
            };

            var noteAcceptorMock = new Mock<INoteAcceptor>();
            _deviceRegistryServiceMock.Setup(m => m.GetDevice<INoteAcceptor>()).Returns(noteAcceptorMock.Object);
            _componentRepositoryMock.SetupGet(m => m.Components).Returns(new[] { component });
            var handler = CreateHandler();
            handler.VerifyComponent();

            Thread.Sleep(1000);
        }

        [TestMethod]
        public void WhenVerifyComponentWithFileSystemTypeNotHardwareExpectInstantRunVerification()
        {
            var component = new Component
            {
                FileSystemType = FileSystemType.Directory
            };

            _componentRepositoryMock.SetupGet(m => m.Components).Returns(new[] { component });
            var handler = CreateHandler();
            handler.VerifyComponent();

            Thread.Sleep(1000);

            Assert.AreEqual(ComponentVerificationState.Complete, _gatComponentVerification.State);
            _gatComponentVerificationRepository
                .Verify(m => m.Update(It.IsAny<DbContext>(), _gatComponentVerification), Times.Exactly(2));
        }

        private VerifyComponentHandler CreateHandler()
        {
            var handler = new VerifyComponentHandler(
                _gatComponentVerification,
                _componentHashServiceMock.Object,
                _componentRepositoryMock.Object,
                _gatComponentVerificationRepository.Object,
                _contextFactoryMock.Object);

            return handler;
        }
    }
}