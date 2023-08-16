namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Data.Model;
    using Data.OptionConfig;
    using G2S.Handlers;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetOptionChangeLogTest
    {
        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetOptionChangeLog>();
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetOptionChangeLog(egm.Object, rep.Object, contextFactory.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetOptionChangeLog(egm.Object, rep.Object, contextFactory.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<IOptionConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);
            var egm = HandlerUtilities.CreateMockEgm(device);
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetOptionChangeLog(egm, rep.Object, contextFactory.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenRunWithNoErrorExpectResponseValues()
        {
            var count = 100;
            var seq = 100;

            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var logs = CreateOptionChangeLog(count);
            rep.Setup(r => r.GetAll(It.IsAny<DbContext>())).Returns(logs.AsQueryable());

            var handler = new GetOptionChangeLog(egm.Object, rep.Object, contextFactory.Object);
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionChangeLog>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.lastSequence = seq;
            command.Command.totalEntries = count;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<optionConfig, optionChangeLogList>;

            Assert.IsNotNull(response);
            Assert.AreNotEqual(null, response);
            Assert.AreEqual(command.Command.totalEntries, response.Command.optionChangeLog.Length);

            logs = logs.OrderByDescending(x => x.Id).ToList();

            for (var i = 0; i < command.Command.totalEntries; i++)
            {
                var log = logs.ElementAt(i);
                var responseLog = response.Command.optionChangeLog[i];

                Assert.AreEqual(log.Id, responseLog.logSequence);
                Assert.AreEqual(log.DeviceId, responseLog.deviceId);
                Assert.AreEqual(log.TransactionId, responseLog.transactionId);
                Assert.AreEqual(log.ConfigurationId, responseLog.configurationId);
                Assert.AreEqual(log.ApplyCondition.ToG2SString(), responseLog.applyCondition);
                Assert.AreEqual(log.DisableCondition.ToG2SString(), responseLog.disableCondition);
                Assert.AreEqual(log.StartDateTime, responseLog.startDateTime);
                Assert.AreEqual(log.EndDateTime, responseLog.endDateTime);
                Assert.AreEqual(log.RestartAfter, responseLog.restartAfter);
                Assert.AreEqual((int)log.ChangeStatus, (int)responseLog.changeStatus);
                Assert.AreEqual(log.ChangeDateTime, responseLog.changeDateTime);
                Assert.AreEqual((int)log.ChangeException, responseLog.changeException);

                Assert.AreEqual(log.AuthorizeItems.Count, responseLog.authorizeStatusList.authorizeStatus.Length);

                for (var j = 0; j < log.AuthorizeItems.Count; j++)
                {
                    var auth = log.AuthorizeItems.ElementAt(j);
                    var responseAuth = responseLog.authorizeStatusList.authorizeStatus[j];

                    Assert.AreEqual(auth.HostId, responseAuth.hostId);
                    Assert.AreEqual((int)auth.AuthorizeStatus, (int)responseAuth.authorizationState);
                    Assert.AreEqual(auth.TimeoutDate, responseAuth.timeoutDate);
                }
            }
        }

        private ICollection<OptionChangeLog> CreateOptionChangeLog(int count)
        {
            var log = new List<OptionChangeLog>();

            for (int i = 1; i <= count; i++)
            {
                log.Add(
                    new OptionChangeLog
                    {
                        Id = i,
                        TransactionId = i,
                        ApplyCondition = ApplyCondition.Cancel,
                        ChangeData = "ChangeData",
                        ChangeDateTime = DateTime.MaxValue,
                        ChangeException = ChangeExceptionErrorCode.EgmMustBeDisabled,
                        ChangeStatus = ChangeStatus.Aborted,
                        ConfigurationId = i,
                        DeviceId = i,
                        DisableCondition = DisableCondition.Idle,
                        EgmActionConfirmed = true,
                        StartDateTime = DateTime.MaxValue,
                        EndDateTime = DateTime.MaxValue,
                        RestartAfter = true,
                        AuthorizeItems = new List<ConfigChangeAuthorizeItem>
                        {
                            new ConfigChangeAuthorizeItem
                            {
                                AuthorizeStatus = AuthorizationState.Timeout,
                                CommChangeLogId = 1,
                                HostId = 1,
                                Id = 1,
                                OptionChangeLogId = 1,
                                TimeoutAction = TimeoutActionType.Ignore,
                                TimeoutDate = DateTime.MaxValue
                            },
                            new ConfigChangeAuthorizeItem
                            {
                                AuthorizeStatus = AuthorizationState.Timeout,
                                CommChangeLogId = 1,
                                HostId = 1,
                                Id = 1,
                                OptionChangeLogId = 1,
                                TimeoutAction = TimeoutActionType.Ignore,
                                TimeoutDate = DateTime.MaxValue
                            },
                        },
                    });
            }

            return log;
        }
    }
}