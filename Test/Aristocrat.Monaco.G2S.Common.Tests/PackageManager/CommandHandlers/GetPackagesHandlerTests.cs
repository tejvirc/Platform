namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using System.Data.Entity;
    using Common.PackageManager.CommandHandlers;
    using Common.PackageManager.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class GetPackagesHandlerTests
    {
        private readonly Mock<IMonacoContextFactory> monacoContextFactory = new Mock<IMonacoContextFactory>();
        private readonly Mock<IPackageRepository> packageRepositoryMock = new Mock<IPackageRepository>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfPackageRepositoryIsNull()
        {
            new GetPackagesHandler(null, this.monacoContextFactory.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfContextFactoryIsNull()
        {
            new GetPackagesHandler(this.packageRepositoryMock.Object, null);
        }

        [TestMethod]
        public void GetPackagesTest()
        {
            this.packageRepositoryMock.Setup(x => x.GetAll(It.IsAny<DbContext>()));

            var handler = new GetPackagesHandler(
                this.packageRepositoryMock.Object,
                this.monacoContextFactory.Object);
            handler.Execute();

            this.packageRepositoryMock.Verify(x => x.GetAll(It.IsAny<DbContext>()), Times.Once);
        }
    }
}