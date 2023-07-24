namespace Aristocrat.Monaco.Gaming.Tests.GameHistoryLogSerializationTests
{
    using Aristocrat.Monaco.Gaming.Commands;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Aristocrat.Monaco.Gaming.Proto.Wrappers;
    using Aristocrat.Monaco.Gaming.Tests.GameHistoryLogTest;
    using Aristocrat.Monaco.Test.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class GameHistoryLogSerializationTests
    {
        private Proto.GameHistoryLog _randomLog;
        //private IGameHistoryLog _gameHistoryLog_source;
        //private IGameHistoryLog _gameHistoryLog_destination;

        [TestInitialize()]
        public void Initialize()
        {
            Random random = new Random();
            _randomLog = GameHistoryLogDataGenerator.GenerateRandomLog(random);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void TestCleanup()
        {
           
        }

        [TestMethod()]
        public void TestRawClone()
        {
            var clonedLog = _randomLog.Clone();
            Assert.AreEqual(_randomLog, clonedLog);
        }

        [TestMethod()]
        public void TestClone()
        {
            var wrappedLog = new GameHistoryLogDecorator(_randomLog);
            var clonedLog = new GameHistoryLogDecorator(_randomLog);
            Assert.AreEqual(wrappedLog, clonedLog);
        }

        [TestMethod()]
        public void TestShallowClone()
        {
            var wrappedLog = new GameHistoryLogDecorator(_randomLog);
            var clonedLog = wrappedLog.ShallowCopy();
            Assert.AreEqual(wrappedLog, clonedLog);
        }

        [TestMethod()]
        public void TestOrigClone()
        {
            var wrappedLog = new GameHistoryLogDecorator(_randomLog);
            var clonedLog = wrappedLog.ShallowCopy();
            Assert.AreEqual(wrappedLog, clonedLog);
        }

        [TestMethod()]
        public void TestGameHistoryLogDecoratorCloneViaFromMethod()
        {
            var source = new GameHistoryLogDecorator(_randomLog);
            var cloned = source.From(source);

            AreIGameHistoryLogsEqual(source, cloned);
        }

        [TestMethod()]
        public void TestGameHistoryLogCloneViaFromMethod()
        {
            var source = new GameHistoryLogDecorator(_randomLog);
            var cloneMe = new GameHistoryLog(source.StorageIndex);

            var cloned = cloneMe.From(source);

            AreIGameHistoryLogsEqual(source, cloned);
        }

        private static void AreIGameHistoryLogsEqual(IGameHistoryLog source, IGameHistoryLog cloned)
        {
            Assert.AreEqual(cloned.StorageIndex, source.StorageIndex);
            Assert.IsTrue(cloned.RecoveryBlob.SequenceEqual(source.RecoveryBlob));
            Assert.AreEqual(cloned.DenomConfiguration, source.DenomConfiguration);
            Assert.AreEqual(cloned.TransactionId, source.TransactionId);
            Assert.AreEqual(cloned.LogSequence, source.LogSequence);
            Assert.AreEqual(cloned.StartDateTime, source.StartDateTime);
            Assert.AreEqual(cloned.EndDateTime, source.EndDateTime);
            Assert.AreEqual(cloned.EndTransactionId, source.EndTransactionId);
            Assert.AreEqual(cloned.GameId, source.GameId);
            Assert.AreEqual(cloned.DenomId, source.DenomId);
            Assert.AreEqual(cloned.StartCredits, source.StartCredits);
            Assert.AreEqual(cloned.EndCredits, source.EndCredits);
            Assert.AreEqual(cloned.PlayState, source.PlayState);
            Assert.AreEqual(cloned.InitialWager, source.InitialWager);
            Assert.AreEqual(cloned.FinalWager, source.FinalWager);
            Assert.AreEqual(cloned.PromoWager, source.PromoWager);
            Assert.AreEqual(cloned.UncommittedWin, source.UncommittedWin);
            Assert.AreEqual(cloned.InitialWin, source.InitialWin);
            Assert.AreEqual(cloned.SecondaryPlayed, source.SecondaryPlayed);
            Assert.AreEqual(cloned.SecondaryWager, source.SecondaryWager);
            Assert.AreEqual(cloned.SecondaryWin, source.SecondaryWin);
            Assert.AreEqual(cloned.FinalWin, source.FinalWin);
            Assert.AreEqual(cloned.GameWinBonus, source.GameWinBonus);
            Assert.AreEqual(cloned.AmountOut, source.AmountOut);
            Assert.AreEqual(cloned.LastUpdate, source.LastUpdate);
            Assert.AreEqual(cloned.LastCommitIndex, source.LastCommitIndex);
            Assert.AreEqual(cloned.GameRoundDescriptions, source.GameRoundDescriptions);
            Assert.IsTrue(cloned.JackpotSnapshot.SequenceEqual(source.JackpotSnapshot));
            Assert.IsTrue(cloned.JackpotSnapshotEnd.SequenceEqual(source.JackpotSnapshotEnd));
            Assert.IsTrue(cloned.Jackpots.SequenceEqual(source.Jackpots));
            Assert.IsTrue(cloned.Transactions.SequenceEqual(source.Transactions));
            Assert.IsTrue(cloned.Events.SequenceEqual(source.Events));
            Assert.IsTrue(cloned.MeterSnapshots.SequenceEqual(source.MeterSnapshots));
            Assert.IsTrue(cloned.FreeGames.SequenceEqual(source.FreeGames));
            Assert.IsTrue(cloned.CashOutInfo.SequenceEqual(source.CashOutInfo));
            Assert.IsTrue(cloned.Outcomes.SequenceEqual(source.Outcomes));
            Assert.AreEqual(cloned.LocaleCode, source.LocaleCode);
            Assert.AreEqual(cloned.GameConfiguration, source.GameConfiguration);
            Assert.AreEqual(cloned.GameRoundDetails, source.GameRoundDetails);
        }
    }
}
