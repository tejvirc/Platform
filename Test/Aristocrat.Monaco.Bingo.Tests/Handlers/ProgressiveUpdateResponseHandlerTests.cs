namespace Aristocrat.Monaco.Bingo.Handlers.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.Progressives;

    [TestClass]
    public class ProgressiveUpdateResponseHandlerTests
    {
        private readonly Mock<IProgressiveUpdateHandler> _updateHandler = new(MockBehavior.Default);
        private ProgressiveUpdateResponseHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ProgressiveUpdateResponseHandler(_updateHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullOutComeHandler()
        {
            _target = new ProgressiveUpdateResponseHandler(null);
        }

        [DataRow(false, ResponseCode.Rejected)]
        [DataRow(true, ResponseCode.Ok)]
        [DataTestMethod]
        public async Task HandleTest(bool accepted, ResponseCode expectedCode)
        {
            var progressiveLevel = 1L;
            var amount = 10001L;
            var update = new ProgressiveUpdateMessage(
                progressiveLevel,
                amount);

            _updateHandler.Setup(x => x.ProcessProgressiveUpdate(update, It.IsAny<CancellationToken>())).Returns(Task.FromResult(accepted));
            var result = await _target.Handle(update, CancellationToken.None);
            Assert.AreEqual(expectedCode, result.ResponseCode);
        }
    }
}