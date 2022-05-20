namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for SerializableAddinDependencyNodeTest
    /// </summary>
    [TestClass]
    public class SerializableAddinDependencyNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new SerializableAddinDependencyNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected1 = "Test id";
            string expected2 = "Test version";
            var target = new SerializableAddinDependencyNode(expected1, expected2);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected1, target.Id);
            Assert.AreEqual(expected2, target.Version);
        }

        [TestMethod]
        public void IdPropertyTest()
        {
            string expected = "Test id";
            var target = new SerializableAddinDependencyNode();
            Assert.IsNull(target.Id);

            target.Id = expected;
            Assert.AreEqual(expected, target.Id);
        }

        [TestMethod]
        public void VersionPropertyTest()
        {
            string expected = "Test version";
            var target = new SerializableAddinDependencyNode();
            Assert.IsNull(target.Version);

            target.Version = expected;
            Assert.AreEqual(expected, target.Version);
        }
    }
}