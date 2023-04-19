namespace Aristocrat.Monaco.Gaming.Tests.Progressives
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Progressives;
    using Gaming.Progressives;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PRNGLib;
    using Gaming.Contracts;

    [TestClass]
    public class MysteryProgressiveProviderTests
    {
        private static readonly AssignableProgressiveId _nonSharedSap = new(AssignableProgressiveType.None, null);

        private static readonly AssignableProgressiveId _sharedSap = new(AssignableProgressiveType.AssociativeSap, "1");

        private static readonly AssignableProgressiveId _customSap = new(AssignableProgressiveType.CustomSap, "2");

        private static readonly AssignableProgressiveId _linkedProg = new(AssignableProgressiveType.Linked, "3");

        // with default rng stub, every even (0, 2, ...) index should jackpot
        private readonly List<ProgressiveLevel> _testData = new()
        {
            new ProgressiveLevel
            {
                ResetValue = 1000,
                MaximumValue = 2000,
                CurrentValue = 1500,
                AssignedProgressiveId = _sharedSap
            },
            new ProgressiveLevel
            {
                ResetValue = 500,
                MaximumValue = 1000,
                CurrentValue = 749,
                AssignedProgressiveId = _sharedSap
            },
            new ProgressiveLevel
            {
                ResetValue = 300,
                MaximumValue = 1000,
                CurrentValue = 650,
                AssignedProgressiveId = _customSap
            },
            new ProgressiveLevel
            {
                ResetValue = 1,
                MaximumValue = 2,
                CurrentValue = 0,
                AssignedProgressiveId = _customSap
            },
            new ProgressiveLevel
            {
                ResetValue = 1,
                MaximumValue = 2,
                CurrentValue = 1,
                AssignedProgressiveId = _linkedProg
            },
            new ProgressiveLevel
            {
                ResetValue = 999,
                MaximumValue = 9999,
                CurrentValue = 5000,
                AssignedProgressiveId = _linkedProg
            },
            new ProgressiveLevel
            {
                ResetValue = 1,
                MaximumValue = 2,
                CurrentValue = 1,
                ProgressivePackName = "Pack1",
                ProgressivePackId = 1,
                LevelName = "Level1",
                Denomination = new List<long> { 1000 },
                AssignedProgressiveId = _nonSharedSap
            },
            new ProgressiveLevel
            {
                ResetValue = 999,
                MaximumValue = 9999,
                CurrentValue = 5000,
                ProgressivePackName = "Pack2",
                ProgressivePackId = 2,
                LevelName = "Level2",
                Denomination = new List<long> { 1000 },
                AssignedProgressiveId = _nonSharedSap
            },
        };

        private MysteryProgressiveProvider _mysteryProgressiveProvider;
        private Mock<IRandomFactory> _randomFactory;
        private Mock<IPRNG> _prng;
        private Mock<IPersistentBlock> _persistentBlock;
        private Mock<IPersistentTransaction> _persistentTransaction;
        private Mock<IPersistenceProvider> _persistenceProvider;

        [TestInitialize]
        public void Init()
        {
            _prng = new Mock<IPRNG>();
            _prng.Setup(x => x.GetValue(It.IsAny<ulong>()))
                 .Returns<ulong>(x => x / 2);

            _randomFactory = new Mock<IRandomFactory>();
            _randomFactory.Setup(x => x.Create(It.IsAny<RandomType>()))
                          .Returns(_prng.Object);

            _persistentTransaction = new Mock<IPersistentTransaction>();
            _persistentBlock = new Mock<IPersistentBlock>();
            _persistentBlock.Setup(x => x.GetOrCreateValue<ConcurrentDictionary<string, long>>(It.IsAny<string>()))
                            .Returns(new ConcurrentDictionary<string, long>());
            _persistentBlock.Setup(x => x.Transaction())
                            .Returns(_persistentTransaction.Object);

            _persistenceProvider = new Mock<IPersistenceProvider>();
            _persistenceProvider.Setup(x => x.GetOrCreateBlock(It.IsAny<string>(), PersistenceLevel.Static))
                                .Returns(_persistentBlock.Object);

            _mysteryProgressiveProvider = new MysteryProgressiveProvider(_randomFactory.Object, _persistenceProvider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void GenerateMagicNumbersTest()
        {
            foreach (var progressiveLevel in _testData)
            {
                var number = _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);

                Assert.IsNotNull(number);
                Assert.IsTrue(MagicNumberIsInProgressiveRange(progressiveLevel, number));
            }
        }

        [TestMethod]
        public void GenerateMagicNumbersMinTest()
        {
            _prng
                .Setup(x => x.GetValue(It.IsAny<ulong>()))
                .Returns<ulong>(x => 0);
            _randomFactory
                .Setup(x => x.Create(It.IsAny<RandomType>()))
                .Returns(_prng.Object);

            foreach (var progressiveLevel in _testData)
            {
                var number = _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);

                Assert.IsNotNull(number);
                Assert.IsTrue(MagicNumberIsInProgressiveRange(progressiveLevel, number));
                Assert.AreEqual(progressiveLevel.CurrentValue, number);
            }
        }

        [TestMethod]
        public void GenerateMagicNumbersMaxTest()
        {
            _prng
                .Setup(x => x.GetValue(It.IsAny<ulong>()))
                .Returns<ulong>(x => x);
            _randomFactory
                .Setup(x => x.Create(It.IsAny<RandomType>()))
                .Returns(_prng.Object);

            foreach (var progressiveLevel in _testData)
            {
                var number = _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);

                Assert.IsNotNull(number);
                Assert.IsTrue(MagicNumberIsInProgressiveRange(progressiveLevel, number));
                Assert.AreEqual(progressiveLevel.MaximumValue, number);
            }
        }

        [TestMethod]
        public void GetMagicNumbersTest()
        {
            foreach (var progressiveLevel in _testData)
            {
                var generatedNumber = _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);

                var status = _mysteryProgressiveProvider.TryGetMagicNumber(progressiveLevel, out var magicNumber);

                Assert.IsTrue(status);
                Assert.IsNotNull(magicNumber);
                Assert.IsNotNull(generatedNumber);
                Assert.AreEqual(generatedNumber, magicNumber);
            }
        }

        [TestMethod]
        public void CheckMysteryJackpotHitTest()
        {
            foreach (var progressiveLevel in _testData)
            {
                var magicNumber = _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);

                var shouldHit = progressiveLevel.CurrentValue >= magicNumber;

                var hit = _mysteryProgressiveProvider.CheckMysteryJackpot(progressiveLevel);

                if (shouldHit)
                {
                    Assert.IsTrue(hit);
                }
                else
                {
                    Assert.IsFalse(hit);
                }
            }
        }

        [TestMethod]
        public void GetSharedMagicNumbersTest()
        {
            var sharedGroups = _testData
                .Where(prog => prog.AssignedProgressiveId.AssignedProgressiveKey != null)
                .GroupBy(prog => prog.AssignedProgressiveId.AssignedProgressiveKey)
                .ToList();

            foreach (var group in sharedGroups)
            {
                foreach (var progressiveLevel in group)
                {
                    _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);
                }

                _mysteryProgressiveProvider.TryGetMagicNumber(group.First(), out var firstMagicNumber);

                var allShareMagicNumber = group.All(prog =>
                {
                    _mysteryProgressiveProvider.TryGetMagicNumber(prog, out var magicNumber);
                    return magicNumber == firstMagicNumber;
                });

                Assert.IsTrue(allShareMagicNumber);
            }
        }

        [TestMethod]
        public void GetUniqueMagicNumbersTest()
        {
            var progsWithDifferentIds = _testData
                .GroupBy(prog => prog.AssignedProgressiveId.AssignedProgressiveKey)
                .Select(g => g.First())
                .ToList();

            foreach (var progressiveLevel in progsWithDifferentIds)
            {
                _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);
            }

            _mysteryProgressiveProvider.TryGetMagicNumber(progsWithDifferentIds.First(), out var firstMagicNumber);
            progsWithDifferentIds.RemoveAt(0);

            var allUniqueNumbers = progsWithDifferentIds.All(prog =>
            {
                _mysteryProgressiveProvider.TryGetMagicNumber(prog, out var magicNumber);
                return magicNumber != firstMagicNumber;
            });

            Assert.IsTrue(allUniqueNumbers);
        }

        [TestMethod]
        public void StandaloneUniqueMagicNumbersTest()
        {
            var standaloneProgs = _testData.Where(prog => prog.AssignedProgressiveId.AssignedProgressiveKey == null).ToList();

            foreach (var progressiveLevel in standaloneProgs)
            {
                _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);
            }

            _mysteryProgressiveProvider.TryGetMagicNumber(standaloneProgs.First(), out var firstMagicNumber);
            standaloneProgs.RemoveAt(0);

            var allUniqueNumbers = standaloneProgs.All(prog =>
            {
                _mysteryProgressiveProvider.TryGetMagicNumber(prog, out var magicNumber);
                return magicNumber != firstMagicNumber;
            });

            Assert.IsTrue(allUniqueNumbers);
        }

        private bool MagicNumberIsInProgressiveRange(ProgressiveLevel progressiveLevel, decimal magicNumber) =>
            progressiveLevel.CurrentValue <= magicNumber && magicNumber <= progressiveLevel.MaximumValue;
    }
}
