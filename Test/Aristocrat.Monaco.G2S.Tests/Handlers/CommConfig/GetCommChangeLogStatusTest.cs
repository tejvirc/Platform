namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Data.CommConfig;
    using G2S.Handlers.CommConfig;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetCommChangeLogStatusTest
    {
        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<GetCommChangeLogStatus>();
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<ICommChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetCommChangeLogStatus(
                egm.Object,
                rep.Object,
                contextFactory.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithNoDeviceExpectError()
        {
            var egm = new Mock<IG2SEgm>();
            var rep = new Mock<ICommChangeLogRepository>();
            var contextFactory = new Mock<IMonacoContextFactory>();

            var handler = new GetCommChangeLogStatus(
                egm.Object,
                rep.Object,
                contextFactory.Object);

            await VerificationTests.VerifyChecksForNoDevice(handler);
        }
    }
}