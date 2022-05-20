namespace Aristocrat.Monaco.Gaming.Tests.Commands.RuntimeEvents
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Contracts;
    using GameRound;
    using Gaming.Commands;
    using Gaming.Commands.RuntimeEvents;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Vgt.Client12.Application.OperatorMenu;

    /// <summary>
    ///     Contains unit tests for the PrimaryEventHandler class
    /// </summary>
    [TestClass]
    public class PrimaryEventHandlerTests
    {
        private PrimaryEventHandler _target;
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _commandFactory = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
        private readonly Mock<IRuntime> _runtime = new Mock<IRuntime>(MockBehavior.Default);
        private readonly Mock<IGamePlayState> _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
        private readonly Mock<IGameHistory> _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);
        private readonly Mock<IGameRecovery> _recovery = new Mock<IGameRecovery>(MockBehavior.Default);
        private readonly Mock<IPersistentStorageManager> _persistentStorage = new Mock<IPersistentStorageManager>(MockBehavior.Default);
        private readonly Mock<IPlayerBank> _bank = new Mock<IPlayerBank>(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
        private readonly Mock<IGameCashOutRecovery> _gameCashOutRecovery = new Mock<IGameCashOutRecovery>(MockBehavior.Default);
        private readonly Mock<IOperatorMenuLauncher> _operatorMenu = new Mock<IOperatorMenuLauncher>(MockBehavior.Default);
        private readonly Mock<IGameRoundInfoParserFactory> _parser = new Mock<IGameRoundInfoParserFactory>(MockBehavior.Default);

        [TestInitialize]
        public void TestInitialize()
        {
            // mock needed by the base class constructor
            _properties.Setup(m => m.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);

            _target = CreatePrimaryEventHandler();
        }

        [DataRow(false, true, true, true, true, true, true, true, true, true, true, DisplayName = "Null Properties")]
        [DataRow(true, false, true, true, true, true, true, true, true, true, true, DisplayName = "Null CommandHandler")]
        [DataRow(true, true, false, true, true, true, true, true, true, true, true, DisplayName = "Null Runtime")]
        [DataRow(true, true, true, false, true, true, true, true, true, true, true, DisplayName = "Null GamePlayState")]
        [DataRow(true, true, true, true, false, true, true, true, true, true, true, DisplayName = "Null GameHistory")]
        [DataRow(true, true, true, true, true, false, true, true, true, true, true, DisplayName = "Null GameRecovery")]
        [DataRow(true, true, true, true, true, true, false, true, true, true, true, DisplayName = "Null PersistentStorageManager")]
        [DataRow(true, true, true, true, true, true, true, false, true, true, true, DisplayName = "Null PlayerBank")]
        [DataRow(true, true, true, true, true, true, true, true, false, true, true, DisplayName = "Null SystemDisableManager")]
        [DataRow(true, true, true, true, true, true, true, true, true, false, true, DisplayName = "Null GameCashOutRecovery")]
        [DataRow(true, true, true, true, true, true, true, true, true, true, false, DisplayName = "Null OperatorMenuLauncher")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertiesTest(
            bool properties = true,
            bool commandFactory = true,
            bool runtime = true,
            bool gamePlay = true,
            bool gameHistory = true,
            bool recovery = true,
            bool storage = true,
            bool bank = true,
            bool disable = true,
            bool cashout = true,
            bool menu = true)
        {
            _target = CreatePrimaryEventHandler(
                properties, commandFactory, runtime, gamePlay, gameHistory, recovery, storage, bank, disable, cashout, menu);
        }

        private PrimaryEventHandler CreatePrimaryEventHandler(
            bool properties = true,
            bool commandFactory = true,
            bool runtime = true,
            bool gamePlayState = true,
            bool gameHistory = true,
            bool recovery = true,
            bool persistentStorage = true,
            bool bank = true,
            bool systemDisableManager = true,
            bool gameCashOutRecovery = true,
            bool operatorMenu = true)
        {
            return new PrimaryEventHandler(
                properties ? _properties.Object : null,
                commandFactory ? _commandFactory.Object : null,
                runtime ? _runtime.Object : null,
                gamePlayState ? _gamePlayState.Object : null,
                gameHistory ? _gameHistory.Object : null,
                recovery ? _recovery.Object : null,
                persistentStorage ? _persistentStorage.Object : null,
                bank ? _bank.Object : null,
                systemDisableManager ? _systemDisableManager.Object : null,
                gameCashOutRecovery ? _gameCashOutRecovery.Object : null,
                operatorMenu ? _operatorMenu.Object : null);
        }
    }
}
