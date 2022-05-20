namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using System.Data.Entity;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class UninstallModuleHandlerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullModuleRepositoryExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new UninstallModuleHandler(
                helper.ContextFactoryMock.Object,
                null,
                helper.PackageErrorRepositoryMock.Object,
                helper.InstallerService);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenExecuteExceptSuccess()
        {
            var helper = new HandlerTestHelper();

            helper.ConfigureAndRegisterMocks();

            var handler = new UninstallModuleHandler(
                helper.ContextFactoryMock.Object,
                helper.ModuleRepositoryMock.Object,
                helper.PackageErrorRepositoryMock.Object,
                helper.InstallerService);

            var args = new UninstallModuleArgs(new Module() { PackageId = "ATI_Test"}, arg => { });

            helper.ModuleRepositoryMock.Setup(a => a.GetModuleByModuleId(It.IsAny<DbContext>(), It.IsAny<string>())).Returns(args.ModuleEntity);

            handler.Execute(args);

            helper.FileSystemProviderMock
            .Verify(m => m.DeleteFile(It.IsAny<string>()));
            //helper.InstallerFactory.Verify(i => i.CreateNew(It.IsAny<string>()));
            helper.ModuleRepositoryMock
            .Verify(m => m.Delete(It.IsAny<DbContext>(), args.ModuleEntity));
        }
    }
}