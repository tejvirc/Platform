namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Net.NetworkInformation;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
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

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(bool nullRegistration, bool nullProperties, bool nullNetworkProvider)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => CreateTarget(nullRegistration, nullProperties, nullNetworkProvider));
        }

        [TestMethod]
        public async Task GoodResponseCodeHandleTest()
        {
            _registrationServer.Setup(x => x.RegisterClient(It.Is<ProgressiveRegistrationMessage>(
                m => string.Equals(m.MachineSerial, MachineId.ToString()) &&
                string.Equals(m.GameTitleId, GameTitleId)), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new RegistrationResults(ResponseCode.Ok)))
                .Verifiable();
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
                .Returns(Task.FromResult(new RegistrationResults(code)))
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
            bool nullUnitOfWorkFactory = false)
        {
            return new ProgressiveRegistrationCommandHandler(
                nullRegistration ? null : _registrationServer.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullUnitOfWorkFactory ? null : _unitOfWorkFactory.Object);
        }
    }
}