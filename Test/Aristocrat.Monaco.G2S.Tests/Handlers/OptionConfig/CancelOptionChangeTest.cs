namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
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
    public class CancelOptionChangeTest
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
            ConstructorTest.TestConstructorNullChecks<CancelOptionChange>();
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectSuccess()
        {
            var handler = CreateHandler();

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidationFailExpectError()
        {
            _changeLogValidationServiceMock.Setup(m => m.Validate(It.IsAny<long>()))
                .Returns(ErrorCode.ATI_CMX001);

            var deviceMock = new Mock<IOptionConfigDevice>();
            deviceMock.SetupGet(m => m.Owner).Returns(TestConstants.HostId);

            var egm = HandlerUtilities.CreateMockEgm(deviceMock);

            var handler = CreateHandler(egm);
            var command = CreateCommand();

            var error = await handler.Verify(command);

            Assert.AreEqual(ErrorCode.ATI_CMX001, error.Code);
        }

        [TestMethod]
        public async Task WhenHandleWithValidCommandExpectSuccess()
        {
            var deviceMock = new Mock<IOptionConfigDevice>();

            _egmMock.Setup(m => m.GetDevice<IOptionConfigDevice>(It.IsAny<int>()))
                .Returns(deviceMock.Object);

            var handler = CreateHandler();
            var command = CreateCommand();

            await handler.Handle(command);

            _configurationServiceMock.Verify(m => m.Cancel(command.Command.transactionId));
            _commandBuilderMock.Verify(m => m.Build(deviceMock.Object, It.IsAny<optionChangeStatus>()));
        }

        private CancelOptionChange CreateHandler(IG2SEgm egm = null)
        {
            var handler = new CancelOptionChange(
                egm ?? _egmMock.Object,
                _commandBuilderMock.Object,
                _changeLogValidationServiceMock.Object,
                _configurationServiceMock.Object);

            return handler;
        }

        private ClassCommand<optionConfig, cancelOptionChange> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, cancelOptionChange>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.transactionId = 1;
            command.Command.configurationId = 1;

            return command;
        }
    }
}