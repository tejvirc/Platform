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
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameRecoveryIsNullExpectException()
        {
            var handler = new AddRecoveryDataPointCommandHandler(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var history = new Mock<IGameHistory>();

            var handler = new AddRecoveryDataPointCommandHandler(history.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenHandleExpectSave()
        {
            byte[] data = { 0x01, 0x02 };

            var history = new Mock<IGameHistory>();

            var handler = new AddRecoveryDataPointCommandHandler(history.Object);

            handler.Handle(new AddRecoveryDataPoint(data));

            history.Verify(m => m.SaveRecoveryPoint(data));
        }
    }
}
