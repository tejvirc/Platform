namespace Aristocrat.Monaco.Gaming.UI.Tests
{
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Gaming.Runtime;
    using Aristocrat.Monaco.Gaming.UI.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System.Collections.Generic;

    [TestClass]
    public class GameControlledOverlayMessageStrategyTests
    {
        private Mock<IOverlayMessageStrategyController> _overlayMessageStrategyController;
        private Mock<IPresentationService> _presentationService;

        [TestInitialize]
        public void TestInitialization()
        {
            _overlayMessageStrategyController = new Mock<IOverlayMessageStrategyController>(MockBehavior.Default);
            _presentationService = new Mock<IPresentationService>(MockBehavior.Default);
        }

        [DataRow(true, LobbyCashOutState.Voucher, PresentationOverrideTypes.PrintingCashwinTicket)]
        [DataRow(false, LobbyCashOutState.Voucher, PresentationOverrideTypes.PrintingCashoutTicket)]
        [DataRow(false, LobbyCashOutState.Wat, PresentationOverrideTypes.TransferingOutCredits)]
        [DataTestMethod]
        public void HandleMessageOverlayCashOutGameAndPresentationsRegisteredTest(
            bool lastCashOutForcedByMaxBank,
            LobbyCashOutState cashOutState,
            PresentationOverrideTypes expectedPresentation)
        {
            var registeredPresentations = new List<PresentationOverrideTypes>()
            {
                PresentationOverrideTypes.PrintingCashwinTicket,
                PresentationOverrideTypes.PrintingCashoutTicket,
                PresentationOverrideTypes.TransferingOutCredits
            };

            var fallbackStrategy = new Mock<IOverlayMessageStrategy>(MockBehavior.Default);
            _overlayMessageStrategyController.Setup(x => x.FallBackStrategy).Returns(fallbackStrategy.Object);
            _overlayMessageStrategyController.Setup(x => x.GameRegistered).Returns(true);
            _overlayMessageStrategyController.Setup(x => x.RegisteredPresentations).Returns(registeredPresentations);

            var expectedMessageData = new MessageOverlayData()
            {
                Text = "Text",
                SubText = "SubText",
                SubText2 = "SubText2"
            };

            var overlayMessageData = new MessageOverlayData();
            fallbackStrategy.Setup(x => x.HandleMessageOverlayCashOut(It.IsAny<MessageOverlayData>(), lastCashOutForcedByMaxBank, cashOutState)).Returns(expectedMessageData);

            var expectedOverride = new PresentationOverrideData("Text\nSubText\nSubText2", expectedPresentation);

            var expectedOverriddenPresentations = new List<PresentationOverrideData>()
            {
                expectedOverride
            };

            var actualOverriddenPresentations = new List<PresentationOverrideData>();

            _presentationService.Setup(x => x.PresentOverriddenPresentation(It.IsAny<IList<PresentationOverrideData>>()))
                .Callback<IList<PresentationOverrideData>>(y => actualOverriddenPresentations = (List<PresentationOverrideData>)y);

            var gameOverlayStrategy = new GameControlledOverlayMessageStrategy(_overlayMessageStrategyController.Object, _presentationService.Object);
            gameOverlayStrategy.LastCashOutAmount = 1000; 
            gameOverlayStrategy.HandleMessageOverlayCashOut(overlayMessageData, lastCashOutForcedByMaxBank, cashOutState);

            Assert.AreEqual(expectedOverriddenPresentations[0].Message, actualOverriddenPresentations[0].Message);
            Assert.AreEqual(expectedOverriddenPresentations[0].Type, actualOverriddenPresentations[0].Type);
        }

        [TestMethod]
        public void HandleMessageOverlayCashOutWithZeroLastCashoutAmountTest()
        {
            var lastCashOutForcedByMaxBank = false;
            var cashOutState = LobbyCashOutState.Voucher;
            var expectedPresentation = PresentationOverrideTypes.PrintingCashoutTicket;

            var registeredPresentations = new List<PresentationOverrideTypes>()
            {
                PresentationOverrideTypes.PrintingCashoutTicket,
            };

            var fallbackStrategy = new Mock<IOverlayMessageStrategy>(MockBehavior.Default);
            _overlayMessageStrategyController.Setup(x => x.FallBackStrategy).Returns(fallbackStrategy.Object);
            _overlayMessageStrategyController.Setup(x => x.GameRegistered).Returns(true);
            _overlayMessageStrategyController.Setup(x => x.RegisteredPresentations).Returns(registeredPresentations);

            var expectedMessageData = new MessageOverlayData()
            {
                Text = "Text",
                SubText = "SubText",
                SubText2 = "SubText2"
            };

            var overlayMessageData = new MessageOverlayData();
            fallbackStrategy.Setup(x => x.HandleMessageOverlayCashOut(It.IsAny<MessageOverlayData>(), lastCashOutForcedByMaxBank, cashOutState)).Returns(expectedMessageData);

            var expectedOverride = new PresentationOverrideData("Text\nSubText\nSubText2", expectedPresentation);

            var expectedOverriddenPresentations = new List<PresentationOverrideData>()
            {
                expectedOverride
            };

            var actualOverriddenPresentations = new List<PresentationOverrideData>();

            _presentationService.Setup(x => x.PresentOverriddenPresentation(It.IsAny<IList<PresentationOverrideData>>()))
                .Callback<IList<PresentationOverrideData>>(y => actualOverriddenPresentations = (List<PresentationOverrideData>)y);

            var gameOverlayStrategy = new GameControlledOverlayMessageStrategy(_overlayMessageStrategyController.Object, _presentationService.Object);
            gameOverlayStrategy.LastCashOutAmount = 0; 
            gameOverlayStrategy.HandleMessageOverlayCashOut(overlayMessageData, lastCashOutForcedByMaxBank, cashOutState);

            Assert.AreEqual(0, actualOverriddenPresentations.Count);
        }

        [DataRow(false,
                PresentationOverrideTypes.PrintingCashwinTicket,
                PresentationOverrideTypes.PrintingCashoutTicket,
                PresentationOverrideTypes.TransferingOutCredits)]
        [DataRow(true)]
        [DataTestMethod]
        public void HandleMessageOverlayCashOutGamePresentationsNotRegisteredTest(bool gameRegistered, params PresentationOverrideTypes[] registeredPresentations)
        {
            var fallbackStrategy = new Mock<IOverlayMessageStrategy>(MockBehavior.Default);

            _overlayMessageStrategyController.Setup(x => x.RegisteredPresentations).Returns(registeredPresentations);
            _overlayMessageStrategyController.Setup(x => x.GameRegistered).Returns(gameRegistered);
            _overlayMessageStrategyController.Setup(x => x.FallBackStrategy).Returns(fallbackStrategy.Object);
            
            var gameOverlayStrategy = new GameControlledOverlayMessageStrategy(_overlayMessageStrategyController.Object, _presentationService.Object);

            var overlayMessageData = new MessageOverlayData();
            var lastCashOutForcedByMaxBank = false;
            var cashOutState = LobbyCashOutState.Voucher;

            gameOverlayStrategy.HandleMessageOverlayCashOut(overlayMessageData, lastCashOutForcedByMaxBank, cashOutState);

            fallbackStrategy.Verify(x => x.HandleMessageOverlayCashOut(overlayMessageData, lastCashOutForcedByMaxBank, cashOutState));
        }

        [TestMethod]
        public void HandleMessageOverlayCashInPresentationRegisteredTest()
        {
            var registeredPresentations = new List<PresentationOverrideTypes>()
            {
                PresentationOverrideTypes.TransferingInCredits
            };

            var fallbackStrategy = new Mock<IOverlayMessageStrategy>(MockBehavior.Default);
            _overlayMessageStrategyController.Setup(x => x.FallBackStrategy).Returns(fallbackStrategy.Object);
            _overlayMessageStrategyController.Setup(x => x.GameRegistered).Returns(true);
            _overlayMessageStrategyController.Setup(x => x.RegisteredPresentations).Returns(registeredPresentations);

            var expectedMessageData = new MessageOverlayData()
            {
                Text = "Text",
                SubText = "SubText",
                SubText2 = "SubText2"
            };

            var overlayMessageData = new MessageOverlayData();
            var cashInType = CashInType.Voucher;
            var stateContainsCashOut = false;
            var cashOutState = LobbyCashOutState.Voucher;
            fallbackStrategy.Setup(x => x.HandleMessageOverlayCashIn(It.IsAny<MessageOverlayData>(), cashInType, stateContainsCashOut, cashOutState)).Returns(expectedMessageData);

            var expectedOverride = new PresentationOverrideData("Text\nSubText\nSubText2", PresentationOverrideTypes.TransferingInCredits);

            var expectedOverriddenPresentations = new List<PresentationOverrideData>()
            {
                expectedOverride
            };

            var actualOverriddenPresentations = new List<PresentationOverrideData>();

            _presentationService.Setup(x => x.PresentOverriddenPresentation(It.IsAny<IList<PresentationOverrideData>>()))
                .Callback<IList<PresentationOverrideData>>(y => actualOverriddenPresentations = (List<PresentationOverrideData>)y);

            var gameOverlayStrategy = new GameControlledOverlayMessageStrategy(_overlayMessageStrategyController.Object, _presentationService.Object);

            gameOverlayStrategy.HandleMessageOverlayCashIn(overlayMessageData, cashInType, stateContainsCashOut, cashOutState);

            Assert.AreEqual(expectedOverriddenPresentations[0].Message, actualOverriddenPresentations[0].Message);
            Assert.AreEqual(expectedOverriddenPresentations[0].Type, actualOverriddenPresentations[0].Type);
        }

        [DataRow(false, PresentationOverrideTypes.TransferingInCredits)]
        [DataRow(true)]
        [DataTestMethod]
        public void HandleMessageOverlayCashInGamePresentationsNotRegisteredTest(bool gameRegistered, params PresentationOverrideTypes[] registeredPresentations)
        {
            var fallbackStrategy = new Mock<IOverlayMessageStrategy>(MockBehavior.Default);

            _overlayMessageStrategyController.Setup(x => x.RegisteredPresentations).Returns(registeredPresentations);
            _overlayMessageStrategyController.Setup(x => x.GameRegistered).Returns(gameRegistered);
            _overlayMessageStrategyController.Setup(x => x.FallBackStrategy).Returns(fallbackStrategy.Object);

            var gameOverlayStrategy = new GameControlledOverlayMessageStrategy(_overlayMessageStrategyController.Object, _presentationService.Object);

            var overlayMessageData = new MessageOverlayData();
            var cashInType = CashInType.Voucher;
            var stateContainsCashOut = false;
            var cashOutState = LobbyCashOutState.Voucher;
            gameOverlayStrategy.HandleMessageOverlayCashIn(overlayMessageData, cashInType, stateContainsCashOut, cashOutState);

            fallbackStrategy.Verify(x => x.HandleMessageOverlayCashIn(overlayMessageData, cashInType, stateContainsCashOut, cashOutState));
        }

        [DataRow(HandpayType.GameWin, PresentationOverrideTypes.JackpotHandpay)]
        [DataRow(HandpayType.BonusPay, PresentationOverrideTypes.BonusJackpot)]
        [DataRow(HandpayType.CancelCredit, PresentationOverrideTypes.CancelledCreditsHandpay)]
        [DataTestMethod]
        public void HandleMessageOverlayHandPayPresentationRegisteredTest(
            HandpayType lastHandpayType,
            PresentationOverrideTypes expectedPresentation)
        {
            var registeredPresentations = new List<PresentationOverrideTypes>()
            {
                PresentationOverrideTypes.JackpotHandpay,
                PresentationOverrideTypes.BonusJackpot,
                PresentationOverrideTypes.CancelledCreditsHandpay
            };

            var fallbackStrategy = new Mock<IOverlayMessageStrategy>(MockBehavior.Default);
            _overlayMessageStrategyController.Setup(x => x.FallBackStrategy).Returns(fallbackStrategy.Object);
            _overlayMessageStrategyController.Setup(x => x.GameRegistered).Returns(true);
            _overlayMessageStrategyController.Setup(x => x.RegisteredPresentations).Returns(registeredPresentations);

            var expectedMessageData = new MessageOverlayData()
            {
                Text = "Text",
                SubText = "SubText",
                SubText2 = "SubText2"
            };

            var overlayMessageData = new MessageOverlayData();
            var subText2 = string.Empty;
            fallbackStrategy.Setup(x => x.HandleMessageOverlayHandPay(It.IsAny<MessageOverlayData>(), subText2)).Returns(expectedMessageData);

            var expectedOverride = new PresentationOverrideData("Text\nSubText\nSubText2", expectedPresentation);

            var expectedOverriddenPresentations = new List<PresentationOverrideData>()
            {
                expectedOverride
            };

            var actualOverriddenPresentations = new List<PresentationOverrideData>();

            _presentationService.Setup(x => x.PresentOverriddenPresentation(It.IsAny<IList<PresentationOverrideData>>()))
                .Callback<IList<PresentationOverrideData>>(y => actualOverriddenPresentations = (List<PresentationOverrideData>)y);

            var gameOverlayStrategy = new GameControlledOverlayMessageStrategy(_overlayMessageStrategyController.Object, _presentationService.Object);
            gameOverlayStrategy.LastHandpayType = lastHandpayType;

            gameOverlayStrategy.HandleMessageOverlayHandPay(overlayMessageData, subText2);

            Assert.AreEqual(expectedOverriddenPresentations[0].Message, actualOverriddenPresentations[0].Message);
            Assert.AreEqual(expectedOverriddenPresentations[0].Type, actualOverriddenPresentations[0].Type);
        }

        [DataRow(false,
        PresentationOverrideTypes.PrintingCashwinTicket,
        PresentationOverrideTypes.PrintingCashoutTicket,
        PresentationOverrideTypes.TransferingOutCredits)]
        [DataRow(true)]
        [DataTestMethod]
        public void HandleMessageOverlayHandPayGamePresentationsNotRegisteredTest(bool gameRegistered, params PresentationOverrideTypes[] registeredPresentations)
        {
            var fallbackStrategy = new Mock<IOverlayMessageStrategy>(MockBehavior.Default);

            _overlayMessageStrategyController.Setup(x => x.RegisteredPresentations).Returns(registeredPresentations);
            _overlayMessageStrategyController.Setup(x => x.GameRegistered).Returns(gameRegistered);
            _overlayMessageStrategyController.Setup(x => x.FallBackStrategy).Returns(fallbackStrategy.Object);

            var gameOverlayStrategy = new GameControlledOverlayMessageStrategy(_overlayMessageStrategyController.Object, _presentationService.Object);

            var overlayMessageData = new MessageOverlayData();
            var subText2 = string.Empty;
            gameOverlayStrategy.HandleMessageOverlayHandPay(overlayMessageData, subText2);

            fallbackStrategy.Verify(x => x.HandleMessageOverlayHandPay(overlayMessageData, subText2));
        }
    }
}
