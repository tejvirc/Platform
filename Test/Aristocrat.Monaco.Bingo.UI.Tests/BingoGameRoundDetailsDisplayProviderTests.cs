namespace Aristocrat.Monaco.Bingo.UI.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Application.Contracts.OperatorMenu;
    using Common;
    using Common.GameOverlay;
    using Common.Storage;
    using Gaming.Contracts.Central;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;
    using Views.OperatorMenu;

    [TestClass]
    public class BingoGameRoundDetailsDisplayProviderTests
    {
        private BingoGameRoundDetailsDisplayProvider _target;
        private Mock<ICentralProvider> _centralProvider;
        private Mock<INotifyPropertyChanged> _ownerViewModel;
        private Mock<IDialogService> _dialogService;
        private CentralTransaction _transaction;
        private const long CentralTransactionId = 12;
        private const string WindowTitle = "sample";

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            MockLocalization.Setup(MockBehavior.Default);
            _centralProvider = new Mock<ICentralProvider>(MockBehavior.Strict);
            _ownerViewModel = new Mock<INotifyPropertyChanged>(MockBehavior.Strict);
            _dialogService = new Mock<IDialogService>(MockBehavior.Strict);

            _transaction = new CentralTransaction()
            {
                TransactionId = CentralTransactionId,
                Descriptions = new List<IOutcomeDescription>()
                {
                    new BingoGameDescription()
                    {
                        GameSerial = 11,
                        JoinBallIndex = 0,
                        Cards = new List<BingoCard>()
                        {
                            GetCard()
                        },
                        BallCallNumbers = new List<BingoNumber>(),
                        GameEndWinClaimAccepted = false,
                        GameTitleId = 10,
                        DenominationId = 9
                    }
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_Fail()
        {
            _target = new BingoGameRoundDetailsDisplayProvider(null);
        }

        [DataRow(true, false, false, DisplayName = "Null INotifyPropertyChanged")]
        [DataRow(false, true, false, DisplayName = "Null IDialogService")]
        [DataRow(false, false, true, DisplayName = "Null window title string")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Display_Fail(bool nullOwnerViewModel, bool nullDialogService, bool nullWindowTitle)
        {
            _target = new BingoGameRoundDetailsDisplayProvider(_centralProvider.Object);
            _target.Display(
                nullOwnerViewModel ? null : _ownerViewModel.Object,
                nullDialogService ? null : _dialogService.Object,
                nullWindowTitle ? null : WindowTitle,
                CentralTransactionId);
        }

        [TestMethod]
        public void Display()
        {
            _centralProvider.Setup(x => x.Transactions)
                .Returns(new List<CentralTransaction> { _transaction } )
                .Verifiable();
            _dialogService.Setup(x => x.ShowInfoDialog<BingoGameHistoryDetailsView>(
                _ownerViewModel.Object,
                It.IsAny<BingoGameHistoryDetailsViewModel>(),
                It.IsAny<string>()))
                .Returns(true)
                .Verifiable();

            _target = new BingoGameRoundDetailsDisplayProvider(_centralProvider.Object);
            _target.Display(_ownerViewModel.Object, _dialogService.Object, WindowTitle, CentralTransactionId);

            _centralProvider.Verify();
            _dialogService.Verify();
        }

        private static BingoCard GetCard()
        {
            var card = new BingoCard(
                new BingoNumber[BingoConstants.BingoCardDimension, BingoConstants.BingoCardDimension],
                2,
                0,
                0,
                false);

            for (var i = 0; i < BingoConstants.BingoCardDimension; i++)
            {
                for (var j = 0; j < BingoConstants.BingoCardDimension; j++)
                {
                    card.Numbers[i, j] = new BingoNumber(0, BingoNumberState.Undefined);
                }
            }

            return card;
        }
    }
}
