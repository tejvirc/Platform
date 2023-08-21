namespace Aristocrat.Monaco.Bingo.Tests.Handlers
{
    using System;
    using System.Threading;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Handlers;
    using Aristocrat.Monaco.Test.Common;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DisableResponseHandlerTests
    {
        private DisableResponseHandler _target;
        private readonly Mock<IBingoDisableProvider> _disableProvider = new Mock<IBingoDisableProvider>(MockBehavior.Default);

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);

            _target = CreateTarget();
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true)]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Fail(bool nullDisable)
        {
            _target = CreateTarget(nullDisable);
        }

        [DataRow(null, false, DisplayName = "Null string")]
        [DataRow("", false, DisplayName = "Empty string")]
        [DataRow("Sample Text", false, DisplayName = "Non-empty string")]
        [DataRow("        Sample Text        ", false, DisplayName = "Padded string")]
        [DataTestMethod]
        public void Handle(string reason, bool forceCashout)
        {
            var formattedReason = string.Empty;
            _disableProvider.Setup(x => x.Disable(It.IsAny<string>()))
                .Callback<string>(x => formattedReason = x)
                .Verifiable();

            var result = _target.Handle(new Disable(reason, forceCashout), new CancellationToken());
            var parsedReason = formattedReason.Split('-');

            Assert.IsTrue(parsedReason.Length >= 1);
            Assert.AreEqual(ResponseCode.Ok, result.Result.ResponseCode);
            _disableProvider.Verify();
        }

        private DisableResponseHandler CreateTarget(bool nullDisable = false)
        {
            return new DisableResponseHandler(nullDisable ? null : _disableProvider.Object);
        }
    }
}
