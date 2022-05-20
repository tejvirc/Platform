namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Gaming.Commands;
    using Hardware.Contracts.ButtonDeck;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class UpdateButtonDeckImageCommandHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenButtonDeckDisplayIsNullExpectException()
        {
            var handler = new UpdateButtonDeckImageCommandHandler(null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var display = new Mock<IButtonDeckDisplay>();
            var systemDisabled = new Mock<ISystemDisableManager>();
            var replay = new Mock<IGameDiagnostics>();
            var handler = new UpdateButtonDeckImageCommandHandler(display.Object, systemDisabled.Object, replay.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenHandleExpectDraw()
        {
            var display = new Mock<IButtonDeckDisplay>();
            var systemDisabled = new Mock<ISystemDisableManager>();
            var replay = new Mock<IGameDiagnostics>();
            var handler = new UpdateButtonDeckImageCommandHandler(display.Object, systemDisabled.Object, replay.Object);

            handler.Handle(new UpdateButtonDeckImage());

            display.Verify(m => m.DrawFromSharedMemory());
        }

        [TestMethod]
        public void WhenSystemDisabledExpectNoDraw()
        {
            var display = new Mock<IButtonDeckDisplay>();
            var systemDisabled = new Mock<ISystemDisableManager>();
            var replay = new Mock<IGameDiagnostics>();
            systemDisabled.Setup(m => m.IsDisabled).Returns(true);

            var handler = new UpdateButtonDeckImageCommandHandler(display.Object, systemDisabled.Object, replay.Object);

            handler.Handle(new UpdateButtonDeckImage());

            display.Verify(m => m.DrawFromSharedMemory(), Times.Never);
        }
    }
}
