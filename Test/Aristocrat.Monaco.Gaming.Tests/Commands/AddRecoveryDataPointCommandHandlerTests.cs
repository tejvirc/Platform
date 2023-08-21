namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AddRecoveryDataPointCommandHandlerTests
    {
        private readonly Mock<IGameHistory> _gameHistory = new();
        private readonly Mock<IGamePlayState> _gamePlayState = new();

        private AddRecoveryDataPointCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataTestMethod]
        [DataRow(true, false)]
        [DataRow(false, true)]
        public void WhenGameRecoveryIsNullExpectException(bool nullHistory, bool nullGamePlayState)
        {
            Assert.ThrowsException<ArgumentNullException>(() => _ = CreateTarget(nullHistory, nullGamePlayState));
        }

        [DataTestMethod]
        [DataRow(false, false, true)]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        public void WhenHandleExpectSave(bool isIdle, bool isPresentationIdle, bool recoverySaved)
        {
            byte[] data = { 0x01, 0x02 };

            _gamePlayState.Setup(x => x.Idle).Returns(isIdle);
            _gamePlayState.Setup(x => x.InPresentationIdle).Returns(isPresentationIdle);
            _target.Handle(new AddRecoveryDataPoint(data));
            _gameHistory.Verify(m => m.SaveRecoveryPoint(data), recoverySaved ? Times.Once() : Times.Never());
        }

        private AddRecoveryDataPointCommandHandler CreateTarget(
            bool nullHistory = false,
            bool nullGamePlayState = false)
        {
            return new AddRecoveryDataPointCommandHandler(
                nullHistory ? null : _gameHistory.Object,
                nullGamePlayState ? null : _gamePlayState.Object);
        }
    }
}
