namespace Aristocrat.Monaco.Kernel.Tests
{
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for SystemShutdownRequestedEventTest and is intended
    ///     to contain all SystemShutdownRequestedEventTest Unit Tests
    /// </summary>
    [TestClass]
    public class SystemShutdownRequestedEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        ///     A test for SystemShutdownRequestedEvent Constructor
        /// </summary>
        [TestMethod]
        public void SystemShutdownRequestedEventConstructorTest()
        {
            var target = new ExitRequestedEvent(ExitAction.ShutDown);
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(ExitRequestedEvent));
        }
    }
}