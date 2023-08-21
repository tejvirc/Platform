namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for SerializableImportNodeTest
    /// </summary>
    [TestClass]
    public class SerializableImportNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new SerializableImportNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected = "Test string";
            var target = new SerializableImportNode(expected);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.Assembly);
        }

        [TestMethod]
        public void AssemblyPropertyTest()
        {
            string expected = "Test string";
            var target = new SerializableImportNode();
            Assert.IsNull(target.Assembly);

            target.Assembly = expected;
            Assert.AreEqual(expected, target.Assembly);
        }
    }
}