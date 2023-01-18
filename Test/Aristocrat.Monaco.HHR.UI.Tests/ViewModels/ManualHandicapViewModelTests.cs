namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using System;
    using System.Windows;
    using System.Collections.Generic;
    using System.Linq;
    using Client.Data;
    using Hardware.Contracts.Button;
    using Hhr.Events;
    using Hhr.Services;
    using Hhr.Storage.Helpers;
    using Gaming.Contracts;
    using Kernel;
    using Test.Common;
    using UI.Menu;
    using UI.Controls;
    using UI.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ManualHandicapViewModelTests
    {
        private ManualHandicapPageViewModel _target;

        private Mock<IPrizeDeterminationService> _prizeDeterminationService;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISystemDisableManager> _systemDisable;
        private Mock<IEventBus> _eventBus;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IServiceManager> _serviceManagerMock;
        private Mock<IManualHandicapEntityHelper> _manualHandicapEntityHelper;

        [TestInitialize]
        public void TestInitialization()
        {
            _serviceManagerMock = MoqServiceManager.CreateInstance(MockBehavior.Default);
            _prizeDeterminationService = new Mock<IPrizeDeterminationService>(MockBehavior.Default);
            _manualHandicapEntityHelper = new Mock<IManualHandicapEntityHelper>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _systemDisable = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

            _propertiesManager.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>())).Returns(It.IsAny<int>());
            UiProperties.ManualHandicapRemainingTime = 200;

            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            var downEvent = new DownEvent((int)ButtonLogicalId.Play);
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<ManualHandicapAbortedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Publish(downEvent)).Verifiable();

            ClientProperties.ManualHandicapTimeOut = 200;
            ClientProperties.ManualHandicapMode = Client.Messages.HhrConstants.QuickPickMode;

            if (Application.Current == null)
            {
                new Application();
            }

            _target = new ManualHandicapPageViewModel(
                _propertiesManager.Object,
                _eventBus.Object,
                _systemDisable.Object,
                _prizeDeterminationService.Object,
                _manualHandicapEntityHelper.Object,
                _gameProvider.Object);

            _target.Init(Command.ManualHandicap);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        public ManualHandicapHorseNumber HorseNumber(int num) => new ManualHandicapHorseNumber { HorseNumber = num };

        [RequireSTA]
        [TestMethod]
        public void RacesToHandicap_ManualPicking_ResultingStringMatchesExpectedValue()
        {
            var manualPicks = new List<int[]>
            {
                new int[] {1,2,3,4,5,6},
                new int[] {1,9,2,8,3,7,4,6},
                new int[] {3,6,9,12,11,10,4,5,7,8,1,2}
            };

            var expectedValues = new List<string>
            {
                "123456",
                "19283746",
                "369CBA45"
            };
            // Expected:<12345636>. Actual:<369CBA45>.
            var myRaceInfo = new CRaceInfo
            {
                RaceData = new CRaceData[]
                {
                    new CRaceData { Runners = 6 },
                    new CRaceData { Runners = 8 },
                    new CRaceData { Runners = 12 }
                }
            };

            _target.SetData(myRaceInfo);
            Assert.AreEqual(myRaceInfo.RaceData.Length, _target.TotalRaces);

            for (int i = manualPicks.Count; i > 0; i--)
            {
                Assert.AreEqual(i, _target.RemainingRacesToHandicap);

                var horsePicks = manualPicks[i - 1];
                foreach (var pick in horsePicks)
                {
                    _target.HorseNumberClicked.Execute(HorseNumber(pick));
                }

                if (i > 1) // If not the last race, press the Next Race button
                {
                    HhrPageCommand nextRaceCommand = _target.Commands.First(c => c.Command == Command.NextRace);
                    Assert.IsNotNull(nextRaceCommand);

                    nextRaceCommand.Execute(Command.NextRace);
                }
            }

            // All manual picks completed, now press the Race button
            HhrPageCommand raceCommand = _target.Commands.First(c => c.Command == Command.Race);
            Assert.IsNotNull(raceCommand);

            raceCommand.Execute(Command.Race);

            var picks = _target.GetPersistedPicks();
            Assert.IsNotNull(picks);
            Assert.AreEqual(picks.Count, manualPicks.Count);

            expectedValues.Reverse();
            for (int i = 0; i < manualPicks.Count; i++)
            {
                Assert.AreEqual(picks[i], expectedValues[i]);
            }
        }

        [TestMethod]
        public void RacesToHandicap_QuickPicking_ReturnsValidResult()
        {
            var manualPicks = new List<int[]>
            {
                new int[] {1,2,3,4,5,6},
                new int[] {1,9,2,8,3,7,4,6},
                new int[] {3,6,9,12,11,10,4,5,7,8,1,2}
            };

            var myRaceInfo = new CRaceInfo
            {
                RaceData = new CRaceData[]
                {
                    new CRaceData { Runners = 6 },
                    new CRaceData { Runners = 8 },
                    new CRaceData { Runners = 12 }
                }
            };

            _target.SetData(myRaceInfo);

            Assert.AreEqual(myRaceInfo.RaceData.Length, _target.TotalRaces);

            // Press the Quick Pick button
            HhrPageCommand quickPickCommand = _target.Commands.First(c => c.Command == Command.QuickPick);
            Assert.IsNotNull(quickPickCommand);

            quickPickCommand.Execute(Command.QuickPick);

            var picks = _target.GetPersistedPicks();
            Assert.IsNotNull(picks);
            Assert.AreEqual(manualPicks.Count, picks.Count);

            for (int i = 0; i < manualPicks.Count; i++)
            {
                // There may be up to 12 horses to pick from, but only 8 can be placed and represented in the result
                Assert.AreEqual(Math.Min(myRaceInfo.RaceData[i].Runners, HhrUiConstants.NumberOfHorsePickPositions), (uint)picks[i].Length);
            }
        }

        [TestMethod]
        public void RacesToHandicap_AutoPicking_ReturnsEmptyResult()
        {
            ClientProperties.ManualHandicapMode = Client.Messages.HhrConstants.AutoPickMode;
            _target.Init(Command.ManualHandicap);

            var manualPicks = new List<int[]>
            {
                new int[] {1,2,3,4,5,6},
                new int[] {1,9,2,8,3,7,4,6},
                new int[] {3,6,9,12,11,10,4,5,7,8,1,2}
            };

            var myRaceInfo = new CRaceInfo
            {
                RaceData = new CRaceData[]
                {
                    new CRaceData { Runners = 6 },
                    new CRaceData { Runners = 8 },
                    new CRaceData { Runners = 12 }
                }
            };

            _target.SetData(myRaceInfo);

            Assert.AreEqual(myRaceInfo.RaceData.Length, _target.TotalRaces);

            // Press the Auto Pick button
            HhrPageCommand autoPickCommand = _target.Commands.First(c => c.Command == Command.AutoPick);
            Assert.IsNotNull(autoPickCommand);

            autoPickCommand.Execute(Command.AutoPick);

            _prizeDeterminationService.Verify(o => o.ClearManualHandicapData(), Times.Once);

            var picks = _target.GetPersistedPicks();
            Assert.IsNotNull(picks);
            Assert.AreEqual(manualPicks.Count, picks.Count);

            for (int i = 0; i < manualPicks.Count; i++)
            {
                // There shouldn't be any horses picked
                Assert.AreEqual(0u, (uint)picks[i].Length);
            }
        }

        [TestMethod]
        public void ManualHandicapPageLoaded_Init_VerifyPageTimeout()
        {
            Assert.AreEqual(ClientProperties.ManualHandicapTimeOut, _target.TimerInfo.Timeout);
        }
    }
}
