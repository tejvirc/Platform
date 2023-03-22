namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
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
    using Data.CommConfig;
    using Data.Model;
    using FizzWare.NBuilder;
    using G2S.Handlers;
    using G2S.Handlers.CommConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetCommChangeLogTest
    {
        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetCommChangeLog>();
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<ICommChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetCommChangeLog(egm.Object, rep.Object, contextFactory.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<ICommChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetCommChangeLog(egm.Object, rep.Object, contextFactory.Object);
            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithValidCommandExpectNoError()
        {
            var device = new Mock<ICommConfigDevice>();
            device.SetupGet(d => d.Owner).Returns(TestConstants.HostId);
            device.SetupGet(d => d.Id).Returns(TestConstants.HostId);
            var egm = HandlerUtilities.CreateMockEgm(device);
            var rep = new Mock<ICommChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetCommChangeLog(egm, rep.Object, contextFactory.Object);
            await VerificationTests.VerifyCanSucceed(handler);
        }

        [TestMethod]
        public async Task WhenRunWithNoErrorExpectResponseValues()
        {
            var count = new Random().Next(1, 100);

            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<ICommChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var logs = Builder<CommChangeLog>.CreateListOfSize(count).All().Do(
                l =>
                {
                    l.AuthorizeItems = (ICollection<ConfigChangeAuthorizeItem>)Builder<ConfigChangeAuthorizeItem>
                        .CreateListOfSize(2)
                        .Build().AsEnumerable();
                }).Build().OrderByDescending(l => l.TransactionId).AsQueryable();
            rep.Setup(r => r.GetAll(It.IsAny<DbContext>())).Returns(logs);

            var handler = new GetCommChangeLog(egm.Object, rep.Object, contextFactory.Object);
            var command = ClassCommandUtilities.CreateClassCommand<commConfig, getCommChangeLog>(
                TestConstants.HostId,
                TestConstants.EgmId);
            command.Command.lastSequence = logs.First().Id;
            command.Command.totalEntries = count;

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<commConfig, commChangeLogList>;

            Assert.IsNotNull(response);
            Assert.AreEqual(count, response.Command.commChangeLog.Length);

            for (var i = 0; i < logs.Count(); i++)
            {
                var log = logs.ElementAt(i);
                var responseLog = response.Command.commChangeLog[i];

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

                Assert.AreEqual(log.AuthorizeItems.Count(), responseLog.authorizeStatusList.authorizeStatus.Length);

                for (var j = 0; j < log.AuthorizeItems.Count(); j++)
                {
                    var auth = log.AuthorizeItems.ElementAt(j);
                    var responseAuth = responseLog.authorizeStatusList.authorizeStatus[j];

                    Assert.AreEqual(auth.HostId, responseAuth.hostId);
                    Assert.AreEqual((int)auth.AuthorizeStatus, (int)responseAuth.authorizationState);
                    Assert.AreEqual(auth.TimeoutDate, responseAuth.timeoutDate);
                }
            }
        }
    }
}