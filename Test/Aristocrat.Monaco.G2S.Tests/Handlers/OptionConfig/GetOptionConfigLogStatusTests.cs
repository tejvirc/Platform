namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Protocol.v21;
    using Data.OptionConfig;
    using G2S.Handlers.OptionConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetOptionChangeLogStatusTest
    {
        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<getOptionChangeLogStatus>();
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetOptionChangeLogStatus(
                egm.Object,
                rep.Object,
                contextFactory.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetOptionChangeLogStatus(
                egm.Object,
                rep.Object,
                contextFactory.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }

        [TestMethod]
        public async Task WhenRunWithNoErrorExpectResponseValues()
        {
            var count = new Random().Next(1, 100);
            var seq = new Random().Next(1, 100);

            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();
            rep.Setup(r => r.Count(It.IsAny<DbContext>())).Returns(count);
            rep.Setup(r => r.GetMaxLastSequence<OptionChangeLog>(It.IsAny<DbContext>())).Returns(seq);

            var handler = new GetOptionChangeLogStatus(
                egm.Object,
                rep.Object,
                contextFactory.Object);
            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionChangeLogStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);
            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<optionConfig, optionChangeLogStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(count, response.Command.totalEntries);
            Assert.AreEqual(seq, response.Command.lastSequence);
        }

        [TestMethod]
        public async Task WhenRunWithEmptyLogExpectResponseValues()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<IOptionChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            rep.Setup(m => m.Count(It.IsAny<DbContext>())).Returns(0);

            var handler = new GetOptionChangeLogStatus(
                egm.Object,
                rep.Object,
                contextFactory.Object);

            var command = ClassCommandUtilities.CreateClassCommand<optionConfig, getOptionChangeLogStatus>(
                TestConstants.HostId,
                TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.FirstOrDefault() as ClassCommand<optionConfig, optionChangeLogStatus>;

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Command.totalEntries);
        }
    }
}