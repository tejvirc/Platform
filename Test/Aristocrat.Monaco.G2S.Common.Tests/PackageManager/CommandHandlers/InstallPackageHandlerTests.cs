namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class InstallPackageHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageRepositoryExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new InstallPackageHandler(
                helper.ContextFactoryMock.Object,
                null,
                helper.PackageErrorRepositoryMock.Object,
                helper.InstallerService);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPackageEntityParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var installHandler = GetInstallPackageHandler(helper);

            installHandler.Execute(new InstallPackageArgs(null, null, false));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCallbackParameterTest()
        {
            var helper = new HandlerTestHelper();
            helper.ConfigureAndRegisterMocks();

            var installHandler = GetInstallPackageHandler(helper);

            installHandler.Execute(new InstallPackageArgs(new Package(), null, false));
        }

        private InstallPackageHandler GetInstallPackageHandler(HandlerTestHelper testHelper)
        {
            var packageRepository = testHelper.PackageRepositoryMock.Object;
            var packageErrorRepository = testHelper.PackageErrorRepositoryMock.Object;

            return new InstallPackageHandler(
                testHelper.ContextFactoryMock.Object,
                packageRepository,
                packageErrorRepository,
                testHelper.InstallerService);
        }
    }
}