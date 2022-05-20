namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using G2S.Consumers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SharedConsumerContextTest
    {
        [TestMethod]
        public void WhenInitializeSharedConsumerContextExpectSuccess()
        {
            var context = new SharedConsumerContext();

            context.Initialize();
        }
    }
}