namespace Aristocrat.Monaco.Kernel.Tests.AssemblyResolver
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;
    using AssemblyResolver = Kernel.AssemblyResolver;

    [TestClass]
    [DoNotParallelize]
    public class AssemblyResolverTests
    {
        private const string DirectoryToSearch = "AssemblyResolver/DirectoryToSearch";
        private readonly string _anotherDirectoryToSearch = Path.Combine(DirectoryToSearch, "AnotherDirectoryToSearch");

        private AssemblyResolver _target;
        private TestPropertiesManager _testPropertiesManager;

        [TestInitialize]
        public void TestInitialize()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            AddinManager.Initialize(currentDirectory, currentDirectory, currentDirectory);
            AddinManager.Registry.Rebuild(null);

            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _testPropertiesManager = new TestPropertiesManager();
            MoqServiceManager.AddService<IPropertiesManager>(_testPropertiesManager);
        }

        [TestCleanup]
        public void TestFinalize()
        {
            MoqServiceManager.RemoveInstance();
            var resolveEventHandler = GetResolveEventHandler(AppDomain.CurrentDomain);
            var invocationList = resolveEventHandler.GetInvocationList();
            foreach (var del in invocationList)
            {
                if (del.Method.Name == "AssemblyResolveHandler")
                {
                    var eventInfo = AppDomain.CurrentDomain.GetType().GetEvent("AssemblyResolve");
                    eventInfo.RemoveEventHandler(AppDomain.CurrentDomain, del);
                }
            }

            _target?.Dispose();
            AddinManager.Shutdown();
        }

        [TestMethod]
        public void InitializeTest()
        {
            _target = CreateAssemblyResolver(true);
            Assert.IsTrue(VerifyDelegateIsPresent(_target));
        }

        [TestMethod]
        public void DisposeTest()
        {
            _target = CreateAssemblyResolver(true);

            _target.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void ExpectExceptionWhenAssemblyIsNotFound()
        {
            _target = CreateAssemblyResolver(true);

            var result = Assembly.Load("ABogusAssembly");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ExpectNotNullWhenAssemblyIsFound()
        {
            var fileNameMappings = new Dictionary<string, string>
            {
                {
                    Path.Combine(
                        _anotherDirectoryToSearch,
                        "AnotherAssemblyToFind.tmp"),
                    Path.Combine(
                        _anotherDirectoryToSearch,
                        "AnotherAssemblyToFind.dll")
                }
            };

            using (new FileMover(fileNameMappings, true))
            {
                _target = CreateAssemblyResolver(true);

                var result = Assembly.Load("AnotherAssemblyToFind");

                Assert.IsNotNull(result);
            }
        }

        private static void GetAssemblyResolver(out AssemblyResolver resolver)
        {
            var node = MonoAddinsHelper.GetSingleSelectedExtensionNode<TypeExtensionNode>("/Kernel/AssemblyResolver");

            if (node != null)
            {
                resolver = (AssemblyResolver)node.CreateInstance();

                return;
            }

            resolver = null;
        }

        private static ResolveEventHandler GetResolveEventHandler(object classInstance)
        {
            var classType = classInstance.GetType();
            var eventField = classType.GetField(
                "_AssemblyResolve",
                BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

            var eventDelegate = (ResolveEventHandler)eventField.GetValue(classInstance);

            // eventDelegate will be null if no listeners are attached to the event
            if (eventDelegate == null)
            {
                return null;
            }

            return eventDelegate;
        }

        private static bool VerifyDelegateIsPresent(AssemblyResolver resolver)
        {
            var resolveEventHandler = GetResolveEventHandler(AppDomain.CurrentDomain);
            var del = Delegate.CreateDelegate(
                typeof(ResolveEventHandler),
                resolver,
                "AssemblyResolveHandler",
                false);
            Assert.IsNotNull(del);
            var delegateIsPresent = resolveEventHandler.GetInvocationList().Contains(del);
            return delegateIsPresent;
        }

        private AssemblyResolver CreateAssemblyResolver(bool initialize)
        {
            GetAssemblyResolver(out var resolver);
            Assert.IsNotNull(resolver);

            if (initialize)
            {
                resolver.Initialize();
            }

            return resolver;
        }
    }
}