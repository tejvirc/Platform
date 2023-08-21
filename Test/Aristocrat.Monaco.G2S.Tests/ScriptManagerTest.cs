namespace Aristocrat.Monaco.G2S.Tests
{
    using System;
    using Aristocrat.G2S.Client;
    using Common.PackageManager;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ScriptManagerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackagManagerExpectException()
        {
            var scriptManager = new ScriptManager(null, null, null, null, null, null, null, null, null, null);

            Assert.IsNull(scriptManager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var packageManager = new Mock<IPackageManager>();

            var scriptManager = new ScriptManager(
                packageManager.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(scriptManager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGatServiceExpectException()
        {
            var packageManager = new Mock<IPackageManager>();
            var egm = new Mock<IG2SEgm>();
            var scriptManager = new ScriptManager(
                packageManager.Object,
                egm.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(scriptManager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var packageManager = new Mock<IPackageManager>();
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var scriptManager = new ScriptManager(
                packageManager.Object,
                egm.Object,
                gatService.Object,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(scriptManager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmStateManagerExpectException()
        {
            var packageManager = new Mock<IPackageManager>();
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var eventBus = new Mock<IEventBus>();
            var scriptManager = new ScriptManager(
                packageManager.Object,
                egm.Object,
                gatService.Object,
                eventBus.Object,
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(scriptManager);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCabinetServiceExpectException()
        {
            var packageManager = new Mock<IPackageManager>();
            var egm = new Mock<IG2SEgm>();
            var gatService = new Mock<IGatService>();
            var eventBus = new Mock<IEventBus>();
            var egmState = new Mock<IEgmStateManager>();
            var scriptManager = new ScriptManager(
                packageManager.Object,
                egm.Object,
                gatService.Object,
                eventBus.Object,
                egmState.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(scriptManager);
        }
    }
}