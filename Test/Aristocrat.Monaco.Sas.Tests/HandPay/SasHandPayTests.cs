namespace Aristocrat.Monaco.Sas.Tests.HandPay
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.HandPay;

    [TestClass]
    public class SasHandPayTests
    {
        private const int WaitTime = 1000;

        private SasHandPay _target;
        private Mock<IPropertiesManager> _properties;
        private Mock<ISasHost> _sasHost;
        private Mock<ISasHandPayCommittedHandler> _sasHandpayCommittedHandler;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _sasHost = new Mock<ISasHost>(MockBehavior.Default);
            _sasHandpayCommittedHandler = new Mock<ISasHandPayCommittedHandler>(MockBehavior.Default);
            _target = new SasHandPay(_properties.Object, _sasHost.Object, _sasHandpayCommittedHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesManagerTest()
        {
            _target = new SasHandPay(null, _sasHost.Object, _sasHandpayCommittedHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSasHostTest()
        {
            _target = new SasHandPay(_properties.Object, null, _sasHandpayCommittedHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSasHandpayCommittedHandlerTest()
        {
            _target = new SasHandPay(_properties.Object, _sasHost.Object, null);
        }

        [TestMethod]
        public void ServiceTest()
        {
            var serviceTypes = _target.ServiceTypes;
            Assert.AreEqual(1, serviceTypes.Count);
            Assert.IsTrue(serviceTypes.Contains(typeof(IHandpayValidator)));
            Assert.AreEqual(typeof(SasHandPay).ToString(), _target.Name);
        }

        [TestMethod]
        public void ValidateHandpay()
        {
            const long cashableAmount = 1000;
            const long promoAmount = 2000;
            const long nonCashAmount = 0;

            Assert.IsTrue(_target.ValidateHandpay(cashableAmount, promoAmount, nonCashAmount, HandpayType.CancelCredit));
        }
        
        [DataRow(
            TicketValidationType.HandPayFromCashOutNoReceipt,
            HandpayType.CancelCredit,
            "5781364",
            DisplayName = "Handpay from cashout")]
        [DataRow(
            TicketValidationType.HandPayFromWinReceiptPrinted,
            HandpayType.GameWin,
            "874220156",
            DisplayName = "Handpay from win")]
        [DataTestMethod]
        public void RequestHandpayTest(
            TicketValidationType ticketValidationType,
            HandpayType handPayType,
            string barcode)
        {
            const long cashoutAmount = 1000;
            const long promoAmount = 2000;
            var handpayTransaction = new HandpayTransaction(
                0,
                DateTime.Now,
                cashoutAmount,
                promoAmount,
                0,
                handPayType,
                false,
                Guid.NewGuid());

            _sasHandpayCommittedHandler.Setup(x => x.HandpayPending(It.IsAny<HandpayTransaction>()))
                .Returns(Task.CompletedTask);
            _sasHost.Setup(x => x.ValidateHandpayRequest(cashoutAmount + promoAmount, It.IsAny<HandPayType>()))
                .Returns(
                    Task.FromResult(
                        new TicketOutInfo
                        {
                            Barcode = barcode,
                            ValidationType = ticketValidationType
                        }));

            var requestHandpay = _target.RequestHandpay(handpayTransaction);

            Assert.IsTrue(requestHandpay.Wait(WaitTime));
            Assert.AreEqual(barcode, handpayTransaction.Barcode);
        }
    }
}