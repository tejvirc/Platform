namespace Aristocrat.Monaco.G2S.Common.Tests.PackageManager.CommandHandlers
{
    using System;
    using Common.PackageManager.CommandHandlers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BasePackageCommandHandlerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullContextFactoryExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new BasePackageCommandHandler(
                null,
                helper.PackageErrorRepositoryMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPackageErrorRepositoryExpectException()
        {
            var helper = new HandlerTestHelper();

            var handler = new BasePackageCommandHandler(
                helper.ContextFactoryMock.Object,
                null);

            Assert.IsNull(handler);
        }
    }
}