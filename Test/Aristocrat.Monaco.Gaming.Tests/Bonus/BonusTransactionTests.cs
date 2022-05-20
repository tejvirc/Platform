namespace Aristocrat.Monaco.Gaming.Tests.Bonus
{
    using System;
    using System.Collections.Generic;
    using Contracts.Bonus;
    using Hardware.Contracts.Persistence;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class BonusTransactionTests
    {
        private const int DeviceId = 23;
        private const long TransactionId = 98784;
        private const long Amount = 45000;
        private const string BonusId = "TestBonusId";
        private const long LogSequence = 41;
        private const int GameId = 1;
        private const long DenomId = 1000;
        private static readonly DateTime DateTime = DateTime.Now;

        private Mock<IPersistentStorageAccessor> _block;
        private BonusTransaction _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);

            _block = new Mock<IPersistentStorageAccessor>(MockBehavior.Strict);
            _target = new BonusTransaction();
        }

        [TestMethod]
        public void ReceivePersistenceTest()
        {
            var values = new Dictionary<string, object>
            {
                { "TransactionId", TransactionId },
                { "LogSequence", LogSequence },
                { "DeviceId", DeviceId },
                { "TransactionDateTime", DateTime },
                { "BonusId", BonusId },
                { "State", BonusState.Acknowledged },
                { "CashableAmount", Amount },
                { "NonCashAmount", 0L },
                { "PromoAmount", 0L },
                { "GameId", GameId },
                { "Denom", DenomId },
                { "PayMethod", PayMethod.Any },
                { "IdRequired", false },
                { "IdNumber", string.Empty },
                { "PlayerId", string.Empty },
                { "PaidCashableAmount", Amount },
                { "PaidNonCashAmount", 0L },
                { "PaidPromoAmount", 0L },
                { "PaidDateTime", DateTime.UtcNow },
                { "LastUpdate", DateTime.UtcNow },
                { "Exception", 0 },
                { "ExceptionInformation", 0 },
                { "Mode", BonusMode.Standard },
                { "DisplayMessageId", Guid.Empty },
                { "BankTransactionId", Guid.Empty },
                { "ProcessorId", Guid.Empty },
                { "Message", string.Empty },
                { "MjtAmountWagered", 0L },
                { "MjtBonusGamesPaid", 0 },
                { "MjtBonusGamesPlayed", 0 },
                { "MjtMaximumWin", 0L },
                { "MjtMinimumWin", 0L },
                { "MjtNumberOfGames", 0 },
                { "MjtWagerRestriction", WagerRestriction.CurrentBet },
                { "MjtRequiredWager", 0L },
                { "MjtWinMultiplier", 1 },
                { "Request", null },
                { "JackpotNumber", 231 },
                { "SourceID", "003001020" },
                { "AssociatedTransactions", "[]" },
                { "MessageDuration", TimeSpan.MinValue.Ticks },
                { "TraceId", Guid.Empty }
            };

            Assert.IsTrue(_target.SetData(values));
        }

        [TestMethod]
        public void SetPersistenceTest()
        {
            const int element = 0;
            _target = CreateBonusTransaction();

            _block.Setup(m => m.Count).Returns(1);
            var transaction = new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);
            transaction.SetupSet(m => m[element, "TransactionId"] = _target.TransactionId);
            transaction.SetupSet(m => m[element, "LogSequence"] = _target.LogSequence);
            transaction.SetupSet(m => m[element, "DeviceId"] = _target.DeviceId);
            transaction.SetupSet(m => m[element, "TransactionDateTime"] = _target.TransactionDateTime);
            transaction.SetupSet(m => m[element, "BonusId"] = _target.BonusId);
            transaction.SetupSet(m => m[element, "State"] = (int)_target.State);
            transaction.SetupSet(m => m[element, "CashableAmount"] = _target.CashableAmount);
            transaction.SetupSet(m => m[element, "NonCashAmount"] = _target.NonCashAmount);
            transaction.SetupSet(m => m[element, "PromoAmount"] = _target.PromoAmount);
            transaction.SetupSet(m => m[element, "GameId"] = _target.GameId);
            transaction.SetupSet(m => m[element, "Denom"] = _target.Denom);
            transaction.SetupSet(m => m[element, "PayMethod"] = (int)_target.PayMethod);
            transaction.SetupSet(m => m[element, "IdRequired"] = _target.IdRequired);
            transaction.SetupSet(m => m[element, "IdNumber"] = _target.IdNumber);
            transaction.SetupSet(m => m[element, "PlayerId"] = _target.PlayerId);
            transaction.SetupSet(m => m[element, "PaidCashableAmount"] = _target.PaidCashableAmount);
            transaction.SetupSet(m => m[element, "PaidNonCashAmount"] = _target.PaidNonCashAmount);
            transaction.SetupSet(m => m[element, "PaidPromoAmount"] = _target.PaidPromoAmount);
            transaction.SetupSet(m => m[element, "PaidDateTime"] = _target.PaidDateTime);
            transaction.SetupSet(m => m[element, "LastUpdate"] = _target.LastUpdate);
            transaction.SetupSet(m => m[element, "Exception"] = _target.Exception);
            transaction.SetupSet(m => m[element, "ExceptionInformation"] = _target.ExceptionInformation);
            transaction.SetupSet(m => m[element, "Mode"] = (int)_target.Mode);
            transaction.SetupSet(m => m[element, "DisplayMessageId"] = _target.DisplayMessageId);
            transaction.SetupSet(m => m[element, "Message"] = _target.Message);

            transaction.SetupSet(m => m[element, "MjtAmountWagered"] = _target.MjtAmountWagered);
            transaction.SetupSet(m => m[element, "MjtBonusGamesPaid"] = _target.MjtBonusGamesPaid);
            transaction.SetupSet(m => m[element, "MjtBonusGamesPlayed"] = _target.MjtBonusGamesPaid);
            transaction.SetupSet(m => m[element, "MjtMaximumWin"] = _target.MjtMaximumWin);
            transaction.SetupSet(m => m[element, "MjtMinimumWin"] = _target.MjtMinimumWin);
            transaction.SetupSet(m => m[element, "MjtNumberOfGames"] = _target.MjtNumberOfGames);
            transaction.SetupSet(m => m[element, "MjtWagerRestriction"] = _target.MjtWagerRestriction);
            transaction.SetupSet(m => m[element, "MjtRequiredWager"] = _target.MjtRequiredWager);
            transaction.SetupSet(m => m[element, "MjtWinMultiplier"] = _target.MjtWinMultiplier);
            transaction.SetupSet(m => m[element, "Request"] = null);
            transaction.SetupSet(mock => mock[0, "AssociatedTransactions"] = "[]");
            transaction.SetupSet(m => m[element, "MessageDuration"] = _target.MessageDuration.Ticks);
            transaction.SetupSet(m => m[element, nameof(_target.JackpotNumber)] = _target.JackpotNumber);
            transaction.SetupSet(m => m[element, nameof(_target.SourceID)] = _target.SourceID);
            transaction.SetupSet(m => m[element, nameof(_target.Protocol)] = (int)_target.Protocol);

            transaction.Setup(m => m.Dispose()).Verifiable();
            transaction.Setup(m => m.Commit()).Verifiable();
            _block.Setup(x => x.StartTransaction()).Returns(transaction.Object);

            _target.SetPersistence(_block.Object, element);

            _block.VerifyAll();
            transaction.VerifyAll();
        }

        [TestMethod]
        public void CloneTest()
        {
            _target = CreateBonusTransaction();

            var same = _target.Clone() as BonusTransaction;
            Assert.IsNotNull(same);
            Assert.IsFalse(ReferenceEquals(_target, same));
            Assert.AreEqual(_target, same);

            Assert.AreEqual(_target.TransactionId, same.TransactionId);
            Assert.AreEqual(_target.LogSequence, same.LogSequence);
            Assert.AreEqual(_target.DeviceId, same.DeviceId);
            Assert.AreEqual(_target.TransactionDateTime, same.TransactionDateTime);
            Assert.AreEqual(_target.BonusId, same.BonusId);
            Assert.AreEqual(_target.State, same.State);
            Assert.AreEqual(_target.CashableAmount, same.CashableAmount);
            Assert.AreEqual(_target.NonCashAmount, same.NonCashAmount);
            Assert.AreEqual(_target.PromoAmount, same.PromoAmount);
            Assert.AreEqual(_target.GameId, same.GameId);
            Assert.AreEqual(_target.Denom, same.Denom);
            Assert.AreEqual(_target.PayMethod, same.PayMethod);
            Assert.AreEqual(_target.IdRequired, same.IdRequired);
            Assert.AreEqual(_target.IdNumber, same.IdNumber);
            Assert.AreEqual(_target.PlayerId, same.PlayerId);
            Assert.AreEqual(_target.PaidAmount, same.PaidAmount);
            Assert.AreEqual(_target.PaidCashableAmount, same.PaidCashableAmount);
            Assert.AreEqual(_target.PaidNonCashAmount, same.PaidNonCashAmount);
            Assert.AreEqual(_target.PaidPromoAmount, same.PaidPromoAmount);
            Assert.AreEqual(_target.PaidDateTime, same.PaidDateTime);
            Assert.AreEqual(_target.LastUpdate, same.LastUpdate);
            Assert.AreEqual(_target.Exception, same.Exception);
            Assert.AreEqual(_target.ExceptionInformation, same.ExceptionInformation);
            Assert.AreEqual(_target.Mode, same.Mode);
            Assert.AreEqual(_target.DisplayMessageId, same.DisplayMessageId);
        }

        private static BonusTransaction CreateBonusTransaction()
        {
            return new BonusTransaction(DeviceId, DateTime, BonusId, Amount, 0, 0, GameId, DenomId, PayMethod.Any)
            {
                PaidDateTime = DateTime.UtcNow,
                State = BonusState.Acknowledged,
                Exception = 0,
                ExceptionInformation = 0,
                IdRequired = false,
                IdNumber = string.Empty,
                Mode = BonusMode.Standard,
                LastUpdate = DateTime.MaxValue,
                PaidCashableAmount = Amount,
                PlayerId = string.Empty,
                LogSequence = LogSequence,
                Protocol = CommsProtocol.None,
            };
        }
    }
}