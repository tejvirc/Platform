namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Application.Contracts.Extensions;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Sas.VoucherValidation;

    [TestClass]
    public class LP57SendPendingCashoutInformationHandlerTests
    {
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IHostValidationProvider> _hostValidationProvider;
        private LP57SendPendingCashoutInformationHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _hostValidationProvider = new Mock<IHostValidationProvider>(MockBehavior.Default);

            _target = new LP57SendPendingCashoutInformationHandler(_propertiesManager.Object, _hostValidationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new LP57SendPendingCashoutInformationHandler(null, _hostValidationProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullHostValidationProviderTest()
        {
            _target = new LP57SendPendingCashoutInformationHandler(_propertiesManager.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendPendingCashoutInformation));
        }

        [DataRow(
            SasValidationType.None,
            TicketType.None,
            false,
            false,
            0UL,
            CashoutTypeCode.NotWaitingForSystemValidation)]
        [DataRow(
            SasValidationType.SecureEnhanced,
            TicketType.None,
            false,
            false,
            0UL,
            CashoutTypeCode.NotWaitingForSystemValidation)]
        [DataRow(SasValidationType.System, TicketType.CashOut, true, true, 100UL, CashoutTypeCode.CashableTicket)]
        [DataRow(
            SasValidationType.System,
            TicketType.Restricted,
            true,
            true,
            100UL,
            CashoutTypeCode.RestrictedPromotionalTicket)]
        [DataRow(
            SasValidationType.System,
            TicketType.CashOut,
            false,
            true,
            0UL,
            CashoutTypeCode.NotWaitingForSystemValidation)]
        [DataTestMethod]
        public void HandleTest(
            SasValidationType validationType,
            TicketType ticketType,
            bool hasPendingValidationData,
            bool validCommand,
            ulong expectedAmount,
            CashoutTypeCode expectedCode)
        {
            var millicentsAmount = ((long)expectedAmount).CentsToMillicents();

            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { ValidationType = validationType });
            _hostValidationProvider.Setup(x => x.GetPendingValidationData())
                .Returns(hasPendingValidationData ? new HostValidationData((ulong)millicentsAmount, ticketType) : null);

            var result = _target.Handle(null);
            Assert.AreEqual(validCommand, result.ValidResponse);
            if (validCommand)
            {
                Assert.AreEqual(expectedAmount, result.Amount);
                Assert.AreEqual(expectedCode, result.TypeCode);
            }
        }
    }
}