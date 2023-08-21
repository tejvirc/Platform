namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Gaming.Commands;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SimpleInjector;
    using Test.Common;

    [TestClass]
    public class RegisterPresentationCommandHandlerTests
    {
        private Mock<IOverlayMessageStrategyController> _overlayMessageStrategyController;

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialization()
        {
            _overlayMessageStrategyController = new Mock<IOverlayMessageStrategyController>(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void HandleTest()
        {
            var presentationData = new PresentationOverrideTypes[2];
            presentationData[0] = PresentationOverrideTypes.PrintingCashoutTicket;
            presentationData[1] = PresentationOverrideTypes.PrintingCashwinTicket;

            bool result = true;

            var command = new RegisterPresentation(result, presentationData);

            _overlayMessageStrategyController.Setup(r => r.RegisterPresentation(command.Result, command.TypeData)).Returns(Task.FromResult(true));

            var handler = Factory_CreateHandler();
            handler.Handle(command);

            _overlayMessageStrategyController.Verify(r => r.RegisterPresentation(command.Result, command.TypeData), Times.Once);
            Assert.AreEqual(command.Success, true);
        }

        private RegisterPresentationCommandHandler Factory_CreateHandler()
        {
            return new RegisterPresentationCommandHandler(_overlayMessageStrategyController.Object);
        }
    }
}
