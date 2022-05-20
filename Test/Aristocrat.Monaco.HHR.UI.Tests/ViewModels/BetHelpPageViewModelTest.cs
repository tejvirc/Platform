namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Hardware.Contracts.Button;
    using Hhr.UI.ViewModels;
    using Kernel;
    using Menu;
    using Test.Common;
    using Moq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BetHelpPageViewModelTest
    {
        private BetHelpPageViewModel _betHelpPageViewModel;
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        private readonly List<ProgressiveLevel> _progressiveLevels = new List<ProgressiveLevel>();
        private HHRCommandEventArgs _commandEvent;

        private readonly Mock<IPropertiesManager>
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);

        private readonly Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter =
            new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Strict);

        private int _buttonId = -1;

        [TestInitialize]
        public void TestInitialization()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<DownEvent>()))
                .Callback((DownEvent ev) => { _buttonId = ev.LogicalId; }).Verifiable();

            // Reset the bet up/down delay counter so that we don't hit the limit while testing.
            HostPageViewModelManager.ResetBetButtonDelayLimit(
                DateTime.UtcNow.Ticks - HostPageViewModelManager.BetUpDownDelayTimeTicks - 1);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _progressiveLevels?.Clear();
            _betHelpPageViewModel?.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ClickButton_BetUp_ExpectDownEventWithBetUP()
        {
            _betHelpPageViewModel = ConstructBetHelpPageViewModel();
            _betHelpPageViewModel.Init(Command.Bet);
            _betHelpPageViewModel.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetUp);
            Assert.AreEqual((int)ButtonLogicalId.BetUp, _buttonId);
            _eventBus.Verify(x => x.Publish(It.IsAny<DownEvent>()), Times.Once);
        }

        [TestMethod]
        public void ClickButton_BetDown_ExpectDownEventWithBetDown()
        {
            _betHelpPageViewModel = ConstructBetHelpPageViewModel();
            _betHelpPageViewModel.Init(Command.Bet);
            _betHelpPageViewModel.Commands.First(c => c.Command == Command.Bet).Execute(Command.BetDown);
            Assert.AreEqual((int)ButtonLogicalId.BetDown, _buttonId);
            _eventBus.Verify(x => x.Publish(It.IsAny<DownEvent>()), Times.Once);
        }

        [TestMethod]
        public void ClickButton_ExitHelp_ExpectExitHelpFired()
        {
            _betHelpPageViewModel = ConstructBetHelpPageViewModel();
            _betHelpPageViewModel.Init(Command.Bet);
            _betHelpPageViewModel.HhrButtonClicked += Handle;
            _betHelpPageViewModel.Commands.First(c => c.Command == Command.Bet).Execute(Command.ExitHelp);
            Assert.IsTrue(_commandEvent.Command == Command.ExitHelp);
        }

        [DataRow(true, false, false, DisplayName = "Null EventBus")]
        [DataRow(false, true, false, DisplayName = "Null PropertiesManager")]
        [DataRow(false, false, true, DisplayName = "Null ProtocolLinkedProgressiveAdapter")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void InitializeWithNullArgumentExpectException(
            bool nullEventBus,
            bool nullPropertiesManager,
            bool nullProtocolLinkedProgressiveAdapter)
        {
            _ = ConstructBetHelpPageViewModel(nullEventBus, nullPropertiesManager,
                nullProtocolLinkedProgressiveAdapter);
        }

        [DataRow(2, 1, 1, 1, 1000, 12345000, LevelCreationType.All, ProgressivePoolCreation.WagerBased)]
        [DataRow(2, 1, 1, 1, 1000, 12345000, LevelCreationType.Default, ProgressivePoolCreation.WagerBased)]
        [DataRow(2, 1, 1, 1, 1000, 12345000, LevelCreationType.Default, ProgressivePoolCreation.Default)]
        [DataRow(2, 1, 1, 1, 1000, 12345000, LevelCreationType.All, ProgressivePoolCreation.Default)]
        [DataTestMethod]
        public void ClickNextButton_CheckProgressivesSupported_ExpectProgressivePageIfSupported(
            int levelsToCreate, int progId, int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, int wager, long currentAmount = 0,
            LevelCreationType levelCreationType = LevelCreationType.Default,
            ProgressivePoolCreation progressivePoolCreation = ProgressivePoolCreation.WagerBased)
        {
            SetupProgressivePoolCreationType(progressivePoolCreation);
            CreateProgressiveLevels(levelsToCreate, progId,
                progLevelId, linkedLevelId,
                linkedProgressiveGroupId, ProtocolNames.HHR, wager, currentAmount, levelCreationType);

            _betHelpPageViewModel = ConstructBetHelpPageViewModel();
            _betHelpPageViewModel.Init(Command.Bet);
            _betHelpPageViewModel.HhrButtonClicked += Handle;

            _betHelpPageViewModel.Commands.First(c => c.Command == Command.Bet).Execute(Command.Next);

            if (levelCreationType == LevelCreationType.Default ||
                progressivePoolCreation == ProgressivePoolCreation.Default)
            {
                Assert.IsTrue(_commandEvent.Command == Command.WinningCombination);
            }
            else
            {
                Assert.IsTrue(_commandEvent.Command == Command.CurrentProgressive);
            }
        }

        private BetHelpPageViewModel ConstructBetHelpPageViewModel(
            bool nullEventBus = false,
            bool nullPropertiesManager = false,
            bool nullProtocolLinkedProgressiveAdapter = false)
        {
            return new BetHelpPageViewModel(
                nullEventBus ? null : _eventBus.Object,
                nullPropertiesManager ? null : _propertiesManager.Object,
                nullProtocolLinkedProgressiveAdapter ? null : _protocolLinkedProgressiveAdapter.Object);
        }

        private void CreateProgressiveLevels(int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName, int wager, long currentAmount = 0,
            LevelCreationType levelCreationType = LevelCreationType.Default)
        {
            for (var i = 0; i < levelsToCreate; ++i)
            {
                var assignedProgressiveKey = $"{protocolName}, " +
                                             $"Level Id: {linkedLevelId + i}, " +
                                             $"Progressive Group Id: {linkedProgressiveGroupId + i}";

                _progressiveLevels.Add(new ProgressiveLevel
                {
                    ProgressiveId = progId + i,
                    LevelId = progLevelId + i,
                    AssignedProgressiveId = new AssignableProgressiveId(AssignableProgressiveType.Linked,
                        assignedProgressiveKey),
                    WagerCredits = wager,
                    ResetValue = (i + 1) * 10000,
                    CurrentValue = currentAmount == 0 ? 100 : currentAmount,
                    CreationType = levelCreationType
                });
            }
        }

        private void Handle(object sender, HHRCommandEventArgs commandEvent)
        {
            _commandEvent = commandEvent;
        }

        private void SetupProgressivePoolCreationType(ProgressivePoolCreation type = ProgressivePoolCreation.Default)
        {
            _propertiesManager.Setup(x =>
                    x.GetProperty(GamingConstants.ProgressivePoolCreationType, ProgressivePoolCreation.Default))
                .Returns(type);

            _protocolLinkedProgressiveAdapter.Setup(x => x.GetActiveProgressiveLevels()).Returns(_progressiveLevels);
        }
    }
}

