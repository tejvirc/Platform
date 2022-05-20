namespace Aristocrat.Monaco.Kernel.Tests.AssemblyResolver
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for FileSystemPathNodeTest and is intended
    ///     to contain all FileSystemPathNodeTest Unit Tests
    /// </summary>
    [TestClass]
    public class FileSystemPathNodeTests
    {
        /// <summary>
        ///     A test for FileSystemPathNode Constructor
        /// </summary>
        [TestMethod]
        public void FileSystemPathNodeConstructorTest()
        {
            var target = new FileSystemPathNode();

            Assert.IsTrue(target.Recursive);
            Assert.AreEqual(string.Empty, target.FileSystemPath);
        }
    }
}