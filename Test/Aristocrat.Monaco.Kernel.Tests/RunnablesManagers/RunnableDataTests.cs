namespace Aristocrat.Monaco.Kernel.Tests.RunnablesManagers
{
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Definition of the RunnableDataTest class.
    /// </summary>
    [TestClass]
    public class RunnableDataTests : BaseTestClass
    {
        /// <summary>
        ///     A test for the constructor.
        /// </summary>
        [TestMethod]
        public void ConstructorTest()
        {
            var mockedRunnable = new Mock<IRunnable>(MockBehavior.Strict);
            var thread = new Thread(mockedRunnable.Object.Run);
            var runnableData = new RunnableData(mockedRunnable.Object, thread);
            Assert.AreEqual(mockedRunnable.Object, runnableData.Runnable);
            Assert.AreEqual(thread, runnableData.Thread);
        }

        /// <summary>
        ///     A test for setting and getting the Runnable property.
        /// </summary>
        [TestMethod]
        public void SetAndGetRunnablePropertyTest()
        {
            var mockedRunnable = new Mock<IRunnable>(MockBehavior.Strict);
            var runnableData = new RunnableData(null, null);
            runnableData.Runnable = mockedRunnable.Object;
            Assert.AreEqual(mockedRunnable.Object, runnableData.Runnable);
            Assert.AreEqual((object)null, runnableData.Thread);
        }

        /// <summary>
        ///     A test for setting and getting the Thread property.
        /// </summary>
        [TestMethod]
        public void SetAndGetThreadPropertyTest()
        {
            var mockedRunnable = new Mock<IRunnable>(MockBehavior.Strict);
            var runnableData = new RunnableData(null, null);
            var thread = new Thread(mockedRunnable.Object.Run);
            runnableData.Thread = thread;
            Assert.AreEqual((object)null, runnableData.Runnable);
            Assert.AreEqual(thread, runnableData.Thread);
        }
    }
}