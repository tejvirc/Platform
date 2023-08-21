namespace Aristocrat.Monaco.Kernel.Tests.PathMapper
{
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///     This is a test class for PathNodeTest and is intended
    ///     to contain all PathNodeTest Unit Tests
    /// </summary>
    [TestClass]
    public class PathNodeTests
    {
        /// <summary>
        ///     A test for PathNode Constructor with just a pathnode name;
        /// </summary>
        [TestMethod]
        public void PathNodeConstructorWithPathNodeNameOnlyTest()
        {
            var name = "TestNodeName";
            var target = new PathNode(name);

            Assert.AreEqual(string.Empty, target.FileSystemPath);
            Assert.AreEqual(name, target.Name);
        }

        /// <summary>
        ///     A test for PathNode Constructor with a path node name and file system path
        /// </summary>
        [TestMethod]
        public void PathNodeConstructorWithPathNodeNameAndFileSystemPathTest()
        {
            var name = "TestNodeName";
            var fileSystemPath = "C:\\TestPath";
            var target = new PathNode(name) { FileSystemPath = fileSystemPath };

            Assert.AreEqual(fileSystemPath, target.FileSystemPath);
            Assert.AreEqual(name, target.Name);
        }

        /// <summary>
        ///     A test for Name
        /// </summary>
        [TestMethod]
        public void NameTest()
        {
            var expected = "target";
            var target = new PathNode(expected);

            Assert.AreEqual(expected, target.Name);

            expected = "new";

            target.Name = expected;

            Assert.AreEqual(expected, target.Name);
            Assert.AreEqual(string.Empty, target.FileSystemPath);
        }

        /// <summary>
        ///     A test for FileSystemPath
        /// </summary>
        [TestMethod]
        public void FileSystemPathTest()
        {
            var expected = "C:\\Test";
            var name = "target";
            var target = new PathNode(name) { FileSystemPath = expected };

            Assert.AreEqual(expected, target.FileSystemPath);

            expected = "C:\\NewPath";

            target.FileSystemPath = expected;

            Assert.AreEqual(expected, target.FileSystemPath);
            Assert.AreEqual(name, target.Name);
        }

        /// <summary>
        ///     A test for Children
        /// </summary>
        [TestMethod]
        public void ChildrenTest()
        {
            var name = "test node";
            var node = new PathNode(name);

            Assert.AreEqual(0, node.Children.Count);
            Assert.AreEqual(name, node.Name);
            Assert.AreEqual(string.Empty, node.FileSystemPath);
        }

        /// <summary>
        ///     A test for AddPathNode
        /// </summary>
        [TestMethod]
        public void AddPathNodeTest()
        {
            var nodeName = "node";
            var node = new PathNode(nodeName);

            Assert.AreEqual(0, node.Children.Count);
            Assert.AreEqual("node", node.Name);
            Assert.AreEqual(string.Empty, node.FileSystemPath);

            var actualNode = node.AddPathNode("test");

            Assert.IsNotNull(actualNode);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(1, node.Children.Count(childNode => childNode.Name == "test"));
            Assert.AreEqual(nodeName, node.Name);
            Assert.AreEqual(string.Empty, node.FileSystemPath);
        }

        /// <summary>
        ///     A test for TryGetChildNode
        /// </summary>
        [TestMethod]
        public void TryGetChildNodeWhereChildExistsTest()
        {
            var node = new PathNode("node");

            Assert.AreEqual(0, node.Children.Count);
            Assert.AreEqual("node", node.Name);
            Assert.AreEqual(string.Empty, node.FileSystemPath);

            node.AddPathNode("child");

            Assert.AreEqual(1, node.Children.Count);
            PathNode actualNode;
            var tryGetChildNodeResult = node.TryGetChildNode("child", out actualNode);

            Assert.IsTrue(tryGetChildNodeResult);
            Assert.AreEqual("child", actualNode.Name);
            Assert.AreEqual(string.Empty, actualNode.FileSystemPath);
            Assert.AreEqual(0, actualNode.Children.Count);
        }

        /// <summary>
        ///     A test for TryGetChildNode
        /// </summary>
        [TestMethod]
        public void TryGetChildNodeWhereChildDoesNotExistTest()
        {
            var node = new PathNode("node");

            Assert.AreEqual(0, node.Children.Count);
            Assert.AreEqual("node", node.Name);
            Assert.AreEqual(string.Empty, node.FileSystemPath);

            PathNode actualNode;
            var tryGetChildNodeResult = node.TryGetChildNode("child", out actualNode);

            Assert.IsFalse(tryGetChildNodeResult);
            Assert.IsNull(actualNode);
        }
    }
}