namespace Aristocrat.Monaco.Kernel.Tests.PathMapper
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using PathMapper = Kernel.PathMapper;

    [TestClass]
    [DoNotParallelize]
    public class PathMapperTests
    {
        private static readonly List<string> _unwantedFiles = new List<string> { "PlatformDirectories.addin.xml" };
        private static readonly string ExpectedExtensionPath = "/Kernel/PathMapping";

        private static string _goodDir1;
        private static string _goodDir2;
        private static string _goodDirDeeper;
        private static string _goodDirRelative1;
        private static string _goodDirRelative2;
        private static string _goodDirRelative3;
        private static string _goodDirRelative4;
        private static string _badDir;
        private static string _badDirRelative1;
        private static string _badDirRelative2;

        public TestContext TestContext { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var tempDir = Path.GetTempPath();

            _goodDir1 = Path.Combine(tempDir, "good1");
            _goodDir2 = Path.Combine(tempDir, "good2");
            _goodDirDeeper = Path.Combine(Path.Combine(tempDir, "good1"), "deeper");
            _badDir = Path.Combine(tempDir, "nothing");
            _goodDirRelative1 = Path.Combine(_goodDir1, "Referring");
            _goodDirRelative2 = Path.Combine(Path.Combine(_goodDirDeeper, "Referring"), "SubDir");
            _goodDirRelative3 = Path.Combine(_goodDir1, "SubKernel");
            _goodDirRelative4 = Path.Combine(Path.Combine(_goodDirDeeper, "SubTest"), "SubSubTest");
            _badDirRelative1 = Path.Combine(_goodDir1, "ReferringBad");
            _badDirRelative2 = Path.Combine(Path.Combine(_goodDir1, "Referring"), "Bad");

            Directory.CreateDirectory(_goodDir1);
            Directory.CreateDirectory(_goodDir2);
            Directory.CreateDirectory(_goodDirRelative1);
            Directory.CreateDirectory(_goodDirRelative2);
            Directory.CreateDirectory(_goodDirRelative3);
            Directory.CreateDirectory(_goodDirRelative4);
            Directory.CreateDirectory(_goodDirDeeper);

            DirectoryHelpers.VerifiedDirectoryDelete(_badDir);
            DirectoryHelpers.VerifiedDirectoryDelete(_badDirRelative1);
            DirectoryHelpers.VerifiedDirectoryDelete(_badDirRelative2);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            DirectoryHelpers.VerifiedDirectoryDelete(_goodDirRelative1);
            DirectoryHelpers.VerifiedDirectoryDelete(_goodDirRelative2);
            DirectoryHelpers.VerifiedDirectoryDelete(_goodDirRelative3);
            DirectoryHelpers.VerifiedDirectoryDelete(_goodDirRelative4);
            DirectoryHelpers.VerifiedDirectoryDelete(_goodDirDeeper);
            DirectoryHelpers.VerifiedDirectoryDelete(_goodDir2);
            DirectoryHelpers.VerifiedDirectoryDelete(_goodDir1);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _unwantedFiles.ForEach(u => { if (File.Exists(u)) File.Delete(u); });

            var currentDirectory = Directory.GetCurrentDirectory();
            AddinManager.Initialize(currentDirectory, currentDirectory, currentDirectory);
            AddinManager.Registry.Rebuild(null);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var pathMapper = GetPathMapper();

            var rootNode = GetRootNodeThroughReflection(pathMapper);
            Assert.AreEqual(0, rootNode.Children.Count);

            AssertPathMapper(pathMapper);
        }

        [TestMethod]
        public void NameTest()
        {
            var pathMapper = GetPathMapper();

            var rootNode = GetRootNodeThroughReflection(pathMapper);
            Assert.AreEqual(0, rootNode.Children.Count);

            AssertPathMapper(pathMapper);
        }

        [TestMethod]
        public void InitializeTest()
        {
            try
            {
                var testMappings = CreateTestMappings();
                BuildMappingFile(testMappings, TestContext.TestName);
                AddinManager.Registry.Rebuild(null);

                var pathMapper = GetPathMapper();
                var rootNode = GetRootNodeThroughReflection(pathMapper);
                Assert.AreEqual(ChildrenInRoot(), rootNode.Children.Count);

                AssertRootNode(rootNode, testMappings);
                AssertPathMapper(pathMapper);
            }
            finally
            {
                CleanupMappings(TestContext.TestName);
            }
        }

        [TestMethod]
        public void GetDirectoryTest()
        {
            try
            {
                var testMappings = CreateTestMappings();
                SetupMappings(TestContext.TestName, testMappings);
                var pathMapper = GetPathMapper();

                var directoryInfo = pathMapper.GetDirectory("/Kernel/Test");
                Assert.IsNotNull(directoryInfo);

                var expectedPath = testMappings["/Kernel/Test"];
                Assert.AreEqual(expectedPath, directoryInfo.FullName);

                var rootNode = GetRootNodeThroughReflection(pathMapper);
                Assert.AreEqual(ChildrenInRoot(), rootNode.Children.Count);

                AssertPathMapper(pathMapper);
            }
            finally
            {
                CleanupMappings(TestContext.TestName);
            }
        }

        [TestMethod]
        public void GetDirectoryWithUnmappedChildTest()
        {
            var testMappings = CreateTestMappings();

            try
            {
                SetupMappings("GetDirectoryWithUnmappedChildTest", testMappings);
                var pathMapper = GetPathMapper();

                // Create a child directory in whatever directory /Kernel/Test is mapped to
                // so that the unmapped directory will logically exist even though it is
                // not specifically mapped
                var dir = pathMapper.GetDirectory("/Kernel/Test");
                Directory.CreateDirectory(Path.Combine(dir.FullName, "Unmapped"));

                var directoryInfo = pathMapper.GetDirectory("/Kernel/Test/Unmapped");
                Assert.IsNotNull(directoryInfo);

                var expectedPath = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}{2}",
                    testMappings["/Kernel/Test"],
                    Path.DirectorySeparatorChar,
                    "Unmapped");
                Assert.AreEqual(expectedPath, directoryInfo.FullName);

                var rootNode = GetRootNodeThroughReflection(pathMapper);
                Assert.AreEqual(ChildrenInRoot(), rootNode.Children.Count);

                AssertPathMapper(pathMapper);
            }
            finally
            {
                CleanupMappings("GetDirectoryWithUnmappedChildTest");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetDirectoryWithUnmappedChildDoesNotExistTest()
        {
            var testMappings = CreateTestMappings();
            var addinFilename = "GetDirectoryWithUnmappedChildDoesNotExistTest";

            try
            {
                // After the directories are created add one that will map  
                // to a directory that does not exist
                testMappings.Add("/Kernel/Test/UnmappedDoesNotExist", _badDir);
                BuildMappingFile(testMappings, addinFilename);
                AddinManager.Registry.Rebuild(null);

                // Result does not matter. It should throw exception
                GetPathMapper();

                Assert.Fail("VerifyPlatformPath didn't throw exception for invalid path");
            }
            finally
            {
                CleanupMappings(addinFilename);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetDirectoryWithInvalidPlatformPathTest()
        {
            var pathMapper = GetPathMapper();

            pathMapper.GetDirectory("Kernel/Test/UnmappedDoesNotExist");
        }
        
        [TestMethod]
        public void GetDirectoryNotFoundTest()
        {
            var pathMapper = GetPathMapper();

            var directoryInfo = pathMapper.GetDirectory("/Kernel/Test");

            Assert.IsNull(directoryInfo);

            var rootNode = GetRootNodeThroughReflection(pathMapper);
            Assert.AreEqual(0, rootNode.Children.Count);

            AssertPathMapper(pathMapper);
        }

        [TestMethod]
        public void VerifyPlatformPathTest()
        {
            var pathParts =
                InvokePrivateStaticMethod(
                    "VerifyPlatformPath",
                    new[] { typeof(string) },
                    new object[] { "/Kernel/TestPath" }) as List<string>;

            Assert.IsNotNull(pathParts);
            Assert.AreEqual(2, pathParts.Count);
            Assert.AreEqual("Kernel", pathParts[0]);
            Assert.AreEqual("TestPath", pathParts[1]);
        }

        [TestMethod]
        public void VerifyPlatformPathWithInvalidPathTest()
        {
            var testPath = "Kernel/TestPath";

            try
            {
                InvokePrivateStaticMethod(
                    "VerifyPlatformPath",
                    new[] { typeof(string) },
                    new object[] { testPath });

                Assert.Fail("VerifyPlatformPath didn't throw exception for invalid path");
            }
            catch (ArgumentException argumentException)
            {
                Assert.IsFalse(string.IsNullOrEmpty(argumentException.Message));
                Assert.AreEqual("platformPath", argumentException.ParamName);
            }
        }

        [TestMethod]
        public void VerifyPlatformPathWithTrailingBackslashTest()
        {
            var pathParts = InvokePrivateStaticMethod(
                "VerifyPlatformPath",
                new[] { typeof(string) },
                new object[] { "/Kernel/TestPath/" }) as List<string>;

            Assert.IsNotNull(pathParts);
            Assert.AreEqual(2, pathParts.Count);
            Assert.AreEqual("Kernel", pathParts[0]);
            Assert.AreEqual("TestPath", pathParts[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWithInvalidPathTest()
        {
            var node = new PathNode("root");

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "Invalid/Path/Test", _goodDir1, string.Empty, string.Empty, false, node });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWithNullFileSystemPathTest()
        {
            var node = new PathNode("root");
            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Test/Path", null, string.Empty, string.Empty, false, node });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWithEmptyFileSystemPathTest()
        {
            var node = new PathNode("root");
            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Test/Path", string.Empty, string.Empty, string.Empty, false, node });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWithNullNodeTest()
        {
            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Test/Path", _goodDir1, string.Empty, string.Empty, false, null });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWhereBothReferencesExistTest()
        {
            var node = new PathNode("root");
            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referred", _goodDir1, string.Empty, string.Empty, false, node });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referring", _goodDir1, "/Referred", string.Empty, false, node });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWhereReferredDoesNotExistTest()
        {
            var node = new PathNode("root");

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referring", string.Empty, "/Referred", string.Empty, false, node });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWhereReferredIsIncorrectTest()
        {
            var node = new PathNode("root");

            // "Referred" should start with a '/'
            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referring", string.Empty, "/Referred", string.Empty, false, node });
        }

        [TestMethod]
        public void AddPathWhereReferredIsCorrectTest()
        {
            var rootNode = new PathNode("root");

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referred", _goodDir1, string.Empty, string.Empty, false, rootNode });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referring", string.Empty, "/Referred", string.Empty, false, rootNode });

            var expected = Path.Combine(rootNode.Children[0].FileSystemPath, "Referring");
            var actual = rootNode.Children[1].FileSystemPath;

            Assert.AreEqual(2, rootNode.Children.Count);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWhereFinalDirectoryDoesNotExistTest()
        {
            var rootNode = new PathNode("root");

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referred", _goodDir1, string.Empty, string.Empty, false, rootNode });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/ReferringBad", string.Empty, "/Referred", string.Empty, false, rootNode });
        }

        [TestMethod]
        public void AddPathWhereReferredIsCorrectAndReferrerIsCompositeTest()
        {
            var rootNode = new PathNode("root");

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referred", _goodDirDeeper, string.Empty, string.Empty, false, rootNode });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referring/SubDir", string.Empty, "/Referred", string.Empty, false, rootNode });

            var expected = Path.Combine(Path.Combine(rootNode.Children[0].FileSystemPath, "Referring"), "SubDir");
            var actual = rootNode.Children[1].Children[0].FileSystemPath;

            Assert.AreEqual(2, rootNode.Children.Count);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddCompositePathWhereFinalDirectoryDoesNotExistTest()
        {
            var rootNode = new PathNode("root");

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referred", _goodDir1, string.Empty, string.Empty, false, rootNode });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Referring/Bad", string.Empty, "/Referred", string.Empty, false, rootNode });
        }

        [TestMethod]
        public void AddPathWhereRootNodeDoesNotExistTest()
        {
            var platformPart1 = "Kernel";
            var platformPart2 = "Test";

            var testPlatformBasePath = "/" + platformPart1; // "/Kernel"
            var testPlatformPath = testPlatformBasePath + "/" + platformPart2; // "/Kernel/Test"
            var testFileSystemPath = _goodDir1;

            var node = new PathNode("root");

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { testPlatformBasePath, _goodDir2, string.Empty, string.Empty, false, node });

            Assert.AreEqual(1, node.Children.Count);

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { testPlatformPath, testFileSystemPath, string.Empty, string.Empty, false, node });

            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(1, node.Children.Count(tempNode => tempNode.Name == platformPart1));

            var kernelNode = node.Children.First(tempNode => tempNode.Name == platformPart1);

            Assert.AreEqual(1, kernelNode.Children.Count);
            Assert.AreEqual(
                1,
                kernelNode.Children.Count(
                    tempNode => (tempNode.Name == platformPart2) && (tempNode.FileSystemPath == testFileSystemPath)));
        }

        [TestMethod]
        public void AddPathWherePathAlreadyExistsAndEqualTest()
        {
            var expectedPart1 = "Test";
            var expectedPart2 = "Path";
            var repeatedFileSystemPath = _goodDir1;
            var repeatedPlatformPath = "/" + expectedPart1 + "/" + expectedPart2;

            var node = new PathNode(string.Empty);

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { repeatedPlatformPath, repeatedFileSystemPath, string.Empty, string.Empty, false, node });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { repeatedPlatformPath, repeatedFileSystemPath, string.Empty, string.Empty, false, node });

            Assert.AreEqual(1, node.Children.Count); // Nodes under /
            Assert.AreEqual(1, node.Children[0].Children.Count); // Nodes under /Test/

            Assert.AreEqual(expectedPart1, node.Children[0].Name);
            Assert.AreEqual(expectedPart2, node.Children[0].Children[0].Name);
            Assert.AreEqual(repeatedFileSystemPath, node.Children[0].Children[0].FileSystemPath);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPathWherePathAlreadyExistsAndDifferentTest()
        {
            var expectedPart1 = "Test";
            var expectedPart2 = "Path";
            var originalFileSystemPath = _goodDir1;
            var secondFileSystemPath = _goodDir2;
            var repeatedPlatformPath = "/" + expectedPart1 + "/" + expectedPart2;

            var node = new PathNode(string.Empty);

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { repeatedPlatformPath, originalFileSystemPath, string.Empty, string.Empty, false, node });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { repeatedPlatformPath, secondFileSystemPath, string.Empty, string.Empty, false, node });
        }

        [TestMethod]
        public void AddPathWhereAllNodesOfPlatformPathAlreadyCreatedTest()
        {
            var expectedName = "Test";
            var expectedFileSystemPath = _goodDir1;
            var anotherFileSystemPath = _goodDir2;

            var node = new PathNode(string.Empty);

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Test/Path/OneMore", anotherFileSystemPath, string.Empty, string.Empty, false, node });

            InvokePrivateStaticMethod(
                "AddPath",
                new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                new object[] { "/Test", expectedFileSystemPath, string.Empty, string.Empty, false, node });

            Assert.AreEqual(1, node.Children.Count); // Nodes under /
            Assert.AreEqual(1, node.Children[0].Children.Count); // Nodes under /Test/
            Assert.AreEqual(1, node.Children[0].Children[0].Children.Count); // Nodes under /Test/Path

            Assert.AreEqual(expectedName, node.Children[0].Name);
            Assert.AreEqual(expectedFileSystemPath, node.Children[0].FileSystemPath);
        }

        [TestMethod]
        public void AddPathWhereSomeNodesOfPlatformPathAlreadyCreatedTest()
        {
            var mappings = new Dictionary<string, string>
            {
                { "/Test/Path/OneMore", _goodDir1 },
                {
                    "/Test/Path/OneMore/OKJustAnother/SeriouslyLastOne",
                    _goodDir2
                }
            };

            var node = new PathNode(string.Empty);

            foreach (var mapping in mappings)
            {
                InvokePrivateStaticMethod(
                    "AddPath",
                    new[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(PathNode) },
                    new object[] { mapping.Key, mapping.Value, string.Empty, string.Empty, false, node });
            }

            Assert.IsTrue(VerifyTree(node, mappings));
        }

        [TestMethod]
        public void RootNodeOfPathAlreadyDefinedTest()
        {
            var expectedPlatformPath = "/Kernel/Test/Test2";
            var expectedFileSystemPath = _goodDir1;

            var testMappings = new Dictionary<string, string>
            {
                { "/Kernel", _goodDir2 },
                {
                    expectedPlatformPath,
                    expectedFileSystemPath
                }
            };

            try
            {
                SetupMappings(TestContext.TestName, testMappings);
                var pathMapper = GetPathMapper();
                var rootNode = GetRootNodeThroughReflection(pathMapper);
                Assert.IsTrue(VerifyTree(rootNode, testMappings));

                var actual = pathMapper.GetDirectory(expectedPlatformPath);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expectedFileSystemPath, actual.FullName);

                // Also assert that in the process of creating the platform path it
                // did not erroneously assign a path to the intermediate node
                var test = pathMapper.GetDirectory("/Kernel/Test");
                Assert.IsNull(test);
            }
            finally
            {
                CleanupMappings(TestContext.TestName);
            }
        }

        private object InvokePrivateStaticMethod(string methodName, Type[] parameterTypes, object[] arguments)
        {
            var pathMapper = GetPathMapper();

            var pathMapperPrivateObject = new PrivateObject(pathMapper);

            var result = pathMapperPrivateObject.Invoke(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic,
                parameterTypes,
                arguments);

            return result;
        }

        private bool VerifyTree(PathNode rootNode, Dictionary<string, string> mappings)
        {
            var countedNodes = new List<string>();

            foreach (var mapping in mappings)
            {
                var parts =
                    InvokePrivateStaticMethod(
                        "VerifyPlatformPath",
                        new[] { typeof(string) },
                        new object[] { mapping.Key }) as List<string>;

                var full = string.Empty;
                foreach (var part in parts)
                {
                    full += "|" + part;
                    if (!countedNodes.Contains(full))
                    {
                        countedNodes.Add(full);
                    }
                }
            }

            var treeCount = WalkTheTree(rootNode);

            // Make sure we count the "root" node in our countedNodes because it will
            // not be counted in the platform path deconstructions
            return treeCount == countedNodes.Count + 1;
        }

        private static int WalkTheTree(PathNode node)
        {
            var totalCount = 1;

            if (node.Children.Count > 0)
            {
                foreach (var child in node.Children)
                {
                    totalCount += WalkTheTree(child);
                }
            }

            return totalCount;
        }

        private static void AssertRootNode(PathNode rootNode, Dictionary<string, string> mappings)
        {
            Assert.AreEqual(ChildrenInRoot(), rootNode.Children.Count);

            Assert.AreEqual("Kernel", rootNode.Children.ElementAt(0).Name);
            Assert.AreEqual(mappings["/Kernel"], rootNode.Children.ElementAt(0).FileSystemPath);
            Assert.AreEqual("Hardware", rootNode.Children.ElementAt(1).Name);
            Assert.AreEqual(mappings["/Hardware"], rootNode.Children.ElementAt(1).FileSystemPath);
            Assert.AreEqual("One", rootNode.Children.ElementAt(2).Name);
            Assert.AreEqual(string.Empty, rootNode.Children.ElementAt(2).FileSystemPath);
            Assert.AreEqual("Three", rootNode.Children.ElementAt(3).Name);
            Assert.AreEqual(string.Empty, rootNode.Children.ElementAt(3).FileSystemPath);

            var kernelNode = rootNode.Children.ElementAt(0);
            Assert.AreEqual(1, kernelNode.Children.Count);
            Assert.AreEqual("Test", kernelNode.Children.ElementAt(0).Name);
            Assert.AreEqual(mappings["/Kernel/Test"], kernelNode.Children.ElementAt(0).FileSystemPath);

            var testNode = kernelNode.Children.ElementAt(0);
            Assert.AreEqual(1, testNode.Children.Count);
            Assert.AreEqual("Kernel", testNode.Children.ElementAt(0).Name);
            Assert.AreEqual(mappings["/Kernel/Test/Kernel"], testNode.Children.ElementAt(0).FileSystemPath);

            var oneNode = rootNode.Children.ElementAt(2);
            Assert.AreEqual(1, oneNode.Children.Count);
            Assert.AreEqual("Two", oneNode.Children.ElementAt(0).Name);
            Assert.AreEqual(string.Empty, oneNode.Children.ElementAt(0).FileSystemPath);

            var twoNode = oneNode.Children.ElementAt(0);
            Assert.AreEqual(1, twoNode.Children.Count);
            Assert.AreEqual("Three", twoNode.Children.ElementAt(0).Name);
            Assert.AreEqual(mappings["/One/Two/Three"], twoNode.Children.ElementAt(0).FileSystemPath);

            var threeNode = rootNode.Children.ElementAt(3);
            Assert.AreEqual(1, threeNode.Children.Count);
            Assert.AreEqual("Two", threeNode.Children.ElementAt(0).Name);
            Assert.AreEqual(string.Empty, threeNode.Children.ElementAt(0).FileSystemPath);

            var secondTwoNode = threeNode.Children.ElementAt(0);
            Assert.AreEqual(1, secondTwoNode.Children.Count);
            Assert.AreEqual("One", secondTwoNode.Children.ElementAt(0).Name);
            Assert.AreEqual(mappings["/Three/Two/One"], secondTwoNode.Children.ElementAt(0).FileSystemPath);

            var subKernelNode = rootNode.Children.ElementAt(4);
            var subKernelExpectedPath = Path.Combine(mappings["/Kernel"], "SubKernel");
            Assert.AreEqual(subKernelExpectedPath, subKernelNode.FileSystemPath);

            var subTestNode = rootNode.Children.ElementAt(5);
            Assert.AreEqual(1, subTestNode.Children.Count);
            Assert.AreEqual("SubSubTest", subTestNode.Children.ElementAt(0).Name);
            var subTestExpectedPath = Path.Combine(Path.Combine(mappings["/Kernel/Test"], "SubTest"), "SubSubTest");
            Assert.AreEqual(subTestExpectedPath, subTestNode.Children.ElementAt(0).FileSystemPath);
        }

        private static int ChildrenInRoot()
        {
            return 6;
        }

        private static Dictionary<string, string> CreateTestMappings()
        {
            return new Dictionary<string, string>
            {
                { "/Kernel", _goodDir1 },
                { "/Hardware", _goodDir2 },
                { "/Kernel/Test", _goodDirDeeper },

                // Test name repeated in the path
                { "/Kernel/Test/Kernel", _goodDir1 },

                // Test different paths with same parts
                { "/One/Two/Three", _goodDir1 },
                { "/Three/Two/One", _goodDir2 },

                // Test relative paths
                { "/SubKernel", ">/Kernel" },
                { "/SubTest/SubSubTest", ">/Kernel/Test" }
            };
        }

        private static void SetupMappings(string addinIdToUse, Dictionary<string, string> mappings)
        {
            BuildMappingFile(mappings, addinIdToUse);
            AddinManager.Registry.Rebuild(null);
        }

        private static void BuildMappingFile(Dictionary<string, string> mappings, string addinId)
        {
            var filename = $"{addinId}.addin.xml";

            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                    sw.WriteLine("<Addin id=\"{0}\" namespace=\"Client12Addins\" version=\"1.0\">", addinId);
                    sw.WriteLine("    <Dependencies>");
                    sw.WriteLine("        <Addin id=\"Monaco.Kernel\" version=\"1.0\" />");
                    sw.WriteLine("    </Dependencies>");
                    sw.WriteLine("    <Extension path=\"/Kernel/PathMapping\">");

                    foreach (var mapping
                        in mappings.Where(x => !x.Value.StartsWith(">", StringComparison.Ordinal)))
                    {
                        sw.WriteLine(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "        <PathMapping platformPath=\"{0}\" fileSystemPath=\"{1}\" />",
                                mapping.Key,
                                mapping.Value));
                    }

                    foreach (var mapping
                        in mappings.Where(x => x.Value.StartsWith(">", StringComparison.Ordinal)))
                    {
                        sw.WriteLine(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "        <PathMapping platformPath=\"{0}\" relativeTo=\"{1}\" />",
                                mapping.Key,
                                mapping.Value.Substring(1))); // Skip the ">"
                    }

                    sw.WriteLine(@"    </Extension>");
                    sw.WriteLine(@"</Addin>");
                }
            }
        }

        private static void AssertPathMapper(PathMapper pathMapper)
        {
            var pathMapperPrivateObject = new PrivateObject(pathMapper);

            Assert.AreEqual("Path Mapper", pathMapper.Name);
            Assert.AreEqual(1, pathMapper.ServiceTypes.Count);
            Assert.IsTrue(pathMapper.ServiceTypes.Contains(typeof(IPathMapper)));

            var pathMappingExtensionPath =
                pathMapperPrivateObject.GetField(
                    "PathMappingExtensionPath",
                    BindingFlags.Static | BindingFlags.NonPublic) as string;
            Assert.AreEqual(ExpectedExtensionPath, pathMappingExtensionPath);
        }

        private static PathMapper GetPathMapper()
        {
            var mapper = (PathMapper)MonoAddinsHelper.GetSingleTypeExtensionNode("/Kernel/PathMapper").CreateInstance();
            mapper.Initialize();
            return mapper;
        }

        private static PathNode GetRootNodeThroughReflection(PathMapper mapper)
        {
            var pathMapperType = typeof(PathMapper);
            var fieldInfo = pathMapperType.GetField("_rootNode", BindingFlags.NonPublic | BindingFlags.Instance);

            return (PathNode)fieldInfo.GetValue(mapper);
        }

        private static void CleanupMappings(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                var current = Directory.GetCurrentDirectory();
                filename = string.Format(CultureInfo.InvariantCulture, "{0}{1}", filename, ".addin.xml");
                var file = Path.Combine(current, filename);
                if (!File.Exists(file))
                {
                    return;
                }

                File.Delete(file);
            }
        }
    }
}
