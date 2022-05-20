namespace Aristocrat.Monaco.Application.UI.Tests
{
    using Aristocrat.Monaco.Application.UI.Loaders;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System.IO;

    [TestClass]
    public class LoadersTest
    {
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            AddinManager.Registry.Update();

            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);

            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Strict);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            AddinManager.Shutdown();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void BeagleBoneLoaderTest()
        {
            // set up mocks for constructor
            _propertiesManager.Setup(m => m.GetProperty("BeagleBoneEnabled", false)).Returns(true);
            const string expected = "Beagle Bone";

            var target = new BeagleBonePageLoader();

            Assert.IsNotNull(target);
            Assert.AreEqual(expected, target.PageName);
        }
    }
}