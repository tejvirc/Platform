namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Sas.VoucherValidation;

    [TestClass]
    public class LP58ReceiveValidationNumberHandlerTests
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IHostValidationProvider> _hostValidationProvider;
        private LP58ReceiveValidationNumberHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _hostValidationProvider = new Mock<IHostValidationProvider>(MockBehavior.Default);

            _target = new LP58ReceiveValidationNumberHandler(_propertiesManager.Object, _hostValidationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LP58ReceiveValidationNumberHandler(null, _hostValidationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullHostValidationProviderTest()
        {
            _target = new LP58ReceiveValidationNumberHandler(_propertiesManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.ReceiveValidationNumber));
        }

        [DataRow(
            SasValidationType.None,
            (byte)0x01,
            100UL,
            ValidationState.NoValidationPending,
            false,
            false,
            false,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            DisplayName = "None validation will fail handling for LP58")]
        [DataRow(
            SasValidationType.SecureEnhanced,
            (byte)0x01,
            100UL,
            ValidationState.NoValidationPending,
            false,
            false,
            false,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            DisplayName = "Secure Enhanced validation will fail handling for LP58")]
        [DataRow(
            SasValidationType.System,
            (byte)0x00,
            100UL,
            ValidationState.NoValidationPending,
            true,
            false,
            true,
            ReceiveValidationNumberStatus.NotInCashout,
            DisplayName = "No Pending Validations returns not in cashout")]
        [DataRow(
            SasValidationType.System,
            (byte)0x00,
            100UL,
            ValidationState.CashoutInformationPending,
            true,
            true,
            true,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            DisplayName = "Cashout pending and denying the ticket will return Command Acknowledged")]
        [DataRow(
            SasValidationType.System,
            (byte)0x01,
            100UL,
            ValidationState.CashoutInformationPending,
            true,
            true,
            true,
            ReceiveValidationNumberStatus.ImproperValidationRejected,
            DisplayName = "Cashout pending and providing validation data will return Improper Validation Rejected")]
        [DataRow(
            SasValidationType.System,
            (byte)0x00,
            100UL,
            ValidationState.ValidationNumberPending,
            true,
            true,
            true,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            DisplayName = "Validation Number Pending and denying the ticket will return Command Acknowledged")]
        [DataRow(
            SasValidationType.System,
            (byte)0x01,
            100UL,
            ValidationState.ValidationNumberPending,
            false,
            true,
            true,
            ReceiveValidationNumberStatus.CommandAcknowledged,
            DisplayName = "Validation Number Pending and providing validation data will return Command Acknowledged")]
        [DataTestMethod]
        public void HandleTest(
            SasValidationType validationType,
            byte validationId,
            ulong validationNumber,
            ValidationState currentState,
            bool rejectTicket,
            bool validationResults,
            bool validResponse,
            ReceiveValidationNumberStatus expectedStatus)
        {
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = validationType });
            if (validResponse)
            {
                _hostValidationProvider.Setup(x => x.CurrentState).Returns(currentState);
                if (rejectTicket)
                {
                    _hostValidationProvider.Setup(x => x.SetHostValidationResult(null))
                        .Returns(validationResults);
                }
                else
                {
                    _hostValidationProvider.Setup(
                            x => x.SetHostValidationResult(
                                It.Is<HostValidationResults>(
                                    res => string.Equals(res.ValidationNumber, $"{validationNumber:D16}") &&
                                           res.SystemId == validationId)))
                        .Returns(validationResults);
                }
            }

            var result = _target.Handle(
                new ReceiveValidationNumberData
                {
                    ValidationSystemId = validationId,
                    ValidationNumber = validationNumber
                });

            Assert.AreEqual(validResponse, result.ValidResponse);
            if (validResponse)
            {
                Assert.AreEqual(expectedStatus, result.Status);
            }
        }
    }
}