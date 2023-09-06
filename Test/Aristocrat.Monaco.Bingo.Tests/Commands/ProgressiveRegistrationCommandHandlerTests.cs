namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.Progressives;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Common.Exceptions;
    using Common.Storage.Model;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class ProgressiveRegistrationCommandHandlerTests
    {
        private const uint MachineId = 1234;
        private int GameTitleId = 0; // Not currently mocked to get a non-zero value

        private readonly Mock<IProgressiveRegistrationService> _registrationServer = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Strict);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
        private readonly Mock<IGameDetail> _gameDetail = new Mock<IGameDetail>(MockBehavior.Default);
        private readonly Mock<IDenomination> _denomination = new Mock<IDenomination>(MockBehavior.Default);
        private readonly Mock<ISubGameDetails> _subGameDetails = new Mock<ISubGameDetails>(MockBehavior.Default);
        private ProgressiveRegistrationCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _unitOfWork.Setup(x => x.BeginTransaction(IsolationLevel.ReadCommitted));
            _unitOfWork.Setup(x => x.Commit());
            _unitOfWork.Setup(x => x.Dispose());
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);

            // Setup for cross game progressive check
            var gameId = 1;
            var gameDetail = new Mock<IGameDetail>();
            var denomination = new Mock<IDenomination>();
            denomination.Setup(m => m.Value).Returns(1L);
            gameDetail.Setup(m => m.Id).Returns(gameId);
            var listOfDenominations = new List<IDenomination>();
            listOfDenominations.Add(denomination.Object);
            gameDetail.Setup(m => m.Denominations).Returns(listOfDenominations);
            var listOfGames = new List<IGameDetail>();
            listOfGames.Add(gameDetail.Object);
            _propertiesManager.Setup(x => x.GetProperty(ApplicationConstants.MachineId, It.IsAny<uint>())).Returns(MachineId);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(gameId);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(1L);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.Games, null)).Returns(listOfGames);
            var bingoConfig = new BingoServerSettingsModel();
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(bingoConfig);

            _target = CreateTarget();
        }

        [DataRow(true, false, false, false, DisplayName = "Null registration")]
        [DataRow(false, true, false, false, DisplayName = "Null properties")]
        [DataRow(false, false, true, false, DisplayName = "Null network provider")]
        [DataRow(false, false, false, true, DisplayName = "Null game provider")]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullRegistration, bool nullProperties, bool nullNetworkProvider, bool nullGameProvider)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => CreateTarget(nullRegistration, nullProperties, nullNetworkProvider, nullGameProvider));
        }

        [TestMethod]
        public async Task GoodResponseCodeHandleTest()
        {
            _registrationServer.Setup(x => x.RegisterClient(It.Is<ProgressiveRegistrationMessage>(
                m => string.Equals(m.MachineSerial, MachineId.ToString()) &&
                string.Equals(m.GameTitleId, GameTitleId)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ProgressiveRegistrationResults(ResponseCode.Ok)))
                .Verifiable();
            SetupGameDetails();

            await _target.Handle(new ProgressiveRegistrationCommand());
            _registrationServer.Verify();
        }

        [DataRow(ResponseCode.Rejected)]
        [DataRow(ResponseCode.Disconnected)]
        [DataTestMethod]
        [ExpectedException(typeof(RegistrationException))]
        public async Task InvalidResponseCodeHandleTest(ResponseCode code)
        {
            _registrationServer.Setup(x => x.RegisterClient(It.Is<ProgressiveRegistrationMessage>(
                m => string.Equals(m.MachineSerial, MachineId) &&
                string.Equals(m.GameTitleId, GameTitleId)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ProgressiveRegistrationResults(code)))
                .Verifiable();
            await _target.Handle(new ProgressiveRegistrationCommand());
        }

        [TestMethod]
        [ExpectedException(typeof(RegistrationException))]
        public async Task RegisterThrowsTest()
        {
            _registrationServer.Setup(x => x.RegisterClient(It.Is<ProgressiveRegistrationMessage>(
                m => string.Equals(m.MachineSerial, MachineId) &&
                string.Equals(m.GameTitleId, GameTitleId)), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException());
            await _target.Handle(new ProgressiveRegistrationCommand());
        }

        private ProgressiveRegistrationCommandHandler CreateTarget(
            bool nullRegistration = false,
            bool nullProperties = false,
            bool nullUnitOfWorkFactory = false,
            bool nullGameProvider = false)
        {
            return new ProgressiveRegistrationCommandHandler(
                nullRegistration ? null : _registrationServer.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullUnitOfWorkFactory ? null : _unitOfWorkFactory.Object,
                nullGameProvider ? null : _gameProvider.Object);
        }

        private void SetupGameDetails()
        {
            _subGameDetails.Setup(x => x.CdsTitleId).Returns("101");
            var denom1 = new Mock<IDenomination>();
            denom1.Setup(x => x.Value).Returns(1);
            var denom2 = new Mock<IDenomination>();
            denom2.Setup(x => x.Value).Returns(25);
            var denom3 = new Mock<IDenomination>();
            denom3.Setup(x => x.Value).Returns(100);
            var denoms = new List<IDenomination> { denom1.Object, denom2.Object, denom3.Object };
            _subGameDetails.Setup(x => x.Denominations).Returns(denoms);
            var progressiveGames = new List<ISubGameDetails>
            {
                _subGameDetails.Object
            };

            _gameProvider.Setup(x => x.GetGame(It.IsAny<int>())).Returns(_gameDetail.Object);
            _gameProvider.Setup(x => x.GetEnabledSubGames(_gameDetail.Object)).Returns(progressiveGames);
        }
    }
}