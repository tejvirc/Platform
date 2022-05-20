namespace Aristocrat.Monaco.G2S.Common.Tests.GAT.CommandHandlers
{
    using System;
    using Common.GAT.CommandHandlers;
    using Common.GAT.Models;
    using Common.GAT.Storage;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Storage;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GetLogStatusHandlerTests
    {
        private readonly Mock<IMonacoContextFactory> monacoContextFactoryMock = new Mock<IMonacoContextFactory>();

        private readonly Mock<IGatVerificationRequestRepository> verificationRequestRepositoryMock =
            new Mock<IGatVerificationRequestRepository>();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfVerificationRequestRepositoryIsNull()
        {
            new GetLogStatusHandler(null, this.monacoContextFactoryMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowIfContextFactoryIsNull()
        {
            new GetLogStatusHandler(this.verificationRequestRepositoryMock.Object, null);
        }

        [TestMethod]
        public void ThrowIfGetLogStatusResultInvalid()
        {
            AssertHelper.Throws<ArgumentOutOfRangeException>(() => new GetLogStatusResult(-1, 0));

            AssertHelper.Throws<ArgumentOutOfRangeException>(() => new GetLogStatusResult(0, -1));
        }
    }
}