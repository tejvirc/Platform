namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using Aristocrat.Monaco.Sas.Eft;
    using Aristocrat.Monaco.Sas.Contracts.Eft;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Contains unit tests for the LP63TransferPromoCreditHandler class.
    /// </summary>
    [TestClass]
    public class LP27SendCurrentPromotionalCreditsHandlerTests
    {
        private Mock<IEftTransferProvider> _eftTransferProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MockLocalization.Setup(MockBehavior.Strict);
            _eftTransferProvider = new Mock<IEftTransferProvider>(MockBehavior.Strict);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfNoInjectionTest()
        {
            new LP27SendCurrentPromotionalCreditsHandler(null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            var lp27Handler = new LP27SendCurrentPromotionalCreditsHandler(_eftTransferProvider.Object);
            Assert.AreEqual(1, lp27Handler.Commands.Count);
            Assert.IsTrue(lp27Handler.Commands.Contains(LongPoll.EftSendCurrentPromotionalCredits));
        }

        [DataRow(0, DisplayName = "0 promotional credits")]
        [DataRow(100, DisplayName = "100 promotional credits")]
        [DataRow(100000, DisplayName = "100000 promotional  promo credits")]
        [DataTestMethod]
        public void ShouldReturnValidPromoCredits(long promoCredits)
        {
            _eftTransferProvider.Setup(l => l.GetCurrentPromotionalCredits()).Returns(promoCredits);
            var lp27Handler = new LP27SendCurrentPromotionalCreditsHandler(_eftTransferProvider.Object);
            var handlerResponse = lp27Handler.Handle(new LongPollData());
            Assert.IsNotNull(handlerResponse);
            Assert.AreEqual((ulong)promoCredits, handlerResponse.CurrentPromotionalCredits);
            _eftTransferProvider.Verify(m => m.GetCurrentPromotionalCredits(), Times.Once);
        }
    }
}