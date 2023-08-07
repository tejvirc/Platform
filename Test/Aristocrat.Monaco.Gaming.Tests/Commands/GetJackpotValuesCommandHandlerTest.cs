using Aristocrat.Monaco.Application.Contracts.Extensions;
using Aristocrat.Monaco.Gaming.Commands;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.Progressives;
using Aristocrat.Monaco.Gaming.Progressives;
using Aristocrat.Monaco.PackageManifest.Models;
using Aristocrat.PackageManifest.Extension.v100;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace Aristocrat.Monaco.Gaming.Tests.Commands
{

    [TestClass]
    public class GetJackpotValuesCommandHandlerTest
    {
        private Mock<IProgressiveLevelProvider> _progressiveLevelProvider;
        private Mock<IGameProvider> _progressiveGameProvider;
        private Mock<IProgressiveGameProvider> _progressiveGame;
        private Mock<IGameDiagnostics> _gameDiagnostics;
        private GetJackpotValuesCommandHandler _target;

        // Line option is not important, we only need 1
        private static readonly LineOptionList _standardLineOptionList = new LineOptionList(
                        new List<c_lineOption>
                        {
                            new c_lineOption()
                            {
                                name = "2 to 123",
                                line = new c_line[]
                                {
                                    new c_line()
                                    {
                                        button = "buttonTest",
                                        buttonName = "button",
                                        cost = 1,
                                        costMultiplier = 1
                                    }
                                }
                            },
                        });
        // Generate betOptions
        private static readonly BetOptionList _standardBetOptionList = GenerateBetOptionList("1 to 5");
        private static readonly BetOptionList _secondBetOptionList = GenerateBetOptionList("1 to 10");
        private static readonly BetOptionList _thirdBetOptionList = GenerateBetOptionList("1 to 20");

        // Test BetLinePresets for different betOptions. Some Progressives Share
        private static readonly BetLinePresetList _testBetLinePreset = new BetLinePresetList(
                    new c_betLinePreset[]
                    {
                        new c_betLinePreset() {
                            id = 0,
                            betOption = "1 to 5",
                            lineOption = "2 to 123",
                        }
                    }, _standardBetOptionList, _standardLineOptionList);

        private static readonly BetLinePresetList _secondTestBetLinePreset = new BetLinePresetList(
                    new c_betLinePreset[]
                    {
                        new c_betLinePreset() {
                            id = 0,
                            betOption = "1 to 10",
                            lineOption = "2 to 123",
                        }
                    }, _secondBetOptionList, _standardLineOptionList);

        private static readonly BetLinePresetList _thirdTestBetLinePreset = new BetLinePresetList(
                    new c_betLinePreset[]
                    {
                        new c_betLinePreset() {
                            id = 0,
                            betOption = "1 to 20",
                            lineOption = "2 to 123",
                        }
                    }, _thirdBetOptionList, _standardLineOptionList);

        // Setup Enabled Games. and Their BetLinePresets used to filter the progressive levels
        private readonly IReadOnlyCollection<IGameDetail> _enabledGames = new List<IGameDetail>()
        {
            new GameDetail()
            {
                ThemeName = "Big Fortune",
                BetLinePresetList = _testBetLinePreset,
                Id = 1,
                ActiveBetOption = new BetOption()
                {
                    Name = "1 to 5"
                },
                ActiveLineOption = new LineOption()
                {
                    Name = "2 to 123"
                }
            },
            new GameDetail()
            {
                ThemeName = "Big Fortune",
                BetLinePresetList = _secondTestBetLinePreset,
                Id = 1,
                ActiveBetOption = new BetOption()
                {
                    Name = "1 to 10"
                },
                ActiveLineOption = new LineOption()
                {
                    Name = "2 to 123"
                }
            },
            new GameDetail()
            {
                ThemeName = "Big Fortune_2",
                BetLinePresetList = _testBetLinePreset,
                Id = 2,
                ActiveBetOption = new BetOption()
                {
                    Name = "1 to 5"
                },
                ActiveLineOption = new LineOption()
                {
                    Name = "2 to 123"
                }
            },
            new GameDetail()
            {
                ThemeName = "Big Fortune_3",
                BetLinePresetList = _testBetLinePreset,
                Id = 3,
                ActiveBetOption = new BetOption()
                {
                    Name = "1 to 5"
                },
                ActiveLineOption = new LineOption()
                {
                    Name = "2 to 123"
                }
            },
        };

        // Define Progressive levels for Bet Options and Denoms
        private readonly IReadOnlyCollection<ProgressiveLevel> _progressiveLevels = new List<ProgressiveLevel>()
        {
            new ProgressiveLevel()
            {
                GameId = 1,
                LevelId = 1,
                Denomination = new List<long>() { 1000L },
                HasAssociatedBetLinePreset = true,
                BetOption = "1 to 5",
                ProgressivePackName = "BF_SAP",
                CurrentValue = 100000L
            },
            new ProgressiveLevel()
            {
                GameId = 1,
                LevelId = 2,
                Denomination = new List<long>() { 1000L },
                HasAssociatedBetLinePreset = true,
                BetOption = "1 to 10",
                ProgressivePackName = "BF_SAP",
                CurrentValue = 440000L
            },
            new ProgressiveLevel()
            {
                GameId = 1,
                LevelId = 3,
                Denomination = new List<long>() { 2000L },
                HasAssociatedBetLinePreset = true,
                BetOption = "1 to 5",
                ProgressivePackName = "BF_SAP",
                CurrentValue = 100000L
            },
            new ProgressiveLevel()
            {
                GameId = 2,
                LevelId = 4,
                Denomination = new List<long>() { 1000L },
                HasAssociatedBetLinePreset = true,
                BetOption = "1 to 5",
                ProgressivePackName = "BF_SAP_2",
                CurrentValue = 200000L
            },
            new ProgressiveLevel()
            {
                GameId = 3,
                LevelId = 5,
                Denomination = new List<long>() { 1000L },
                HasAssociatedBetLinePreset = true,
                BetOption = "1 to 5",
                ProgressivePackName = "BF_SAP_3",
                CurrentValue = 300000L
            }
        };

        /// <summary>

        [TestInitialize]
        public void Initialize()
        {
            _progressiveLevelProvider = new Mock<IProgressiveLevelProvider>();
            _progressiveGameProvider = new Mock<IGameProvider>();
            _progressiveGame = new Mock<IProgressiveGameProvider>();
            _gameDiagnostics = new Mock<IGameDiagnostics>();

            SetupGames();
            SetupProgressiveLevels();
            _gameDiagnostics.Setup(x => x.IsActive).Returns(false);

            _target = new GetJackpotValuesCommandHandler(_gameDiagnostics.Object, _progressiveGame.Object, _progressiveGameProvider.Object, _progressiveLevelProvider.Object);
        }

        [DataRow("Big Fortune", 1, "BF_SAP", 1UL)]
        [DataRow("Big Fortune", 1, "BF_SAP", 2UL)]
        [DataRow("Big Fortune_2", 2, "BF_SAP_2", 1UL)]
        [DataRow("Big Fortune_3", 3, "BF_SAP_3", 1UL)]
        [DataTestMethod]
        public void GetJackpotCommandHandlerAllParameters(string gameName, int gameId, string packName, ulong denom)
        {
            var info = new GetJackpotValues(packName, false, gameName, denom);
            _target.Handle(info);

            // Should return All Jackpot Values with PackName, GameName and Denom
            Assert.IsTrue(info.JackpotValues.Count == _progressiveLevels.Where(x => x.ProgressivePackName == packName &&
                                                                                    x.GameId == gameId &&
                                                                                    x.Denomination.Contains(((long)denom).CentsToMillicents())).Count()) ;
            
        }

        [DataRow("BF_SAP")]
        [DataRow("BF_SAP")]
        [DataRow( "BF_SAP_2")]
        [DataRow("BF_SAP_3")]
        [DataTestMethod]
        public void GetJackPotCommandHandlerJustPackName(string packName)
        {
            // Setup for GetAllJackpotValues for PackName
            _progressiveGame.Setup(x => x.GetProgressiveLevel(It.IsAny<string>(), It.IsAny<bool>())).Returns(_progressiveLevels.Where(x => x.ProgressivePackName == packName).ToDictionary(x => x.LevelId, x => x.CurrentValue));

            var info = new GetJackpotValues(packName, true);
            _target.Handle(info);

            // Should return all Jackpot Values with the PackName Only.
            Assert.IsTrue(info.JackpotValues.Count == _progressiveLevels.Where(x => x.ProgressivePackName == packName).Count());
        }

        private void SetupGames()
        {
            _progressiveGameProvider.Setup(gameProvider => gameProvider.GetEnabledGames()).Returns(_enabledGames);
        }

        private void SetupProgressiveLevels()
        {
            _progressiveLevelProvider.Setup(progressiveLevelProvider => progressiveLevelProvider.GetProgressiveLevels()).Returns(_progressiveLevels);
        }

        private static BetOptionList GenerateBetOptionList(string betOption)
        {
            return new BetOptionList (new List<c_betOption>
            {
                new c_betOption()
                {
                    name = betOption,
                    bet = new c_bet[]
                    {
                        new c_bet()
                        {
                            button = "buttonTest",
                            buttonName = "button",
                            multiplier = 1,
                        }
                    }
                },
            },
                new c_betLinePreset[]
                {
                    new c_betLinePreset() {
                        id = 0,
                        betOption = betOption,
                        lineOption = "2 to 123",
                    }
                });
        }
    }
}