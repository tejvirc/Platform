namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    #endregion

    /// <summary>
    ///     Summary description for AddinConfigurationGroupReferenceNodeTest
    /// </summary>
    [TestClass]
    public class AddinConfigurationGroupReferenceNodeTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Constructor1Test()
        {
            var target = new AddinConfigurationGroupReferenceNode();
            Assert.IsNotNull(target);
        }

        [TestMethod]
        public void Constructor2Test()
        {
            string expected1 = "Test name";
            var target = new AddinConfigurationGroupReferenceNode(expected1);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected1, target.Name);
            Assert.IsFalse(target.Optional);
        }

        [TestMethod]
        public void Constructor3Test()
        {
            string expected1 = "Test name";
            bool expected2 = true;
            var target = new AddinConfigurationGroupReferenceNode(expected1, expected2);
            Assert.IsNotNull(target);
            Assert.AreEqual(expected1, target.Name);
            Assert.IsTrue(target.Optional);
        }

        [TestMethod]
        public void NameTest()
        {
            string expected = "Test name";
            var target = new AddinConfigurationGroupReferenceNode();
            Assert.IsNull(target.Name);

            target.Name = expected;
            Assert.AreEqual(expected, target.Name);
        }

        [TestMethod]
        public void OptionalPropertyTest()
        {
            bool expected = true;
            var target = new AddinConfigurationGroupReferenceNode();
            Assert.IsFalse(target.Optional);

            target.Optional = expected;
            Assert.IsTrue(target.Optional);
        }
    }
}