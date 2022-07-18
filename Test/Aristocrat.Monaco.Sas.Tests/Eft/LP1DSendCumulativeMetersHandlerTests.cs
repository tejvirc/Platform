namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.EFT;
    using Contracts.Eft;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Eft;

    [TestClass]
    public class LP1DSendCumulativeMetersHandlerTests
    {
        private LP1DSendCumulativeMetersHandler _target;
        private Mock<IEftTransferProvider> _eftProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _eftProvider = new Mock<IEftTransferProvider>(MockBehavior.Strict);
            _target = new LP1DSendCumulativeMetersHandler(_eftProvider.Object);
            _eftProvider.Setup(x => x.QueryBalanceAmount()).Returns(new CumulativeEftMeterData());
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void InitializeWithNullArgumentExpectException()
        {
            var testNull = new LP1DSendCumulativeMetersHandler(null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EftSendCumulativeMeters));
        }

        [DataRow((ulong)0, (ulong)0, (ulong)0, (ulong)0, DisplayName = "All Credits Zero")]
        [DataRow((ulong)1000, (ulong)0, (ulong)0, (ulong)0, DisplayName = "Cashable 1000")]
        [DataRow((ulong)0, (ulong)1000, (ulong)0, (ulong)0, DisplayName = "NonCashableCredits 1000")]
        [DataRow((ulong)0, (ulong)0, (ulong)1000, (ulong)0, DisplayName = "PromotionalCredits 1000")]
        [DataRow((ulong)0, (ulong)0, (ulong)0, (ulong)1000, DisplayName = "TransferredCredits 1000")]
        [DataRow((ulong)1000, (ulong)1000, (ulong)1000, (ulong)1000, DisplayName = "All Credits 1000")]
        [DataRow((ulong)0, (ulong)1000, (ulong)1000, (ulong)0, DisplayName = "NonCashableCredits and PromotionalCredits 1000")]
        [DataRow((ulong)1000, (ulong)0, (ulong)1000, (ulong)0, DisplayName = "PromotionalCredits and Cashable 1000")]
        [DataRow((ulong)1000, (ulong)1000, (ulong)0, (ulong)0, DisplayName = "NonCashableCredits and Cashable 1000")]
        [DataRow((ulong)0, (ulong)0, (ulong)1000, (ulong)1000, DisplayName = "TransferredCredits and PromotionalCredits 1000")]
        [DataTestMethod]
        public void TestHandle(
            ulong cashableCredits,
            ulong nonCashableCredits,
            ulong promotionalCredits,
            ulong transferredCredits)
        {
            _eftProvider.Setup(x => x.QueryBalanceAmount()).Returns(
                new CumulativeEftMeterData
                {
                    PromotionalCredits = promotionalCredits,
                    NonCashableCredits = nonCashableCredits,
                    TransferredCredits = transferredCredits,
                    CashableCredits = cashableCredits
                });

            var handleResponseData = _target.Handle(null);

            var expectedResponseData =
                new CumulativeEftMeterData
                {
                    PromotionalCredits = promotionalCredits,
                    NonCashableCredits = nonCashableCredits,
                    TransferredCredits = transferredCredits,
                    CashableCredits = cashableCredits
                };
            _eftProvider.Verify(m => m.QueryBalanceAmount(), Times.Once);
            Assert.AreEqual(expectedResponseData.CashableCredits, handleResponseData.CashableCredits);
            Assert.AreEqual(expectedResponseData.PromotionalCredits, handleResponseData.PromotionalCredits);
            Assert.AreEqual(expectedResponseData.NonCashableCredits, handleResponseData.NonCashableCredits);
            Assert.AreEqual(expectedResponseData.TransferredCredits, handleResponseData.TransferredCredits);
        }
    }
}