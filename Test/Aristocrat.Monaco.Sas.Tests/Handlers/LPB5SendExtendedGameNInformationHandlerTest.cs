namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Progressive;
    using Sas.Handlers;

    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class LPB5SendExtendedGameNInformationHandlerTest
    {
        private const int MachineMaxBet = 10_000;
        private const string MachineGameName = "";
        private const string MachinePaytableName = "";

        private static readonly IReadOnlyCollection<IWagerCategory> WagerCategories = new List<IWagerCategory>
        {
            new WagerCategory("WC1", 9876, 1, 5, 100_000),
            new WagerCategory("WC2", 97656, 5, 10, 200_000),
            new WagerCategory("WC3", 9654, 25, 100, 300_000),
            new WagerCategory("WC4", 9543, 50, 150, 400_000),
            new WagerCategory("WC5", 9432, 75, 300, 500_000),
            new WagerCategory("WC6", 9321, 100, 500, 600_000),
            new WagerCategory("WC7", 9210, 250, 1_000, 700_000),
            new WagerCategory("WC8", 91098, 500, 2_500, 800_000),
            new WagerCategory("WC9", 9098, 750, 3_750, 900_000),
            new WagerCategory("WC10", 8987, 1000, 5_000, 1_000_000)
        };

        private static readonly List<IDenomination> StandardDenominations = new List<IDenomination>
        {
            new MockDenomination(1_000, 1),
            new MockDenomination(5_000, 2),
            new MockDenomination(10_000, 3),
            new MockDenomination(25_000, 4),
            new MockDenomination(50_000, 5),
            new MockDenomination(1_00_000, 6),
            new MockDenomination(5_00_000, 7),
            new MockDenomination(10_00_000, 8),
            new MockDenomination(20_00_000, 9),
            new MockDenomination(100_00_000, 10),
            new MockDenomination(20_000, 11),
            new MockDenomination(2_00_000, 13),
            new MockDenomination(2_50_000, 14),
            new MockDenomination(50_00_000, 15),
            new MockDenomination(200_00_000, 16),
            new MockDenomination(250_00_000, 17),
            new MockDenomination(500_00_000, 18),
            new MockDenomination(1_000_00_000, 19),
            new MockDenomination(2_000_00_000, 20),
            new MockDenomination(2_500_00_000, 21),
            new MockDenomination(5_000_00_000, 22),
            new MockDenomination(2_000, 23),
            new MockDenomination(3_000, 24),
            new MockDenomination(15_000, 25),
            new MockDenomination(40_000, 26),
            new MockDenomination(0_000_000, 27),
            new MockDenomination(7_000_000, 28)
        };

        private static readonly List<byte> ExpectedStandardDenomCodes = new List<byte>
        {
            0x1,
            0x2,
            0x3,
            0x4,
            0x5,
            0x6,
            0x7,
            0x8,
            0x9,
            0xA,
            0xB,
            0xC,
            0xD,
            0xF,
            0x10,
            0x11,
            0x12,
            0x13,
            0x14,
            0x15,
            0x16,
            0x17,
            0x18,
            0x19,
            0x1A,
            0x00,
            0x00
        };

        private static readonly IReadOnlyCollection<IGameDetail> GameDataCollection = new List<IGameDetail>
        {
            new TestGameProfile
            {
                Id = 1,
                MaximumWagerCredits = 5,
                ThemeName = "Game1",
                PaytableName = "PayTable01",
                WagerCategories = WagerCategories.Take(3),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations
            },
            new TestGameProfile
            {
                Id = 2,
                MaximumWagerCredits = 5,
                ThemeName = "Game2",
                PaytableName = "PayTable02",
                WagerCategories = WagerCategories.Take(4),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations =
                    StandardDenominations.Select(x => new MockDenomination(x.Value, StandardDenominations.Count + x.Id))
            },
            new TestGameProfile
            {
                Id = 3,
                MaximumWagerCredits = 150,
                ThemeName = "Game3",
                PaytableName = "PayTable03",
                WagerCategories = WagerCategories.Take(5),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations.Select(
                    x => new MockDenomination(x.Value, (long)StandardDenominations.Count * 2 + x.Id))
            },
            new TestGameProfile
            {
                Id = 4,
                MaximumWagerCredits = 100,
                ThemeName = "Game4",
                PaytableName = "PayTable04",
                WagerCategories = WagerCategories.Take(1),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations.Select(
                    x => new MockDenomination(x.Value, (long)(StandardDenominations.Count + 1) * 3 + x.Id))
            },
            new TestGameProfile
            {
                Id = 5,
                MaximumWagerCredits = 10_000,
                ThemeName = "GameThemeName05",
                PaytableName = "PayTableLongName05",
                WagerCategories = WagerCategories.Take(8),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations.Select(
                    x => new MockDenomination(x.Value, (long)(StandardDenominations.Count + 2) * 4 + x.Id))
            },
            new TestGameProfile
            {
                Id = 6,
                MaximumWagerCredits = 2_500,
                ThemeName = "GameThemeName06",
                PaytableName = "PayTableLongName06",
                WagerCategories = WagerCategories.Take(10),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations.Select(
                    x => new MockDenomination(x.Value, (long)(StandardDenominations.Count + 3) * 5 + x.Id))
            },
            new TestGameProfile
            {
                Id = 7,
                MaximumWagerCredits = 10,
                ThemeName = "GameThemeName07",
                PaytableName = "PayTableLongName07",
                WagerCategories = WagerCategories.Take(7),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations.Select(
                    x => new MockDenomination(x.Value, (long)(StandardDenominations.Count + 4) * 6 + x.Id))
            },
            new TestGameProfile
            {
                Id = 8,
                MaximumWagerCredits = 300,
                ThemeName = "GameThemeName08",
                PaytableName = "PayTableLongName08",
                WagerCategories = WagerCategories.Take(9),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations.Select(
                    x => new MockDenomination(x.Value, (long)(StandardDenominations.Count + 5) * 7 + x.Id))
            }
        };

        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private LPB5SendExtendedGameNInformationHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);

            _target = CreateHandler();

            SetupMockGameProvider(GameDataCollection);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendExtendedGameNInformation));
        }

        [DataTestMethod]
        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullPropertiesProvider,
            bool nullGameProvider,
            bool nullLinkedProgressive)
        {
            _target = CreateHandler(
                nullPropertiesProvider,
                nullGameProvider,
                nullLinkedProgressive);
        }

        [DataTestMethod]
        [DataRow(0, 0, 0, 0L, (byte)0, DisplayName = "Machine extended information with 0 as progressive Group")]
        [DataRow(0, 5, 0, 0L, (byte)0, DisplayName = "Machine extended information with 5 as progressive Group")]
        [DataRow(
            1,
            0,
            0x1C,
            1000L,
            (byte)0x01,
            DisplayName = "Game 1 found extended game information with 0 as progressive Group")]
        [DataRow(
            2,
            1,
            0x2F,
            5000L,
            (byte)0x02,
            DisplayName = "Game 2 found extended game information with 9 as progressive Group")]
        [DataRow(
            5,
            3,
            0x3F,
            1000L,
            (byte)0x01,
            DisplayName =
                "Game 5 found extended game information with 3 as progressive Group, and retains too large of maxbet, game name, paytable name")]
        [DataRow(9, 4, 1, 1000L, (byte)0x01, DisplayName = "Game 9 not found extended game information, returns null")]
        public void HandleTest(int gameId, int progGroup, int progLevels, long expectedDenom, byte denomCode)
        {
            var game = GameDataCollection.SingleOrDefault(g => g.Id == gameId);

            if (gameId != 0 && game == null)
            {
                _gameProvider.Setup(g => g.GetAllGames()).Returns(new List<IGameDetail>());
                Assert.AreSame(
                    _target.Handle(new LongPollExtendedGameNInformationData { GameNumber = (uint)gameId }),
                    null);
            }
            else
            {
                _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                    .Returns(new SasFeatures { ProgressiveGroupId = progGroup });
                SetupProgressiveLevels(progLevels, gameId, expectedDenom);

                var denomination = game?.Denominations.FirstOrDefault(x => x.Value == expectedDenom);
                var result = _target.Handle(
                    new LongPollExtendedGameNInformationData { GameNumber = (uint)(denomination?.Id ?? gameId) });
                var expectedProgressiveGroupId = progLevels > 0 ? progGroup : 0;

                if (gameId == 0)
                {
                    Assert.AreEqual(MachineMaxBet, result.MaxBet);
                    Assert.AreEqual((byte)expectedProgressiveGroupId, result.ProgressiveGroup);
                    Assert.AreEqual((uint)progLevels, result.ProgressiveLevels);
                    Assert.AreEqual(MachineGameName, result.GameName);
                    Assert.AreEqual(MachinePaytableName, result.PaytableName);
                    Assert.AreEqual(0, result.NumberOfWagerCategories);
                    CollectionAssert.AreEqual(ExpectedStandardDenomCodes, result.Denominations.ToList());
                }
                else
                {
                    Assert.IsNotNull(game);
                    Assert.IsNotNull(denomination);
                    Assert.AreEqual(game.MaximumWagerCredits, result.MaxBet);
                    Assert.AreEqual((byte)expectedProgressiveGroupId, result.ProgressiveGroup);
                    Assert.AreEqual((uint)progLevels, result.ProgressiveLevels);
                    Assert.AreEqual($"{game.ThemeName}-{expectedDenom.MillicentsToDollars()}", result.GameName);
                    Assert.AreEqual($"{game.PaytableName}_{expectedDenom.MillicentsToDollars()}", result.PaytableName);
                    Assert.AreEqual(game.WagerCategories.Count(), result.NumberOfWagerCategories);
                    CollectionAssert.AreEqual(new List<byte> { denomCode }, result.Denominations.ToList());
                }
            }
        }

        [TestMethod]
        public void NoActiveGamesTest()
        {
            var game = new TestGameProfile
            {
                Id = 1,
                MaximumWagerCredits = 5,
                ThemeName = "Game1",
                PaytableName = "PayTable01",
                WagerCategories = WagerCategories.Take(3),
                Denominations = new List<IDenomination>
                {
                    new MockDenomination(1000, 1, false),
                    new MockDenomination(5000, 2, false)
                },
                ActiveDenominations = new List<long>()
            };

            SetupMockGameProvider(new List<IGameDetail> { game });
            SetupProgressiveLevels(0, game.Id, 1);

            var result = _target.Handle(new LongPollExtendedGameNInformationData { GameNumber = 0 });

            Assert.AreEqual(0, result.MaxBet);
            Assert.AreEqual((byte)0, result.ProgressiveGroup);
            Assert.AreEqual((uint)0, result.ProgressiveLevels);
            Assert.AreEqual(string.Empty, result.GameName);
            Assert.AreEqual(string.Empty, result.PaytableName);
            Assert.AreEqual(0, result.NumberOfWagerCategories);
            CollectionAssert.AreEqual(new List<byte> { 1, 2 }, result.Denominations.ToList());
        }

        [TestMethod]
        public void NotActiveGamesTest()
        {
            const long denomId = 1;
            const long denomValue = 1000;

            var game = new TestGameProfile
            {
                Id = 1,
                MaximumWagerCredits = 5,
                ThemeName = "Game1",
                PaytableName = "PayTable01",
                WagerCategories = WagerCategories.Take(3),
                Denominations = new List<IDenomination> { new MockDenomination(denomValue, denomId, false) }
            };

            SetupMockGameProvider(new List<IGameDetail> { game });
            SetupProgressiveLevels(0, game.Id, denomId);

            var result = _target.Handle(
                new LongPollExtendedGameNInformationData { GameNumber = (uint)denomId });

            Assert.AreEqual(0, result.MaxBet);
            Assert.AreEqual((byte)0, result.ProgressiveGroup);
            Assert.AreEqual((uint)0, result.ProgressiveLevels);
            Assert.AreEqual($"{game.ThemeName}-{denomValue.MillicentsToDollars()}", result.GameName);
            Assert.AreEqual($"{game.PaytableName}_{denomValue.MillicentsToDollars()}", result.PaytableName);
            Assert.AreEqual(game.WagerCategories.Count(), result.NumberOfWagerCategories);
            CollectionAssert.AreEqual(new List<byte> { 1 }, result.Denominations.ToList());
        }

        [TestMethod]
        public void NullGameNameAndPaytableNameTest()
        {
            const int gameId = 100;
            const int maxWagerCredits = 300;
            const int progGroup = 3;
            const uint progLevels = 0; // This is 0 for now...

            var game = new TestGameProfile
            {
                Id = gameId,
                MaximumWagerCredits = maxWagerCredits,
                ThemeName = null,
                PaytableName = null,
                WagerCategories = WagerCategories.Take(9),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations
            };

            _gameProvider.Setup(x => x.GetAllGames()).Returns(new List<IGameDetail> { game });

            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                    .Returns(new SasFeatures { ProgressiveGroupId = progGroup });

            var denomination = StandardDenominations.First();
            var result = _target.Handle(
                new LongPollExtendedGameNInformationData { GameNumber = (uint)denomination.Id });

            Assert.IsNotNull(game);
            Assert.AreEqual(game.MaximumWagerCredits, result.MaxBet);
            Assert.AreEqual((byte)0, result.ProgressiveGroup);
            Assert.AreEqual(progLevels, result.ProgressiveLevels);
            Assert.AreEqual($"-{denomination.Value.MillicentsToDollars()}", result.GameName);
            Assert.AreEqual($"_{denomination.Value.MillicentsToDollars()}", result.PaytableName);
            Assert.AreEqual(game.WagerCategories.Count(), result.NumberOfWagerCategories);
            Assert.AreEqual(1, result.Denominations.Count);
            Assert.AreEqual((byte)0x01, result.Denominations.First());
        }

        [TestMethod]
        public void HandleDenomTest()
        {
            const uint gameNumber = 3;

            var denomShiftedGames = new List<IGameDetail>
            {
                new TestGameProfile
                {
                    Id = 3,
                    MaximumWagerCredits = 50,
                    ThemeName = "Game3",
                    PaytableName = "Paytable3",
                    WagerCategories = WagerCategories,
                    ActiveDenominations = new List<long> { 1_000, 5_000 },
                    Denominations = new List<IDenomination>
                    {
                        new MockDenomination(1_000, 3), new MockDenomination(5_000, 4)
                    }
                }
            };

            var game = denomShiftedGames.SingleOrDefault(g => g.Id == gameNumber);
            SetupMockGameProvider(denomShiftedGames);

            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                    .Returns(new SasFeatures { ProgressiveGroupId = 0 });

            var result = _target.Handle(
                new LongPollExtendedGameNInformationData { GameNumber = gameNumber, TargetDenomination = 1 });

            Assert.IsNotNull(game);
            Assert.AreEqual(game.MaximumWagerCredits, result.MaxBet);
            Assert.AreEqual((byte)0, result.ProgressiveGroup);
            Assert.AreEqual((uint)0, result.ProgressiveLevels);
            Assert.AreEqual($"{game.ThemeName}-0.01", result.GameName);
            Assert.AreEqual($"{game.PaytableName}_0.01", result.PaytableName);
            Assert.AreEqual(game.WagerCategories.Count(), result.NumberOfWagerCategories);
            CollectionAssert.AreEqual(new List<byte> { 0x1 }, result.Denominations.ToList());
        }

        [TestMethod]
        public void HandleDenomGameMachineInfoTest()
        {
            var denomShiftedGames = new List<IGameDetail>
            {
                new TestGameProfile
                {
                    Id = 1,
                    MaximumWagerCredits = 5,
                    ThemeName = "Game1",
                    PaytableName = "Paytable1",
                    WagerCategories = WagerCategories,
                    ActiveDenominations = new List<long> { 1_000, 5_000, 10_000 },
                    Denominations = new List<IDenomination>
                    {
                        new MockDenomination(1_000, 1), new MockDenomination(5_000, 2), new MockDenomination(10_000, 3)
                    }
                },
                new TestGameProfile
                {
                    Id = 2,
                    MaximumWagerCredits = 100,
                    ThemeName = "Game2",
                    PaytableName = "Paytable2",
                    WagerCategories = WagerCategories,
                    ActiveDenominations = new List<long> { 5_000 },
                    Denominations = new List<IDenomination> { new MockDenomination(5_000, 4) }
                },
                new TestGameProfile
                {
                    Id = 3,
                    MaximumWagerCredits = 50,
                    ThemeName = "Game3",
                    PaytableName = "Paytable3",
                    WagerCategories = WagerCategories,
                    ActiveDenominations = new List<long> { 1_000, 5_000 },
                    Denominations = new List<IDenomination>
                    {
                        new MockDenomination(1_000, 5), new MockDenomination(5_000, 6)
                    }
                }
            };

            SetupMockGameProvider(denomShiftedGames);
            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                    .Returns(new SasFeatures { ProgressiveGroupId = 1 });

            var result = _target.Handle(
                new LongPollExtendedGameNInformationData { TargetDenomination = 1, MultiDenomPoll = true });

            Assert.AreEqual(denomShiftedGames.ElementAt(2).MaximumWagerCredits, result.MaxBet);
            Assert.AreEqual((byte)0, result.ProgressiveGroup);
            Assert.AreEqual((uint)0, result.ProgressiveLevels);
            Assert.AreEqual(MachineGameName, result.GameName);
            Assert.AreEqual(MachinePaytableName, result.PaytableName);
            Assert.AreEqual(0, result.NumberOfWagerCategories);
            CollectionAssert.AreEqual(new List<byte> { 0x1 }, result.Denominations.ToList());
        }

        [TestMethod]
        public void LongThemeNameTest()
        {
            const string expectedName = "VeryLongGameNam-0.01";
            var game = new TestGameProfile
            {
                Id = 1,
                MaximumWagerCredits = 5,
                ThemeName = "VeryLongGameNameTest",
                PaytableName = "PayTable01",
                WagerCategories = WagerCategories.Take(3),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations
            };

            SetupMockGameProvider(new List<IGameDetail> { game });

            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                    .Returns(new SasFeatures { ProgressiveGroupId = 0 });

            var result = _target.Handle(
                new LongPollExtendedGameNInformationData { GameNumber = 1, TargetDenomination = 1 });

            Assert.IsNotNull(game);
            Assert.AreEqual(game.MaximumWagerCredits, result.MaxBet);
            Assert.AreEqual((byte)0, result.ProgressiveGroup);
            Assert.AreEqual((uint)0, result.ProgressiveLevels);
            Assert.AreEqual(expectedName, result.GameName);
            Assert.AreEqual($"{game.PaytableName}_0.01", result.PaytableName);
            Assert.AreEqual(game.WagerCategories.Count(), result.NumberOfWagerCategories);
            CollectionAssert.AreEqual(new List<byte> { 0x1 }, result.Denominations.ToList());
        }

        [TestMethod]
        public void NullThemeNameTest()
        {
            const string expectedName = "-0.01";
            var game = new TestGameProfile
            {
                Id = 1,
                MaximumWagerCredits = 5,
                ThemeName = null,
                PaytableName = "PayTable01",
                WagerCategories = WagerCategories.Take(3),
                ActiveDenominations = StandardDenominations.Select(d => d.Value),
                Denominations = StandardDenominations
            };

            SetupMockGameProvider(new List<IGameDetail> { game });

            _propertiesManager.Setup(c => c.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                    .Returns(new SasFeatures { ProgressiveGroupId = 0 });

            var result = _target.Handle(
                new LongPollExtendedGameNInformationData { GameNumber = 1, TargetDenomination = 1 });

            Assert.IsNotNull(game);
            Assert.AreEqual(game.MaximumWagerCredits, result.MaxBet);
            Assert.AreEqual((byte)0, result.ProgressiveGroup);
            Assert.AreEqual((uint)0, result.ProgressiveLevels);
            Assert.AreEqual(expectedName, result.GameName);
            Assert.AreEqual($"{game.PaytableName}_0.01", result.PaytableName);
            Assert.AreEqual(game.WagerCategories.Count(), result.NumberOfWagerCategories);
            CollectionAssert.AreEqual(new List<byte> { 0x1 }, result.Denominations.ToList());
        }

        private void SetupMockGameProvider(IReadOnlyCollection<IGameDetail> gameData)
        {
            _gameProvider.Setup(c => c.GetAllGames()).Returns(gameData);
        }

        private LPB5SendExtendedGameNInformationHandler CreateHandler(
            bool nullPropertiesProvider = false,
            bool nullGameProvider = false,
            bool nullLinkedProgressive = false)
        {
            return new LPB5SendExtendedGameNInformationHandler(
                nullPropertiesProvider ? null : _propertiesManager.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullLinkedProgressive ? null : _protocolLinkedProgressiveAdapter.Object);
        }

        private void SetupProgressiveLevels(int levels, int gameId, long denom)
        {
            var progressiveLevels = new List<Mock<IViewableProgressiveLevel>>();
            var levelId = 0;
            for (var i = 1; i <= levels; i <<= 1)
            {
                levelId++;
                if ((levels & i) == 0)
                {
                    continue;
                }

                var level = new Mock<IViewableProgressiveLevel>();

                var levelName = $"SasLevel{levelId}";
                level.Setup(x => x.AssignedProgressiveId).Returns(
                    new AssignableProgressiveId(AssignableProgressiveType.Linked, levelName));
                level.Setup(x => x.LevelName).Returns(levelName);
                if (gameId != 0)
                {
                    level.Setup(x => x.GameId).Returns(gameId);
                }

                if (denom != 0)
                {
                    level.Setup(x => x.Denomination).Returns(new List<long> { denom });
                }

                var linkLevel = new Mock<IViewableLinkedProgressiveLevel>();
                linkLevel.Setup(x => x.LevelId).Returns(levelId);
                linkLevel.Setup(x => x.ProtocolName).Returns(ProgressiveConstants.ProtocolName);

                var expectedLevel = linkLevel.Object;
                _protocolLinkedProgressiveAdapter.Setup(x => x.ViewLinkedProgressiveLevel(levelName, out expectedLevel))
                    .Returns(true);

                progressiveLevels.Add(level);
            }

            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels())
                .Returns(progressiveLevels.Select(x => x.Object));
            _protocolLinkedProgressiveAdapter.Setup(x => x.ViewConfiguredProgressiveLevels(gameId, denom))
                .Returns(progressiveLevels.Select(x => x.Object));
        }
    }
}