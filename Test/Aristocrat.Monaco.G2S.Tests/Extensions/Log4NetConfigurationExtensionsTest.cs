namespace Aristocrat.Monaco.G2S.Tests.Extensions
{
    using log4net;
    using Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class Log4NetConfigurationExtensionsTest
    {
        [TestMethod]
        public void WhenAddAsTraceSourceExpectNoException()
        {
            var logMock = new Mock<ILog>();
            var log = logMock.Object;

            log.AddAsTraceSource();
        }
    }
}