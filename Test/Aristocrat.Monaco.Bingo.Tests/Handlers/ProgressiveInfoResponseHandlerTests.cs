namespace Aristocrat.Monaco.Bingo.Handlers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.Progressives;

    [TestClass]
    public class ProgressiveInfoResponseHandlerTests
    {
        private readonly Mock<IProgressiveInfoHandler> _infoHandler = new(MockBehavior.Default);
        private ProgressiveInfoResponseHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new ProgressiveInfoResponseHandler(_infoHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullOutComeHandler()
        {
            _target = new ProgressiveInfoResponseHandler(null);
        }

        [DataRow(false, ResponseCode.Rejected)]
        [DataRow(true, ResponseCode.Ok)]
        [DataTestMethod]
        public async Task HandleTest(bool accepted, ResponseCode expectedCode)
        {
            var gameTitleId = 101;
            var denomination = 25L;
            var authToken = "ABC123";
            var progressiveLevels = new List<ProgressiveLevelInfo>();
            progressiveLevels.Add(new ProgressiveLevelInfo(101L, 1, gameTitleId, denomination));
            progressiveLevels.Add(new ProgressiveLevelInfo(102L, 2, gameTitleId, denomination));
            var metersToReport = new List<int>();
            metersToReport.Add(200);
            metersToReport.Add(201);

            var info = new ProgressiveInfoMessage(
                ResponseCode.Ok,
                accepted,
                gameTitleId,
                authToken,
                progressiveLevels,
                metersToReport);

            _infoHandler.Setup(x => x.ProcessProgressiveInfo(info, It.IsAny<CancellationToken>())).Returns(Task.FromResult(accepted));
            var result = await _target.Handle(info, CancellationToken.None);
            Assert.AreEqual(expectedCode, result.ResponseCode);
        }
    }
}