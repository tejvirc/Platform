namespace Aristocrat.Monaco.Gaming.Tests.Progressives
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Gaming.Progressives;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PRNGLib;
    using Gaming.Contracts;

    [TestClass]
    public class MysteryProgressiveProviderTests
    {
        private static readonly AssignableProgressiveId _nonSharedSap = new(AssignableProgressiveType.None, "0");

        private static readonly AssignableProgressiveId _sharedSap = new(AssignableProgressiveType.AssociativeSap, "1");

        private static readonly AssignableProgressiveId _customSap = new(AssignableProgressiveType.CustomSap, "2");

        private static readonly AssignableProgressiveId _linkedProg = new(AssignableProgressiveType.Linked, "3");

        private readonly List<ProgressiveLevel> _testData = new()
        {
            new ProgressiveLevel
            {
                ResetValue = 1000,
                MaximumValue = 2000,
                CurrentValue = 2000,
                AssignedProgressiveId = _sharedSap
            },
            new ProgressiveLevel
            {
                ResetValue = 500,
                MaximumValue = 1000,
                CurrentValue = 999,
                AssignedProgressiveId = _sharedSap
            },
            new ProgressiveLevel
            {
                ResetValue = 300,
                MaximumValue = 1000,
                CurrentValue = 1000,
                AssignedProgressiveId = _customSap
            },
            new ProgressiveLevel
            {
                ResetValue = 1,
                MaximumValue = 2,
                CurrentValue = 1,
                AssignedProgressiveId = _customSap
            },
            new ProgressiveLevel
            {
                ResetValue = 1,
                MaximumValue = 2,
                CurrentValue = 2,
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
                CurrentValue = 2,
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
            _prng
                .Setup(x => x.GetValue(It.IsAny<ulong>()))
                .Returns<ulong>(x => x);

            _randomFactory = new Mock<IRandomFactory>();
            _randomFactory
                .Setup(x => x.Create(It.IsAny<RandomType>()))
                .Returns(_prng.Object);

            _persistentTransaction = new Mock<IPersistentTransaction>();
            _persistentBlock = new Mock<IPersistentBlock>();
            _persistentBlock
                .Setup(x => x.GetOrCreateValue<ConcurrentDictionary<string, decimal>>(It.IsAny<string>()))
                .Returns(new ConcurrentDictionary<string, decimal>());
            _persistentBlock
                .Setup(x => x.Transaction())
                .Returns(_persistentTransaction.Object);

            _persistenceProvider = new Mock<IPersistenceProvider>();
            _persistenceProvider
                .Setup(x => x.GetOrCreateBlock(It.IsAny<string>(), PersistenceLevel.Static))
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
                Assert.AreEqual(progressiveLevel.MaximumValue, number);
            }
        }

        [TestMethod]
        public void GetMagicNumbersTest()
        {
            foreach (var progressiveLevel in _testData)
            {
                 _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);

                var status = _mysteryProgressiveProvider.GetMagicNumber(progressiveLevel, out var magicNumber);

                Assert.IsTrue(status);
                Assert.IsNotNull(magicNumber);
                Assert.AreEqual(progressiveLevel.MaximumValue, magicNumber);
            }
        }

        [TestMethod]
        public void CheckMysteryJackpotHitTest()
        {
            foreach (var progressiveLevel in _testData)
            {
                _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);

                var hit = _mysteryProgressiveProvider.CheckMysteryJackpot(progressiveLevel);

                if (progressiveLevel.MaximumValue == progressiveLevel.CurrentValue)
                {
                    Assert.IsTrue(hit);
                    continue;
                }
                Assert.IsFalse(hit);
            }
        }

        [TestMethod]
        public void GetSharedMagicNumbersTest()
        {
            foreach (var progressiveLevel in _testData)
            {
                _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);
            }

            for (var i = 0; i < _testData.Count(); i += 2)
            {

                var progressiveLevel = _testData[i];
                var sharedLevel = _testData[i + 1];

                var statusOne = _mysteryProgressiveProvider.GetMagicNumber(progressiveLevel, out var magicNumberOne);
                var statusTwo = _mysteryProgressiveProvider.GetMagicNumber(sharedLevel, out var magicNumberTwo);

                Assert.IsTrue(statusOne);
                Assert.IsNotNull(magicNumberOne);
                Assert.IsTrue(statusTwo);
                Assert.IsNotNull(magicNumberTwo);

                if (_testData[i].AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.None)
                {
                    Assert.AreNotEqual(magicNumberOne, magicNumberTwo);
                }
                else
                {
                    Assert.AreEqual(magicNumberOne, magicNumberTwo);
                }
            }
        }

        [TestMethod]
        public void GetUniqueMagicNumbersTest()
        {
            foreach(var progressiveLevel in _testData)
            {
                _mysteryProgressiveProvider.GenerateMagicNumber(progressiveLevel);
            }

            for (var i = 0; i < _testData.Count() - 2; i += 2)
            {
                var progressiveLevel = _testData[i];
                var notSharedLevel = _testData[i + 2];

                var statusOne = _mysteryProgressiveProvider.GetMagicNumber(progressiveLevel, out var magicNumberOne);
                var statusTwo = _mysteryProgressiveProvider.GetMagicNumber(notSharedLevel, out var magicNumberTwo);

                Assert.IsTrue(statusOne);
                Assert.IsNotNull(magicNumberOne);
                Assert.IsTrue(statusTwo);
                Assert.IsNotNull(magicNumberTwo);
                Assert.AreNotEqual(magicNumberOne, magicNumberTwo);
            }
        }
    }
}
