namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Contracts.SASProperties;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;

    [TestClass]
    public class CurrencyInCompletedConsumerTest
    {
        private CurrencyInCompletedConsumer _target;
        private Mock<ISasExceptionHandler> _exceptionHandler;
        private Mock<ISasNoteAcceptorProvider> _noteAcceptorProvider;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IMeterManager> _meterManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);
            _noteAcceptorProvider = new Mock<ISasNoteAcceptorProvider>(MockBehavior.Strict);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
            _meterManager = new Mock<IMeterManager>(MockBehavior.Default);

            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _target = new CurrencyInCompletedConsumer(_noteAcceptorProvider.Object, _exceptionHandler.Object, _meterManager.Object, _propertiesManager.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullNoteAcceptorProvider()
        {
            _target = new CurrencyInCompletedConsumer(null, _exceptionHandler.Object, _meterManager.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NulExceptionHandler()
        {
            _target = new CurrencyInCompletedConsumer(_noteAcceptorProvider.Object, null, _meterManager.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullMeterManager()
        {
            _target = new CurrencyInCompletedConsumer(_noteAcceptorProvider.Object, _exceptionHandler.Object, null, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManager()
        {
            _target = new CurrencyInCompletedConsumer(_noteAcceptorProvider.Object, _exceptionHandler.Object, _meterManager.Object, null);
        }

        [DataTestMethod]
        [DataRow(false, DisplayName = "Test when not disabling note acceptor after currency stacked")]
        [DataRow(true, DisplayName = "Test when disabling note acceptor after currency stacked")]
        public void ConsumeTest(bool disableNoteAcceptor)
        {
            BillDataExceptionBuilder actual = null;
            var expectedBillData = new BillData { AmountInCents = 10_00, CountryCode = BillAcceptorCountryCode.UnitedStates, LifetimeCount = 1234 };
            var expectedResults = new BillDataExceptionBuilder(expectedBillData);

            var meterBill = new Mock<IMeter>();
            meterBill.Setup(m => m.Lifetime).Returns(1234);
            _meterManager.Setup(m => m.IsMeterProvided(AccountingMeters.BillCount10s)).Returns(true);
            _meterManager.Setup(m => m.GetMeter(AccountingMeters.BillCount10s)).Returns(meterBill.Object);

            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<BillDataExceptionBuilder>()))
                .Callback((ISasExceptionCollection b) => actual = b as BillDataExceptionBuilder)
                .Verifiable();

            _noteAcceptorProvider.Setup(m => m.BillDisableAfterAccept).Returns(disableNoteAcceptor);
            _noteAcceptorProvider.Setup(m => m.ConfigureBillDenominations(It.IsAny<IEnumerable<ulong>>())).Returns(true);

            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.CurrencyMultiplierKey, 1d)).Returns(100_000d);

            var @event = new CurrencyInCompletedEvent(10,
                new Note
                {
                    Value = 10,
                    ISOCurrencySymbol = "USD"
                });

            _target.Consume(@event);

            Assert.IsNotNull(actual);
            CollectionAssert.AreEquivalent(expectedResults, actual);
            _exceptionHandler.Verify();
        }

        [TestMethod]
        public void NullNoteTest()
        {
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<BillDataExceptionBuilder>())).Verifiable();

            var @event = new CurrencyInCompletedEvent(10);

            _target.Consume(@event);

            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<BillDataExceptionBuilder>()), Times.Never);
        }

        [TestMethod]
        public void ZeroAmountTest()
        {
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<BillDataExceptionBuilder>())).Verifiable();

            var @event = new CurrencyInCompletedEvent(0,
                new Note
                {
                    Value = 0,
                    ISOCurrencySymbol = "USD"
                });

            _target.Consume(@event);

            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<BillDataExceptionBuilder>()), Times.Never);
        }
    }
}