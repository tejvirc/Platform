namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Application.Contracts.Extensions;
    using Contracts.SASProperties;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Test.Common;
    using Aristocrat.Monaco.Sas.Contracts.Client;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using System.Threading;

    [TestClass]
    public class LP90SendLegacyBonusWinAmountsHandlerTest
    {
        private LP90SendLegacyBonusWinAmountsHandler _target;
        private readonly Mock<ISasBonusCallback> _bonusCallBack = new Mock<ISasBonusCallback>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new LP90SendLegacyBonusWinAmountsHandler(_bonusCallBack.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullPropertyManagerTest()
        {
            _target = new LP90SendLegacyBonusWinAmountsHandler(null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendLegacyBonusWinAmount));
        }

        [TestMethod]
        public void HandleSendBonusWinAmountPassTest()
        {
            const long bonusMillicents = 1234560000L;
            const string bonusId = "TestBonus";
            var data = new LongPollSingleValueData<long>(1);
            // mocks for Bonus Win Amount property

            _bonusCallBack.Setup(x => x.GetLastPaidLegacyBonus()).Returns(new BonusTransaction(0, DateTime.Now, bonusId, bonusMillicents, 0, 0, 0, 1000, PayMethod.Any)
            {
                Mode = BonusMode.NonDeductible,
                State = BonusState.Committed,
                PaidCashableAmount = bonusMillicents
            });

            var expected = new LegacyBonusWinAmountResponse
            {
                BonusAmount = (ulong)bonusMillicents.MillicentsToCents(),
                Multiplier = 0,
                MultipliedWin = 0,
                TaxStatus = TaxStatus.Nondeductible
            };

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.BonusAmount, actual.BonusAmount);
            Assert.AreEqual(expected.Multiplier, actual.Multiplier);
            Assert.AreEqual(expected.MultipliedWin, actual.MultipliedWin);
            Assert.AreEqual(expected.TaxStatus, actual.TaxStatus);
        }

        [TestMethod]
        public void AckNackHandlerTest()
        {
            var waiter = new ManualResetEvent(false);

            // setups for Handle call
            const long bonusMillicents = 1234560000L;
            const string bonusId = "TestBonus";
            var data = new LongPollSingleValueData<long>(1);
            _bonusCallBack.Setup(x => x.GetLastPaidLegacyBonus()).Returns(new BonusTransaction(0, DateTime.Now, bonusId, bonusMillicents, 0, 0, 0, 1000, PayMethod.Any)
            {
                Mode = BonusMode.Standard,
                State = BonusState.Committed,
                PaidCashableAmount = bonusMillicents
            });

            var actual = _target.Handle(data);
            _bonusCallBack.Setup(x => x.AcknowledgeBonus(bonusId))
                .Callback(() => waiter.Set())
                .Verifiable();

            // simulate getting an Implied Ack
            actual.Handlers.ImpliedAckHandler.Invoke();

            Assert.IsTrue(waiter.WaitOne(1000));

            _bonusCallBack.Verify();

            // verify NACK handler is null
            Assert.IsNull(actual.Handlers.ImpliedNackHandler);
        }
    }
}