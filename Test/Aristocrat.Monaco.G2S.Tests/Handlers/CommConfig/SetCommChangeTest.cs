namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Protocol.v21;
    using Data.CommConfig;
    using Data.Model;
    using G2S.Handlers;
    using G2S.Handlers.CommConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SetCommChangeTest : BaseCommConfigHandlerTest
    {
        private readonly Mock<IConfigurationService> _configuration = new Mock<IConfigurationService>();

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<SetCommChange>();
        }

        [TestMethod]
        public async Task WhenApplyConditionIsDisableAndDisableConditionIsNoneExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);

            command.Command.applyCondition = "G2S_disable";
            command.Command.disableCondition = "G2S_none";

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX009");
        }

        [TestMethod]
        public async Task WhenDuplicateCommandExpectError()
        {
            var handler = CreateHandler();

            var command = CreateCommand(StartDateTime, EndDateTime);

            var existChangeLog = CreateCommChangeLog();

            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenRegisteredHostsLessTwoExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.configurationId = 2;

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            HostConfigItemRepositoryMock.Setup(x => x.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<CommHostConfigItem>
                    {
                        new CommHostConfigItem
                        {
                            HostId = 1,
                            HostRegistered = true
                        },
                        new CommHostConfigItem
                        {
                            HostId = 2,
                            HostRegistered = false
                        }
                    }.AsQueryable());

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenNotAnyConfiguredHostsExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.configurationId = 2;

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenEmptyIncomingDevicesExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.setHostItem = new setHostItem[] { };

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenNotExistOwnedDevicesIncomingDevicesExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.setHostItem[0].ownedDevice1 = new c_setHostItem.ownedDevice[] { };

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenNotExistGuestDevicesIncomingDevicesExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.setHostItem[0].guestDevice1 = new c_setHostItem.guestDevice[] { };

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenNotExistConfigDevicesIncomingDevicesExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.setHostItem[0].configDevice1 = new c_setHostItem.configDevice[] { };

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenAttemptChangeHostIdExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.configurationId = 2;
            command.Command.setHostItem[0].hostIndex = 2;

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenNoIncomingDeviceClassExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.configurationId = 2;
            command.Command.setHostItem[0].configDevice1[0].deviceClass = "G2S_meters";
            command.Command.setHostItem[0].ownedDevice1[0].deviceClass = "G2S_meters";

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenEqualDeviceIdAndHostIdExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.configurationId = 2;
            command.Command.setHostItem[0].guestDevice1[0].deviceId = 5;

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var handler = CreateHandler();
            var command = CreateCommand(StartDateTime, EndDateTime);
            command.Command.configurationId = 2;

            var existChangeLog = CreateCommChangeLog();
            ChangeLogRepositoryMock
                .Setup(x => x.Get(It.IsAny<DbContext>(), It.IsAny<Expression<Func<CommChangeLog, bool>>>()))
                .Returns(new List<CommChangeLog> { existChangeLog }.AsQueryable());

            ConfigureGetAllForChangeLogRepository();

            var result = await handler.Verify(command);

            Assert.AreEqual(result.Code, "G2S_CCX018");
        }

        private authorizeList CreateAuthorizeList()
        {
            var authorizeList = new authorizeList
            {
                authorizeItem =
                    new[]
                    {
                        new authorizeItem
                        {
                            hostId = 1,
                            timeoutDate = DateTime.MinValue,
                            timeoutAction =
                                t_timeoutActionTypes.G2S_abort
                        }
                    }
            };

            return authorizeList;
        }

        private void ConfigureGetAllForChangeLogRepository()
        {
            HostConfigItemRepositoryMock.Setup(x => x.GetAll(It.IsAny<DbContext>()))
                .Returns(
                    new List<CommHostConfigItem>
                    {
                        new CommHostConfigItem
                        {
                            HostId = 2,
                            HostRegistered = true,
                            HostIndex = 2
                        },
                        new CommHostConfigItem
                        {
                            HostId = 3,
                            HostRegistered = false,
                            HostIndex = 3
                        },
                        new CommHostConfigItem
                        {
                            HostId = 4,
                            HostRegistered = true,
                            HostIndex = 4
                        }
                    }.AsQueryable());
        }

        private SetCommChange CreateHandler()
        {
            var egm = HandlerUtilities.CreateMockEgm(ConfigDeviceMock);

            ConfigDeviceMock.SetupGet(d => d.Owner).Returns(TestConstants.HostId);

            var handler = new SetCommChange(
                egm,
                CommandBuilderMock.Object,
                ContextFactoryMock.Object,
                ChangeLogRepositoryMock.Object,
                EventLiftMock.Object,
                TaskSchedulerMock.Object,
                _configuration.Object,
                IdProvider.Object);

            return handler;
        }

        private ClassCommand<commConfig, setCommChange> CreateCommand(DateTime startDateTime, DateTime endDateTime)
        {
            var command = ClassCommandUtilities.CreateClassCommand<commConfig, setCommChange>(
                TestConstants.HostId,
                TestConstants.EgmId);

            command.Command.applyCondition = ApplyCondition.Disable.ToG2SString();
            command.Command.configurationId = 1;
            command.Command.disableCondition = DisableCondition.Immediate.ToG2SString();
            command.Command.authorizeList = null;
            command.Command.endDateTime = endDateTime;
            command.Command.startDateTime = startDateTime;
            command.Command.restartAfter = true;
            command.Command.setHostItem = new[]
            {
                ////new setHostItem
                ////{
                ////    hostId = 0,
                ////    hostIndex = 0,
                ////    allowMulticast = false,
                ////    hostRegistered = true,
                ////    hostLocation = "localhost"
                ////}, 

                new setHostItem
                {
                    hostId = 1,
                    hostIndex = 1,
                    allowMulticast = true,
                    hostRegistered = true,
                    hostLocation = "location_1",
                    displayCommFault = false,
                    useDefaultConfig = true,
                    noResponseTimer = 2,
                    requiredForPlay = true,
                    timeToLive = 1,
                    configDevice1 =
                        new[]
                        {
                            new c_setHostItem.configDevice
                            {
                                deviceId = 1,
                                deviceActive = true,
                                deviceClass = "G2S_communications"
                            }
                        },
                    ownedDevice1 = new[]
                    {
                        new c_setHostItem.ownedDevice
                        {
                            deviceId = 1,
                            deviceActive = true,
                            deviceClass = "G2S_eventHandler"
                        }
                    },
                    guestDevice1 = new[]
                    {
                        new c_setHostItem.guestDevice
                        {
                            deviceId = 1,
                            deviceActive = true,
                            deviceClass = "G2S_meters"
                        }
                    }
                }
            };

            return command;
        }
    }
}