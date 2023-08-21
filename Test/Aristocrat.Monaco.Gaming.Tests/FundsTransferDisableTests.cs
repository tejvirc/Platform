namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Application.Contracts;
    using Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FundsTransferDisableTests
    {
        private FundsTransferDisable _target;

        private static readonly IReadOnlyList<Guid> NoImmediateDisables = new List<Guid> { };
        private static readonly IReadOnlyList<Guid> ImmediateDisables = new List<Guid> { ApplicationConstants.MainDoorGuid };
        private static readonly IReadOnlyList<Guid> ReelsNeedHoming = new List<Guid>
        {
            GamingConstants.ReelsNeedHomingGuid,
            ApplicationConstants.LiveAuthenticationDisableKey
        };

        private readonly Mock<IGamePlayState> _gameState = new Mock<IGamePlayState>(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus= new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _disableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
        private readonly Mock<IPlayerBank> _playerBank = new Mock<IPlayerBank>(MockBehavior.Default);
        private Action<OverlayMenuExitedEvent> overlayExitedHandler = null;
        private Action<OverlayMenuEnteredEvent> overlayEnteredHandler = null;
        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateFundsTransferDisable();
        }

        [DataRow(true, false, false, DisplayName = "Null IGamePlayState")]
        [DataRow(false, true, false, DisplayName = "Null ISystemDisableManager")]
        [DataRow(false, false, true, DisplayName = "Null IEventBus")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(bool nullGameState, bool nullDisableManager, bool nullEventBus)
        {
            CreateFundsTransferDisable(nullGameState, nullDisableManager, nullEventBus);
        }

        [DataRow(false, "NoImmediateDisables", true, true, true, false, true, false, DisplayName = "In game round with active transaction should disable money on and off")]
        [DataRow(false, "NoImmediateDisables", true, false, true, false, true, false, DisplayName = "In game round without active transaction should disable money on and off")]
        [DataRow(false, "NoImmediateDisables", false, false, false, false, false, false, DisplayName = "All transfer types are enabled when not disabled and not in a game round")]
        [DataRow(true, "NoImmediateDisables", false, false, false, true, false, false, DisplayName = "A non immediate disabled should disable money on")]
        [DataRow(true, "ImmediateDisables", false, false, false, true, true, false, DisplayName = "A immediate disable should disable money on and off")]
        [DataRow(false, "NoImmediateDisables", false, false, false, false, false, true, DisplayName = "Transfer disabled with overlay window")]
        [DataRow(true, "ReelsNeedHoming", false, false, false, true, false, false, DisplayName = "The ReelsNeedHoming fault shouldn't prevent cashing out")]
        [DataTestMethod]
        public void TransferTypeDisabledTest(
            bool disabled,
            string immediateDisables,
            bool inGameRound,
            bool activeTransaction,
            bool transferOnDisabled,
            bool transferOnDisabledTilt,
            bool transferOffDisabled,
            bool transferOnDisabledOverlay)
        {
            _gameState.Setup(m => m.UncommittedState).Returns(inGameRound ? PlayState.Initiated : PlayState.Idle);
            SetupCurrentImmediateDisableKeys(immediateDisables);

            _disableManager.Setup(m => m.IsDisabled).Returns(disabled);
            _playerBank.Setup(x => x.TransactionId).Returns(activeTransaction ? Guid.NewGuid() : Guid.Empty);
            if (transferOnDisabledOverlay) { overlayEnteredHandler.Invoke(new OverlayMenuEnteredEvent()); }
            Assert.AreEqual(transferOffDisabled, _target.TransferOffDisabled);
            Assert.AreEqual(transferOnDisabled, _target.TransferOnDisabledInGame);
            Assert.AreEqual(transferOnDisabledTilt, _target.TransferOnDisabledTilt);
            Assert.AreEqual(transferOnDisabledOverlay, _target.TransferOnDisabledOverlay);
        }

        [TestMethod]
        public void AFTEnabled_WhenPresentationIdle()
        {
            _gameState.Setup(m => m.UncommittedState).Returns(PlayState.PresentationIdle);

            overlayEnteredHandler.Invoke(new OverlayMenuEnteredEvent());

            Assert.IsFalse(_target.TransferOnDisabledInGame);
        }

        [TestMethod]
        public void AFTDisabled_WhenOverlayEnteredAndExited_SwitchesTransferOnDisabledOverLayFlag()
        {
            Assert.AreEqual(false, _target.TransferOnDisabledOverlay);
            overlayEnteredHandler.Invoke(new OverlayMenuEnteredEvent());
            Assert.AreEqual(true, _target.TransferOnDisabledOverlay);
            overlayExitedHandler.Invoke(new OverlayMenuExitedEvent());
            Assert.AreEqual(false, _target.TransferOnDisabledOverlay);
        }                                 

        private void SetupEventBus(Mock<IEventBus> eventBus)
        {
            
            eventBus?.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OverlayMenuExitedEvent>>()))
                .Callback<object, Action<OverlayMenuExitedEvent>>((o, h) => overlayExitedHandler = h);
            eventBus?.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OverlayMenuEnteredEvent>>()))
                .Callback<object, Action<OverlayMenuEnteredEvent>>((o, h) => overlayEnteredHandler = h);
        }
        private FundsTransferDisable CreateFundsTransferDisable(
            bool nullGameState = false,
            bool nullDisableManager = false,
            bool nullEventBus = false)
        {
            SetupEventBus(_eventBus);
            return new FundsTransferDisable(
                nullGameState ? null : _gameState.Object,
                nullDisableManager ? null : _disableManager.Object,
                nullEventBus ? null : _eventBus.Object);
        }

        private void SetupCurrentImmediateDisableKeys(string immediateDisables)
        {
            switch (immediateDisables)
            {
                case "ImmediateDisables":
                    _disableManager.Setup(m => m.CurrentImmediateDisableKeys).Returns(ImmediateDisables);
                    break;
                case "ReelsNeedHoming":
                    _disableManager.Setup(m => m.CurrentImmediateDisableKeys).Returns(ReelsNeedHoming);
                    break;
                case "NoImmediateDisables":
                default:
                    _disableManager.Setup(m => m.CurrentImmediateDisableKeys).Returns(NoImmediateDisables);
                    break;
            }
        }
    }
}
