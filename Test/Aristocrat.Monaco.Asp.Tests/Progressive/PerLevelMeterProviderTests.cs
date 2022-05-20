namespace Aristocrat.Monaco.Asp.Tests.Progressive
{
    using Aristocrat.Monaco.Asp.Progressive;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;

    [TestClass]
    public class PerLevelMeterProviderTests
    {
        private Mock<IPersistentStorageManager> _persistenceStorageManager;
        private Mock<IPersistentStorageAccessor> _persistentStorageAccessor;
        private Mock<IPersistentStorageTransaction> _persistentStorageTransaction;

        private IPerLevelMeterProvider _subject;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _persistenceStorageManager = new Mock<IPersistentStorageManager>();
            _persistentStorageAccessor = new Mock<IPersistentStorageAccessor>();
            _persistentStorageTransaction = new Mock<IPersistentStorageTransaction>();

            _persistenceStorageManager.Setup(s => s.GetBlock(It.IsAny<string>())).Returns(() => _persistentStorageAccessor.Object).Verifiable();
            _persistenceStorageManager.Setup(s => s.CreateDynamicBlock(It.Is<PersistenceLevel>(i => i == PersistenceLevel.Static), It.IsAny<string>(), It.Is<int>(i => i == 1), It.IsAny<BlockFormat>())).Returns(() => _persistentStorageAccessor.Object).Verifiable();
            _persistenceStorageManager.Setup(s => s.BlockExists(It.IsAny<string>())).Returns(() => true).Verifiable();

            _persistentStorageAccessor.Setup(s => s.StartTransaction()).Returns(() => _persistentStorageTransaction.Object).Verifiable();

            _subject = new PerLevelMeterProvider(_persistenceStorageManager.Object);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new PerLevelMeterProvider(null));
        }

        [TestMethod]
        public void Constructor_StorageInitialisation_ShouldCreateNewBlock()
        {
            _persistenceStorageManager.Setup(s => s.BlockExists(It.IsAny<string>())).Returns(() => false);

            _subject = new PerLevelMeterProvider(_persistenceStorageManager.Object);

            _persistenceStorageManager.Verify(v => v.GetBlock(It.IsAny<string>()), Times.Once);
            _persistenceStorageManager.Verify(v => v.CreateDynamicBlock(It.Is<PersistenceLevel>(i => i == PersistenceLevel.Static), It.IsAny<string>(), It.Is<int>(i => i == 1), It.IsAny<BlockFormat>()), Times.Once);

            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Once);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Once);
        }

        [TestMethod]
        public void Constructor_StorageInitialisation_ShouldGetExistingBlock()
        {
            _subject = new PerLevelMeterProvider(_persistenceStorageManager.Object);

            _persistenceStorageManager.Verify(v => v.GetBlock(It.IsAny<string>()), Times.Exactly(2));
            _persistenceStorageManager.Verify(v => v.CreateDynamicBlock(It.Is<PersistenceLevel>(i => i == PersistenceLevel.Static), It.IsAny<string>(), It.Is<int>(i => i == 1), It.IsAny<BlockFormat>()), Times.Never);

            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Never);
            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Never);
        }

        [TestMethod]
        public void GetValue_ConvertToLongFailsReturnsZero()
        {
            var getAll = new Dictionary<int, Dictionary<string, object>>
            {
                {
                    0, new Dictionary<string, object>
                    {
                        { $"{ProgressivePerLevelMeters.CurrentJackpotNumber}_Level_0", 12345111111111111111 },
                        { $"{ProgressivePerLevelMeters.JackpotControllerId}_Level_0", 12345111111111111111 },
                        { $"{ProgressivePerLevelMeters.JackpotHitStatus}_Level_0", 12345111111111111111 },
                        { $"{ProgressivePerLevelMeters.LinkJackpotHitAmountWon}_Level_0", 12345111111111111111 }
                    }
                }
            };

            _persistentStorageAccessor.Setup(s => s.GetAll()).Returns(() => getAll).Verifiable();

            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotControllerId), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotHitStatus), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon), 0);

            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Exactly(4));
        }

        [TestMethod]
        public void GetValue_ReturnsExistingMeterValue()
        {
            var getAll = new Dictionary<int, Dictionary<string, object>>
            {
                {
                    0, new Dictionary<string, object>
                    {
                        { $"{ProgressivePerLevelMeters.CurrentJackpotNumber}_Level_0", 12345 },
                        { $"{ProgressivePerLevelMeters.JackpotControllerId}_Level_0", 54321 },
                        { $"{ProgressivePerLevelMeters.JackpotHitStatus}_Level_0", 111222 },
                        { $"{ProgressivePerLevelMeters.LinkJackpotHitAmountWon}_Level_0", 222111 }
                    }
                }
            };

            _persistentStorageAccessor.Setup(s => s.GetAll()).Returns(() => getAll).Verifiable();

            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber), 12345);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotControllerId), 54321);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotHitStatus), 111222);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon), 222111);

            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Exactly(4));
        }

        [TestMethod]
        public void GetValue_BadStorageAccessor()
        {
            
            var getAll = new Dictionary<int, Dictionary<string, object>>
            {
                {
                    0, new Dictionary<string, object>()
                }
            };

            _persistentStorageAccessor.Setup(s => s.GetAll()).Callback(() => throw new Exception());

            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotControllerId), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotHitStatus), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon), 0);

            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Exactly(4));
        }

        [TestMethod]
        public void GetValue_ReturnsZeroWhenNoMeterExists()
        {
            var getAll = new Dictionary<int, Dictionary<string, object>>
            {
                {
                    0, new Dictionary<string, object>()
                }
            };

            _persistentStorageAccessor.Setup(s => s.GetAll()).Returns(() => getAll).Verifiable();

            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotControllerId), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.JackpotHitStatus), 0);
            Assert.AreEqual(_subject.GetValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon), 0);

            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Exactly(4));
        }

        [TestMethod]
        public void SetValue_NullTransaction()
        {
            // Pass in a bad Transaction
            _persistentStorageAccessor.Setup(x => x.StartTransaction()).Callback(() => throw new Exception());
            _subject.SetValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber, 1);
            _subject.SetValue(0, ProgressivePerLevelMeters.JackpotControllerId, 2);
            _subject.SetValue(0, ProgressivePerLevelMeters.JackpotHitStatus, 3);
            _subject.SetValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon, 4);

            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Exactly(4));

            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Never);
        }

        [TestMethod]
        public void SetValue_StartsAndCommitsTransaction()
        {
            _subject.SetValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber, 1);
            _subject.SetValue(0, ProgressivePerLevelMeters.JackpotControllerId, 2);
            _subject.SetValue(0, ProgressivePerLevelMeters.JackpotHitStatus, 3);
            _subject.SetValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon, 4);

            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Exactly(4));

            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Exactly(4));
        }

        [TestMethod]
        public void IncrementValue_StartsAndCommitsTransaction()
        {
            _subject.IncrementValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber, 1);
            _subject.IncrementValue(0, ProgressivePerLevelMeters.JackpotControllerId, 1);
            _subject.IncrementValue(0, ProgressivePerLevelMeters.JackpotHitStatus, 2);
            _subject.IncrementValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon, 3);

            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Exactly(4));
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Exactly(4));

            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Exactly(4));
        }

        [TestMethod]
        public void IncrementValue_ThrowsException()
        {
            _persistentStorageAccessor.Setup(x => x.GetAll()).Callback(() => throw new Exception());
            _persistentStorageAccessor.Setup(x => x.StartTransaction()).Callback(() => throw new Exception());

            _subject.IncrementValue(0, ProgressivePerLevelMeters.CurrentJackpotNumber, 1);
            _subject.IncrementValue(0, ProgressivePerLevelMeters.JackpotControllerId, 1);
            _subject.IncrementValue(0, ProgressivePerLevelMeters.JackpotHitStatus, 2);
            _subject.IncrementValue(0, ProgressivePerLevelMeters.LinkJackpotHitAmountWon, 3);

            _persistentStorageAccessor.Verify(v => v.GetAll(), Times.Exactly(4));
            _persistentStorageAccessor.Verify(v => v.StartTransaction(), Times.Exactly(4));

            _persistentStorageTransaction.Verify(v => v.Commit(), Times.Never);
        }
    }
}
