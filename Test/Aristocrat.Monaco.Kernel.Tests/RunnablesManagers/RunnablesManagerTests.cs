namespace Aristocrat.Monaco.Kernel.Tests.RunnablesManagers
{
    using System;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for RunnablesManager and is intended
    ///     to contain all RunnablesManager Unit Tests
    /// </summary>
    [TestClass]
    public class RunnablesManagerTests : BaseTestClass
    {
        /// <summary>
        ///     Initialize for each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            Debug("{0}() initialize-start", TestContext.TestName);

            AddinManager.Initialize(Directory.GetCurrentDirectory());

            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            var propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
            propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", null))
                .Returns(null);

            Debug("{0}() initialize-end", TestContext.TestName);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            Debug("{0}() cleanup-start", TestContext.TestName);

            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
            Debug("{0}() cleanup-end", TestContext.TestName);
        }

        /// <summary>
        ///     A test for StartRunnables() where no runnables exist at the extension point
        /// </summary>
        [TestMethod]
        public void NoRunnablesTest()
        {
            var ExtensionPath = "UnknownExtensionPath";

            var target = new RunnablesManager();
            var accessor = new RunnablesManagerPrivateObject(target);

            var nodes = AddinManager.GetExtensionNodes<TypeExtensionNode>(ExtensionPath);
            Assert.AreEqual(0, nodes.Count);

            target.StartRunnables(ExtensionPath);
            Assert.AreEqual(0, accessor.Runnables.Count);

            target.StopRunnables();
            Assert.AreEqual(0, accessor.Runnables.Count);
        }
    }
}