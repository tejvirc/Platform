namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts.Identification;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Common;
    using Mgam.Consumers;
    using Mgam.Services.PlayerTracking;
    using Gaming;
    using Gaming.Contracts;
    using Hardware.Contracts.Audio;
    using Hardware.Contracts.TowerLight;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EndSession = Commands.EndSession;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Localization.Properties;
    using Aristocrat.Monaco.Mgam.Services.CreditValidators;

    [TestClass]
    public class GameIdleConsumerTest
    {
        private Mock<ICashOut> _bank;
        private Mock<ILogger<GameIdleConsumer>> _logger;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IAudio> _audio;
        private Mock<ITowerLight> _towerLight;
        private Mock<IPlayerTracking> _playerTracking;
        private Mock<IIdentificationValidator> _idValidator;
        private Mock<IEmployeeLogin> _employeeLogin;
        private Mock<ISystemDisableManager> _disableManager;
        private GameIdleConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bank = new Mock<ICashOut>(MockBehavior.Default);
            _logger = new Mock<ILogger<GameIdleConsumer>>(MockBehavior.Default);
            _commandFactory = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _audio = new Mock<IAudio>(MockBehavior.Default);
            _towerLight = new Mock<ITowerLight>(MockBehavior.Default);
            _playerTracking = new Mock<IPlayerTracking>(MockBehavior.Default);
            _idValidator = new Mock<IIdentificationValidator>(MockBehavior.Default);
            _employeeLogin = new Mock<IEmployeeLogin>(MockBehavior.Default);
            _disableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);

            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), false)).Returns(false);
        }

        [DataRow(false, true, true, true, true, true, true, true, true, true, DisplayName = "Null Command Handler Factory Object")]
        [DataRow(true, false, true, true, true, true, true, true, true, true, DisplayName = "Null Logger Object")]
        [DataRow(true, true, false, true, true, true, true, true, true, true, DisplayName = "Null Bank Object")]
        [DataRow(true, true, true, false, true, true, true, true, true, true, DisplayName = "Null Properties Manager Object")]
        [DataRow(true, true, true, true, false, true, true, true, true, true, DisplayName = "Null Audio Service Object")]
        [DataRow(true, true, true, true, true, false, true, true, true, true, DisplayName = "Null Tower Light Service Object")]
        [DataRow(true, true, true, true, true, true, false, true, true, true, DisplayName = "Null Player Tracking Object")]
        [DataRow(true, true, true, true, true, true, true, false, true, true, DisplayName = "Null ID Validator Object")]
        [DataRow(true, true, true, true, true, true, true, true, false, true, DisplayName = "Null Employee Login Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, false, DisplayName = "Null System Disable Manager Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool commandFactory,
            bool logger,
            bool bank,
            bool propertiesManager,
            bool audioService,
            bool towerLightService,
            bool playerTracking,
            bool idValidator,
            bool employeeLogin,
            bool disableManager)
        {
            _target = new GameIdleConsumer(
                logger ? _logger.Object : null,
                commandFactory ? _commandFactory.Object : null,
                bank ? _bank.Object : null,
                propertiesManager ? _propertiesManager.Object : null,
                audioService ? _audio.Object : null,
                towerLightService ? _towerLight.Object : null,
                playerTracking ? _playerTracking.Object : null,
                idValidator ? _idValidator.Object : null,
                employeeLogin ? _employeeLogin.Object : null,
                disableManager ? _disableManager.Object : null);
        }

        [TestMethod]
        public void SuccessfulConstructorTest()
        {
            CreateNewTarget();
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void TestConsumerSuccess()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), false)).Returns(false);
            _bank.SetupGet(b => b.Balance).Returns(0).Verifiable();
            _commandFactory.Setup(c => c.Execute(It.IsAny<EndSession>())).Returns(Task.CompletedTask).Verifiable();

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _bank.Verify();
            _commandFactory.Verify();
        }

        [TestMethod]
        public void TestConsumerFailed()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(It.IsAny<string>(), false)).Returns(false);
            _bank.SetupGet(b => b.Balance).Returns(0).Verifiable();
            _commandFactory.Setup(c => c.Execute(It.IsAny<EndSession>()))
                .Throws(new ServerResponseException(ServerResponseCode.ServerError)).Verifiable();

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _bank.Verify();
            _commandFactory.Verify();
        }

        [TestMethod]
        public void TestConsumerEndPlayerSession()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(MgamConstants.EndPlayerSessionAfterGameRoundKey, false)).Returns(true).Verifiable();
            _propertiesManager.Setup(p => p.SetProperty(MgamConstants.EndPlayerSessionAfterGameRoundKey, false)).Verifiable();
            _playerTracking.Setup(p => p.EndPlayerSession()).Verifiable();

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _playerTracking.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void TestConsumerForcedCashout()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(MgamConstants.ForceCashoutAfterGameRoundKey, false)).Returns(true).Verifiable();
            _propertiesManager.Setup(p => p.SetProperty(MgamConstants.ForceCashoutAfterGameRoundKey, false)).Verifiable();
            _bank.SetupGet(b => b.Balance).Returns(10000).Verifiable();
            _bank.Setup(b => b.CashOut()).Verifiable();

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _bank.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void TestConsumerLogoffPlayer()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(MgamConstants.LogoffPlayerAfterGameRoundKey, false)).Returns(true).Verifiable();
            _propertiesManager.Setup(p => p.SetProperty(MgamConstants.LogoffPlayerAfterGameRoundKey, false)).Verifiable();
            _idValidator.Setup(p => p.LogoffPlayer()).Returns(Task.CompletedTask);

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _idValidator.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void TestConsumerPlayAlarm()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(MgamConstants.PlayAlarmAfterGameRoundKey, false)).Returns(true).Verifiable();
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.HostOfflineSoundKey, string.Empty)).Returns("test.file").Verifiable();
            _propertiesManager.Setup(p => p.GetProperty(ApplicationConstants.AlertVolumeKey, MgamConstants.DefaultAlertVolume)).Returns(MgamConstants.DefaultAlertVolume).Verifiable();
            _propertiesManager.Setup(p => p.SetProperty(MgamConstants.PlayAlarmAfterGameRoundKey, false)).Verifiable();
            _audio.Setup(p => p.Load(SoundName.HostOfflineSound, It.IsAny<string>())).Returns(true);
            _audio.Setup(p => p.Play(SoundName.HostOfflineSound, It.IsAny<int>(), It.IsAny<float>(), SpeakerMix.All, null)).Verifiable();
            _towerLight.Setup(t => t.SetFlashState(LightTier.Tier1, FlashState.FastFlash, TimeSpan.MaxValue, false)).Verifiable();

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _audio.Verify();
            _towerLight.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void TestConsumerForcedDisableGamePlay()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(MgamConstants.DisableGamePlayAfterGameRoundKey, false)).Returns(true).Verifiable();
            _propertiesManager.Setup(p => p.SetProperty(MgamConstants.DisableGamePlayAfterGameRoundKey, false)).Verifiable();
            _disableManager.Setup(d => d.Disable(MgamConstants.GamePlayDisabledKey, SystemDisablePriority.Normal, It.IsAny<Func<string>>(), It.IsAny<Type>())).Verifiable();

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _bank.Verify();
            _propertiesManager.Verify();
        }

        [TestMethod]
        public void TestConsumerEnterDropMode()
        {
            CreateNewTarget();
            _propertiesManager.Setup(p => p.GetProperty(MgamConstants.EnterDropModeAfterGameRoundKey, false)).Returns(true).Verifiable();
            _propertiesManager.Setup(p => p.SetProperty(MgamConstants.EnterDropModeAfterGameRoundKey, false)).Verifiable();
            _employeeLogin.Setup(e => e.Login(ResourceKeys.DropMode)).Verifiable();
            _audio.Setup(a => a.SetSystemMuted(true)).Verifiable();

            _target.Consume(new GameIdleEvent(1, 1, "123", new GameHistoryLog(1)), CancellationToken.None).Wait(10);

            _employeeLogin.Verify();
            _audio.Verify();
            _propertiesManager.Verify();
        }

        private void CreateNewTarget()
        {
            _target = new GameIdleConsumer(_logger.Object, _commandFactory.Object, _bank.Object, _propertiesManager.Object, _audio.Object,
                _towerLight.Object, _playerTracking.Object, _idValidator.Object, _employeeLogin.Object, _disableManager.Object);
        }
    }
}