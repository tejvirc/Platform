namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using G2S.Handlers.CommConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CancelCommChangeTest : BaseCommConfigHandlerTest
    {
        private readonly Mock<IConfigurationService> _configuration = new Mock<IConfigurationService>();

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<CancelCommChange>();
        }

        [TestMethod]
        public async Task WhenNotFoundPendingChangeLogByIdentifiersExpectError()
        {
            ConfigureConfigDeviceMock();

            var handler = CreateHandler();

            var command = CreateCommand();

            CommChangeLogValidationServiceMock.Setup(
                    x => x.Validate(command.Command.transactionId))
                .Returns(ErrorCode.G2S_CCX006);

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_CCX006);
        }

        [TestMethod]
        public async Task WhenNotFoundPendingChangeLogByTransactionIdExpectError()
        {
            ConfigureConfigDeviceMock();

            var handler = CreateHandler();

            var command = CreateCommand();

            CommChangeLogValidationServiceMock.Setup(
                    x => x.Validate(command.Command.transactionId))
                .Returns(ErrorCode.G2S_CCX005);

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, ErrorCode.G2S_CCX005);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var handler = CreateHandler(egm.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithExpiredCommandExpectError()
        {
            var handler = CreateHandler(HandlerUtilities.CreateMockEgm<ICommConfigDevice>());

            await VerificationTests.VerifyChecksTimeToLive(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithInvalidHostIdExpectError()
        {
            const int invalidHostId = 99;

            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(invalidHostId);

            var handler = CreateHandler(HandlerUtilities.CreateMockEgm(device));

            await VerificationTests.VerifyChecksForOwner(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var handler = CreateHandler();
            var command = CreateCommand();

            ConfigureConfigDeviceMock();

            CommChangeLogValidationServiceMock.Setup(
                    x => x.Validate(command.Command.transactionId))
                .Returns(string.Empty);

            await VerificationTests.VerifyCanSucceed(handler, command);
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SResponseExpectNoResult()
        {
            var handler = CreateHandler();

            var command =
                ClassCommandUtilities.CreateClassCommand<commConfig, cancelCommChange>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_response;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenSessionTypeIsG2SNotificationExpectNoResult()
        {
            var handler = CreateHandler();

            var command =
                ClassCommandUtilities.CreateClassCommand<commConfig, cancelCommChange>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            command.Class.sessionType = t_sessionTypes.G2S_notification;

            await handler.Handle(command);

            Assert.AreEqual(0, command.Responses.Count());
        }

        [TestMethod]
        public async Task WhenVerifyCanSucceedExpectSuccess()
        {
            ConfigureConfigDeviceMock();
            var handler = CreateHandler();

            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerDeviceExpectNoError()
        {
            ConfigureConfigDeviceMock();
            var handler = CreateHandler();

            await handler.Verify(CreateCommand());
        }

        [TestMethod]
        public async Task WhenNormalFlowExpectSuccess()
        {
            ConfigureConfigDeviceMock();

            var commChangeLog = CreateCommChangeLog();
            commChangeLog.AuthorizeItems = (ICollection<ConfigChangeAuthorizeItem>)CreateAuthorizeItems();

            ChangeLogRepositoryMock.Setup(
                    x => x.GetByTransactionId(It.IsAny<DbContext>(), It.IsAny<long>()))
                .Returns(commChangeLog);

            var handler = CreateHandlerWithRealBulder();

            var command = CreateCommand();

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<commConfig, commChangeStatus>;

            _configuration.Verify(c => c.Cancel(It.IsAny<long>()));

            Assert.IsNotNull(response);

            Assert.IsNotNull(response.Command.authorizeStatusList);
            Assert.AreEqual(response.Command.authorizeStatusList.authorizeStatus.Length, 2);
        }

        private ClassCommand<commConfig, cancelCommChange> CreateCommand()
        {
            var command = ClassCommandUtilities.CreateClassCommand<commConfig, cancelCommChange>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.transactionId = 1;
            command.Command.configurationId = 1;

            return command;
        }

        private CancelCommChange CreateHandler()
        {
            return CreateHandler(HandlerUtilities.CreateMockEgm(ConfigDeviceMock));
        }

        private CancelCommChange CreateHandler(IG2SEgm egm)
        {
            var handler = new CancelCommChange(
                egm,
                CommChangeLogValidationServiceMock.Object,
                _configuration.Object,
                CommandBuilderMock.Object);

            return handler;
        }

        private CancelCommChange CreateHandlerWithRealBulder()
        {
            var egm = HandlerUtilities.CreateMockEgm(ConfigDeviceMock);
            var contextFactory = new Mock<IMonacoContextFactory>();

            var builder = new CommChangeStatusCommandBuilder(ChangeLogRepositoryMock.Object, contextFactory.Object);

            var handler = new CancelCommChange(
                egm,
                CommChangeLogValidationServiceMock.Object,
                _configuration.Object,
                builder);

            return handler;
        }
    }
}