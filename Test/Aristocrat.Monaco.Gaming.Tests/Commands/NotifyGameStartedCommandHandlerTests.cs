namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Gaming.Commands;
    using Gaming.Runtime;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class NotifyGameStartedCommandHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRuntimeServiceIsNullExpectException()
        {
            var handler = new NotifyGameStartedCommandHandler(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenRuntimeIsNullExpectException()
        {
            var runtime = new Mock<IRuntime>();

            var handler = new NotifyGameStartedCommandHandler(runtime.Object, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenGameRecoveryIsNullExpectException()
        {
            var runtime = new Mock<IRuntime>();
            var bank = new Mock<IPlayerBank>();

            var handler = new NotifyGameStartedCommandHandler(runtime.Object, bank.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var runtime = new Mock<IRuntime>();
            var bank = new Mock<IPlayerBank>();
            var recovery = new Mock<IGameRecovery>();

            var handler = new NotifyGameStartedCommandHandler(runtime.Object, bank.Object, recovery.Object);

            Assert.IsNotNull(handler);
        }
    }
}
