namespace Aristocrat.Monaco.Kernel.Tests.PathMapper
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for PathMappingNodeTest and is intended
    ///     to contain all PathMappingNodeTest Unit Tests
    /// </summary>
    [TestClass]
    public class PathMappingNodeTests
    {
        /// <summary>
        ///     A test for PlatformPath
        /// </summary>
        [TestMethod]
        public void PlatformPathTest()
        {
            var target = new PathMappingNode();

            Assert.AreEqual(string.Empty, target.FileSystemPath);
            Assert.AreEqual(string.Empty, target.PlatformPath);
            Assert.AreEqual(string.Empty, target.RelativeTo);
            Assert.AreEqual(string.Empty, target.AbsolutePathName);
        }
    }
}