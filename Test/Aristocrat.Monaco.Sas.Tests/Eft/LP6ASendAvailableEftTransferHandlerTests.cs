namespace Aristocrat.Monaco.Sas.Tests.Eft
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Sas.Contracts.Eft;
    using Aristocrat.Monaco.Sas.Eft;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.EFT;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Application.OperatorMenu;
    using static Aristocrat.Sas.Client.EFT.AvailableEftTransferResponse;

    [TestClass]
    public class LP6ASendAvailableEftTransferHandlerTests
    {
        private LP6ASendAvailableEftTransferHandler _target;
        private Mock<IEftTransferProvider> _eftProvider;
        private Mock<ISystemDisableManager> _systemDisableManager;
        private Mock<IGamePlayState> _gamePlayState;
        private Mock<IOperatorMenuLauncher> _operatorMenuLauncher;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _eftProvider = new Mock<IEftTransferProvider>(MockBehavior.Strict);
            _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Strict);
            _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);
            _operatorMenuLauncher = new Mock<IOperatorMenuLauncher>(MockBehavior.Strict);
            _target = new LP6ASendAvailableEftTransferHandler(_eftProvider.Object, _systemDisableManager.Object, _gamePlayState.Object, _operatorMenuLauncher.Object);
            Setup();
        }

        [DataRow(true, false, false, false, DisplayName = "Null IEftTransferProvider")]
        [DataRow(false, true, false, false, DisplayName = "Null ISystemDisableManager")]
        [DataRow(false, false, true, false, DisplayName = "Null IGamePlayState")]
        [DataRow(false, false, false, true, DisplayName = "Null IOperatorMenuLauncher")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(bool nullEftProvider, bool nullDisableManager, bool nullGamePlayState, bool nullOperatorMenuLauncher)
        {
            _ = ConstructHandler(nullEftProvider, nullDisableManager, nullGamePlayState, nullOperatorMenuLauncher);
        }

        private LP6ASendAvailableEftTransferHandler ConstructHandler(bool nullEftProvider, bool nullDisableManager, bool nullGamePlayState, bool nullOperatorMenuLauncher)
        {
            return new LP6ASendAvailableEftTransferHandler(
                nullEftProvider ? null : _eftProvider.Object,
                nullDisableManager ? null : _systemDisableManager.Object,
                nullGamePlayState ? null : _gamePlayState.Object,
                nullOperatorMenuLauncher ? null : _operatorMenuLauncher.Object
                );
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EftSendAvailableEftTransfers));
        }

        private void Setup()
        {
            _gamePlayState.Setup(x => x.CurrentState).Returns(PlayState.Idle);
            _operatorMenuLauncher.Setup(x => x.IsShowing).Returns(false);
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((true, true));
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(It.IsAny<IReadOnlyList<Guid>>());
        }

        [TestMethod]
        public void HandlerInGameplayTest()
        {
            var longPollData = new LongPollData();
            _gamePlayState.Setup(x => x.CurrentState).Returns(PlayState.Initiated);
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = 0 };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerInOperatorMenuTest()
        {
            var longPollData = new LongPollData();
            _operatorMenuLauncher.Setup(x => x.IsShowing).Returns(true);
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = 0 };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerOnlyEftInAllowedWhileNotInLockUpTest()
        {
            var longPollData = new LongPollData();
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((true, false));
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(false);
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = EftTransferAvailability.TransferToGamingMachine };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerOnlyEftInAllowedWhileInLockUpOtherThanEftLockupTest()
        {
            var longPollData = new LongPollData();
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((true, false));
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(true);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid> { ApplicationConstants.DisabledByHost0Key });
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = 0 };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerOnlyEftInAllowedWhileInOnlyEftLockupTest()
        {
            var longPollData = new LongPollData();
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((true, false));
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(true);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid> { SasConstants.EftTransactionLockUpGuid });
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = EftTransferAvailability.TransferToGamingMachine };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerOnlyEftOutAllowedWhileNotInLockUpTest()
        {
            var longPollData = new LongPollData();
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((false, true));
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(false);
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = EftTransferAvailability.TransferFromGamingMachine };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerOnlyEftOutAllowedWhileInOnlyEftOrHostLockUpTest()
        {
            var longPollData = new LongPollData();
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((false, true));
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(true);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>() { SasConstants.EftTransactionLockUpGuid });
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = EftTransferAvailability.TransferFromGamingMachine };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);

            //Testing for Host enbled Lockup
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(true);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.DisabledByHost0Key });
            result = _target.Handle(longPollData);
            expected = new AvailableEftTransferResponse { TransferAvailability = EftTransferAvailability.TransferFromGamingMachine };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerOnlyEftOutAllowedWhileInOthrThanEftOrHostLockUpTest()
        {
            var longPollData = new LongPollData();
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((false, true));
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(true);
            _systemDisableManager.Setup(x => x.CurrentDisableKeys).Returns(new List<Guid>() { ApplicationConstants.LiveAuthenticationDisableKey });
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = 0 };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }

        [TestMethod]
        public void HandlerEftInAndEftOutAllowedWhileNotInLockup()
        {
            var longPollData = new LongPollData();
            _eftProvider.Setup(x => x.GetSupportedTransferTypes()).Returns((true, true));
            _systemDisableManager.Setup(x => x.IsDisabled).Returns(false);
            var result = _target.Handle(longPollData);
            var expected = new AvailableEftTransferResponse { TransferAvailability = EftTransferAvailability.TransferFromGamingMachine | EftTransferAvailability.TransferToGamingMachine };
            Assert.AreEqual(expected.TransferAvailability, result.TransferAvailability);
        }
    }
}
