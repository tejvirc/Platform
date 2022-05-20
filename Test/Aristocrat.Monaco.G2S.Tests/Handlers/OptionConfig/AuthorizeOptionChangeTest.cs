namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.OptionConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class AuthorizeOptionChangeTest
    {
        private Mock<IOptionChangeLogValidationService> _changeLogValidationServiceMock;

        private Mock<ICommandBuilder<IOptionConfigDevice, optionChangeStatus>> _commandBuilderMock;

        private Mock<IConfigurationService> _configurationServiceMock;
        private Mock<IG2SEgm> _egmMock;

        [TestInitialize]
        public void Initialize()
        {
            _egmMock = new Mock<IG2SEgm>();
            _changeLogValidationServiceMock = new Mock<IOptionChangeLogValidationService>();
            _commandBuilderMock = new Mock<ICommandBuilder<IOptionConfigDevice, optionChangeStatus>>();
            _configurationServiceMock = new Mock<IConfigurationService>();
        }

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<AuthorizeOptionChange>();
        }

        [TestMethod]
        public async Task WhenChangeLogValidationReturnErrorExpectError()
        {
            var deviceMock = new Mock<IOptionConfigDevice>();
            deviceMock.SetupGet(m => m.Owner).Returns(TestConstants.HostId);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);

            var handler = CreateHandler(egm);
            var command = CreateCommand();

            _changeLogValidationServiceMock.Setup(
                    x => x.Validate(command.Command.transactionId))
                .Returns(ErrorCode.G2S_OCX006);

            var error = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.G2S_OCX006, error.Code);
        }

        [TestMethod]
        public async Task WhenVerifyChecksForNoDeviceExpectSuccess()
        {
            var handler = CreateHandler();

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        [Ignore]
        public async Task WhenHandleCommandExpectResponse()
        {
            var deviceMock = new Mock<IOptionConfigDevice>();
            _egmMock.Setup(m => m.GetDevice<IOptionConfigDevice>(It.IsAny<int>()))
                .Returns(deviceMock.Object);

            var handler = CreateHandler();
            var command = CreateCommand();

            command.Command.configurationId = 1;
            command.Command.transactionId = 2;

            await handler.Handle(command);

            _configurationServiceMock.Verify(m => m.Authorize(command.Command.transactionId, command.HostId, false));
            _commandBuilderMock.Verify(m => m.Build(deviceMock.Object, It.IsAny<optionChangeStatus>()));
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResponse()
        {
            var handler = CreateHandler();

            var command = CreateCommand();
            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        private AuthorizeOptionChange CreateHandler(IG2SEgm egm = null)
        {
            var handler = new AuthorizeOptionChange(
                egm ?? _egmMock.Object,
                _commandBuilderMock.Object,
                _changeLogValidationServiceMock.Object,
                _configurationServiceMock.Object);

            return handler;
        }

        private ClassCommand<optionConfig, authorizeOptionChange> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, authorizeOptionChange>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.transactionId = 1;
            command.Command.configurationId = 1;

            return command;
        }
    }
}