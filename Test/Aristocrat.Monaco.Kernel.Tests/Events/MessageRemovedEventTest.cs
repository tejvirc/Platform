namespace Aristocrat.Monaco.Kernel.Tests.Events
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Kernel.MessageDisplay;
    using Kernel.Contracts.MessageDisplay;

    #endregion

    /// <summary>
    ///     Summary description for MessageRemovedEventTest
    /// </summary>
    [TestClass]
    public class MessageRemovedEventTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void ConstructorTest()
        {
            var expected = new DisplayableMessage(
                () => "test",
                DisplayableMessageClassification.Informative,
                DisplayableMessagePriority.Normal);
            var target = new MessageRemovedEvent(expected);

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.Message);
        }
    }
}