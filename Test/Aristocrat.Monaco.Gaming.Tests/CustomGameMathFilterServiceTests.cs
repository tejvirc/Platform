namespace Aristocrat.Monaco.Gaming.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PackageManifest.Models;

    [TestClass]
    public class CustomGameMathFilterServiceTests
    {
        private static readonly JurisdictionRestriction EmptyJur = new()
        {
            JurisdictionId = "emptyJur",
            AllowedBetLinePresets = new List<int>(),
            AllowedDenoms = new List<long>(),
            AllowedVariationIds = new List<string>()
        };

        private static readonly JurisdictionRestriction NoCommon = new()
        {
            JurisdictionId = "noCommon",
            AllowedBetLinePresets = new List<int> { 99 },
            AllowedDenoms = new List<long> { 1 },
            AllowedVariationIds = new List<string> { "99" }
        };

        private static readonly JurisdictionRestriction SomeRestriction = new()
        {
            JurisdictionId = "someRestriction",
            AllowedBetLinePresets = new List<int> { 1 },
            AllowedDenoms = new List<long> { 1000, 5000 },
            AllowedVariationIds = new List<string> { "01" }
        };

        private static readonly JurisdictionRestriction AllAllowed = new()
        {
            JurisdictionId = "allAllowed",
            AllowedBetLinePresets = new List<int> { 1, 2, 3 },
            AllowedDenoms = new List<long> { 1000, 2000, 5000 },
            AllowedVariationIds = new List<string> { "01", "99" }
        };

        private CustomGameMathFilterService _target;
        private GameContent _gameContent;

        private Mock<IPropertiesManager> _properties;

        public static IEnumerable<object[]> Restrictions
        {
            get
            {
                return new[]
                {
                    new object[] { EmptyJur },
                    new object[] { NoCommon },
                    new object[] { SomeRestriction },
                    new object[] { AllAllowed }
                };
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _gameContent = new GameContent();
            _target = new CustomGameMathFilterService(_properties.Object);
            SetupGameContent();
        }

        [DataTestMethod]
        [DynamicData(nameof(Restrictions))]
        public void FilterTest(JurisdictionRestriction restriction)
        {
            _properties.Setup(p => p.GetProperty(ApplicationConstants.JurisdictionId, It.IsAny<string>()))
                .Returns(restriction.JurisdictionId);
            _target.Initialize();
            _target.Filter(ref _gameContent);
            Assert.AreEqual(restriction.AllowedVariationIds.Count(), _gameContent.GameAttributes.Count());
            foreach (var attributes in _gameContent.GameAttributes)
            {
                var denoms = attributes.Denominations.ToList();
                var expectedDenom = restriction.AllowedDenoms.ToList();
                foreach (var denom in denoms)
                {
                    Assert.IsTrue(expectedDenom.Contains(denom));
                }

                var betLinePresets = attributes.BetLinePresetList.ToList();
                var expectedBetLinePresets = restriction.AllowedBetLinePresets.ToList();
                foreach (var betLine in betLinePresets)
                {
                    Assert.IsTrue(expectedBetLinePresets.Contains(betLine.Id));
                }
            }
        }

        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataRow(true)]
        public void WhenArgumentIsNullExpectException(bool nullProperties)
        {
            CreateTarget(nullProperties);
        }

        private void SetupGameContent()
        {
            var attribute1 = new GameAttributes
            {
                PaytableId = "TestPaytableId1",
                Denominations = new List<long> { 1000, 2000, 5000 },
                VariationId = "01",
                BetLinePresetList = new BetLinePresetList(
                    new[]
                    {
                        new BetLinePreset
                        {
                            Id = 1,
                            BetOption = new BetOption
                            {
                                Name = "1 to 10",
                                Description = "10 credits multiplier",
                                Bets = new Bet[] { new() },
                                BonusBets = new[] { 1 }
                            },
                            LineOption = new LineOption
                            {
                                Name = "20 lines", Description = "20 lines", Lines = new Line[] { new() }
                            }
                        },
                        new BetLinePreset
                        {
                            Id = 2,
                            BetOption = new BetOption
                            {
                                Name = "1 to 15",
                                Description = "15 credits multiplier",
                                Bets = new Bet[] { new() },
                                BonusBets = new[] { 1 }
                            },
                            LineOption = new LineOption
                            {
                                Name = "20 lines", Description = "20 lines", Lines = new Line[] { new() }
                            }
                        },
                        new BetLinePreset
                        {
                            Id = 3,
                            BetOption = new BetOption
                            {
                                Name = "1 to 20",
                                Description = "20 credits multiplier",
                                Bets = new Bet[] { new() },
                                BonusBets = new[] { 1 }
                            },
                            LineOption = new LineOption
                            {
                                Name = "20 lines", Description = "20 lines", Lines = new Line[] { new() }
                            }
                        }
                    })
            };
            var attributes2 = new GameAttributes
            {
                PaytableId = "TestPaytableId2",
                Denominations = new List<long> { 1000, 2000 },
                VariationId = "99",
                BetLinePresetList = new BetLinePresetList(
                    new[]
                    {
                        new BetLinePreset
                        {
                            Id = 2,
                            BetOption = new BetOption
                            {
                                Name = "1 to 5",
                                Description = "5 credits multiplier",
                                Bets = new Bet[] { new() },
                                BonusBets = new[] { 1 }
                            },
                            LineOption = new LineOption
                            {
                                Name = "10 lines", Description = "10 lines", Lines = new Line[] { new() }
                            }
                        }
                    })
            };

            _gameContent.JurisdictionRestrictions =
                new List<JurisdictionRestriction> { EmptyJur, SomeRestriction, AllAllowed, NoCommon };
            _gameContent.GameAttributes = new List<GameAttributes> { attribute1, attributes2 };
            _gameContent.ReleaseNumber = "1";
        }

        private void CreateTarget(bool nullProperties = false)
        {
            _target = new CustomGameMathFilterService(
                nullProperties ? null : _properties.Object);

            _target.Initialize();
        }
    }
}