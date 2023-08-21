namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PackageManifest.Models;

    [TestClass]
    public class UninstallPackageHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullModuleRepositoryExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new UninstallPackageHandler(
                helper.ContextFactoryMock.Object,
                null,
                helper.PackageErrorRepositoryMock.Object,
                helper.InstallerService);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void TestUninstallPackageSteps()
        {
            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();

            helper.FileSystemProviderMock
                .Setup(m => m.SearchFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new[] { "search_file" });

            var manifest = new Image(
                "Test",
                "game",
                string.Empty,
                0,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                new string[] { });

            helper.ManifestServiceMock.Setup(m => m.Read(It.IsAny<string>()))
                .Returns(manifest);

            var uninstallHandler = GetUninstallPackageHandler(helper);

            uninstallHandler.Execute(new InstallPackageArgs(new Package { PackageId = "Test" }, args => { }, false));

            helper.ManifestServiceMock.Verify(x => x.Read(It.IsAny<string>()), Times.Once);
            //helper.FileSystemProviderMock.Verify(x => x.DeleteFolder(It.IsAny<string>()), Times.Once);
            //helper.ModuleRepositoryMock.Verify(m => m.GetModuleByModuleId(It.IsAny<DbContext>(), It.IsAny<string>()));
            //helper.ModuleRepositoryMock.Verify(m => m.Delete(It.IsAny<DbContext>(), It.IsAny<Module>()));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageEntityParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var uninstallHandler = GetUninstallPackageHandler(helper);

            uninstallHandler.Execute(new InstallPackageArgs(null, null, false));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCallbackParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var uninstallHandler = GetUninstallPackageHandler(helper);

            uninstallHandler.Execute(new InstallPackageArgs(new Package(), null, false));
        }

        private static UninstallPackageHandler GetUninstallPackageHandler(HandlerTestHelper testHelper)
        {
            var packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;

            return new UninstallPackageHandler(
                testHelper.ContextFactoryMock.Object,
                testHelper.ModuleRepositoryMock.Object,
                packageErrorRepository,
                testHelper.InstallerService);
        }
    }
}
