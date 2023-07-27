namespace Aristocrat.Monaco.Sas.Tests
{
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class SasHostTests
    {
        private SasHost _target;

        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>();
        private readonly Mock<IReadOnlyCollection<ISasLongPollHandler>> _longPollHandlers = new Mock<IReadOnlyCollection<ISasLongPollHandler>>();
        private readonly Mock<IValidationHandler> _validationHandler = new Mock<IValidationHandler>();
        private readonly Mock<ISasTicketPrintedHandler> _ticketPrintedHandler = new Mock<ISasTicketPrintedHandler>();
        private readonly Mock<IAftRegistrationProvider> _aftRegistrationProvider = new Mock<IAftRegistrationProvider>();
        private readonly Mock<ISasHandPayCommittedHandler> _sasHandPayCommittedHandler = new Mock<ISasHandPayCommittedHandler>();
        private readonly Mock<IAftTransferProvider> _aftTransferProvider = new Mock<IAftTransferProvider>();
        private readonly Mock<ISasVoucherInProvider> _sasVoucherInProvider = new Mock<ISasVoucherInProvider>();

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Disable parallel test execution at the assembly level.
            context.Properties["microsoft.testfx.testrun.isparallel"] = false;
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new SasHost();
            _target.RegisterHandlers(
                _exceptionHandler.Object,
                _longPollHandlers.Object,
                _validationHandler.Object,
                _ticketPrintedHandler.Object,
                _aftRegistrationProvider.Object,
                _sasHandPayCommittedHandler.Object,
                _aftTransferProvider.Object,
                _sasVoucherInProvider.Object);
        }

        [TestMethod]
        public void AftTransferFailedTest()
        {
            var aftData = new AftData();
            var errorCode = AftTransferStatusCode.GamingMachineUnableToPerformTransfer;

            _target.AftTransferFailed(aftData, errorCode);
            _aftTransferProvider.Verify(x => x.UpdateFinalAftResponseData(aftData, errorCode, true), Times.Once);
        }
    }
}
