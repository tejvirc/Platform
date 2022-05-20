namespace Aristocrat.Monaco.Kernel.Tests.MonoAddinsHelpers
{
    #region Using

    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    #endregion

    /// <summary>
    ///     Summary description for AddinHelperTest
    /// </summary>
    [TestClass]
    public class AddinHelperTest
    {
        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void NameTest()
        {
            var target = new AddinHelper();

            Assert.AreEqual("AddinHelper", target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var target = new AddinHelper();

            Assert.AreEqual(1, target.ServiceTypes.Count);
            Assert.AreEqual(typeof(IAddinHelper), target.ServiceTypes.ToArray()[0]);
        }

        [TestMethod]
        public void InitializeTest()
        {
            var target = new AddinHelper();

            Assert.IsNotNull(target);
            target.Initialize();
        }

        [TestMethod]
        public void GetSelectedNodesTest()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            var propertiesManager =
                MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict, true);
            propertiesManager.Setup(mock => mock.GetProperty("Mono.SelectedAddinConfigurationHashCode", null))
                .Returns(null);

            var target = new AddinHelper();
            var result = target.GetSelectedNodes<ExtensionNode>("/Kernel/EventBus");
            Assert.AreEqual(1, result.Count);

            if (AddinManager.IsInitialized)
            {
                AddinManager.Shutdown();
            }

            MoqServiceManager.RemoveInstance();
        }
    }
}