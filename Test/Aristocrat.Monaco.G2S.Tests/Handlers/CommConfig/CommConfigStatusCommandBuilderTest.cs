namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using Aristocrat.G2S.Client;
    using G2S.Handlers.CommConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CommConfigStatusCommandBuilderTest
    {
        private readonly Mock<IDisableConditionSaga> _configurationMode = new Mock<IDisableConditionSaga>();

        private readonly Mock<IG2SEgm> _egmMock = new Mock<IG2SEgm>();

        [TestMethod]
        public void WhenConstructWithNullsExpectException()
        {
            ConstructorTest.TestConstructorNullChecks<CommConfigStatusCommandBuilder>();
        }

        [TestMethod]
        public void WhenConstructWithValidParamsExpectSuccess()
        {
            var commandBuilder = new CommConfigStatusCommandBuilder(_egmMock.Object, _configurationMode.Object);

            Assert.IsNotNull(commandBuilder);
        }

        // TODO: add tests for build results
    }
}