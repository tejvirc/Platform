namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Asp.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Asp.Client.DataSources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EgmMetersDataSourceTests
    {
        private Mock<IEventBus> _eventBus;
        private Action<BankBalanceChangedEvent> _bankBalanceChangedAction;

        private readonly List<(Mock<IMeter> Meter, string MeterName, string MemberName, long Lifetime, int Scale)> _meters = new List<(Mock<IMeter>, string, string, long, int)>();

        private Mock<IMeterManager> _meterManager;
        private EgmMetersDataSource _source;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _meterManager = new Mock<IMeterManager>(MockBehavior.Strict);
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);

            _meters.Add((new Mock<IMeter>(MockBehavior.Strict), GamingMeters.PlayedCount, "Games_Completed", 100000, 1));
            _meters.Add((new Mock<IMeter>(MockBehavior.Strict), GamingMeters.WageredAmount, "Total_Turnover", 200000, 1000));
            _meters.Add((new Mock<IMeter>(MockBehavior.Strict), AccountingConstants.LargeWinLimit, "Reserved", 300000, 1000));
            _meters.Add((new Mock<IMeter>(MockBehavior.Strict), GamingMeters.HandPaidBonusAmount, "Tot_Money_Won_ToHandpaid", 400000, 1000));
            _meters.Add((new Mock<IMeter>(MockBehavior.Strict), GamingMeters.EgmPaidBonusAmount, "Total_Money_Won_ToCreditMeter", 500000, 1000));
            _meters.Add((new Mock<IMeter>(MockBehavior.Strict), AccountingMeters.CurrentCredits, "Current_Credit_Base_Units", 400000, 1000));

            _meters.ForEach(l => _meterManager.Setup(m => m.IsMeterProvided(It.Is<string>(a => a == l.MeterName))).Returns(() => true).Verifiable());
            _meters.ForEach(l => _meterManager.Setup(m => m.GetMeter(It.Is<string>(a => a == l.MeterName))).Returns(() => l.Meter.Object).Verifiable());

            _meters.ForEach(l => l.Meter.SetupGet(s => s.Name).Returns(() => l.MeterName).Verifiable());
            _meters.ForEach(l => l.Meter.SetupGet(s => s.Lifetime).Returns(() => l.Lifetime).Verifiable());

            _eventBus.Setup(m => m.Subscribe<BankBalanceChangedEvent>(It.IsAny<EgmMetersDataSource>(), It.IsAny<Action<BankBalanceChangedEvent>>()))
              .Callback<object, Action<BankBalanceChangedEvent>>((subscriber, bankBalanceChangedEvent) => _bankBalanceChangedAction = bankBalanceChangedEvent);

            _eventBus.Setup(s => s.UnsubscribeAll(It.IsAny<object>())).Verifiable();

            _source = new EgmMetersDataSource(_meterManager.Object, _eventBus.Object);
        }

        [TestMethod]
        public void GetMembersTest()
        {
            var members = _source.Members.ToList();
            CollectionAssert.AreEqual(members, (from m in _meters select m.MemberName).ToList());
        }

        [TestMethod]
        public void GetMemberValueTest()
        {
            var results = new Dictionary<string, object>();
            _meters.ForEach(l =>
            {
                var value = _source.GetMemberValue(l.MemberName);
                Assert.IsNotNull(value);
                results[l.MemberName] = value;
            });

            Assert.IsNull(_source.GetMemberValue("random"));

            _meters.ForEach(l => Assert.AreEqual(Convert.ToInt32(results[l.MemberName]), l.Lifetime / l.Scale));
            _meters.ForEach(l => _meterManager.Verify(v => v.IsMeterProvided(It.Is<string>(a => a == l.MeterName)), Times.Exactly(2)));
            _meters.ForEach(l => l.Meter.VerifyGet(v => v.Lifetime, Times.Once));
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(new EgmMetersDataSource(_meterManager.Object, _eventBus.Object));
        }

        [TestMethod]
        public void NullConsutructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new EgmMetersDataSource(null, null));
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "EGMMeters";
            Assert.AreEqual(expectedName, _source.Name);
        }

        [TestMethod]
        public void MeterValueChangeTriggersDataSourceMemberValueChangedEvent()
        {
            var calledMembers = new Dictionary<string, int>();
            _source.MemberValueChanged += (s, e) =>
            {
                Assert.AreEqual(1, e.Count);
                var memberName = e.First().Key;
                if (calledMembers.ContainsKey(memberName)) calledMembers[memberName]++;
                else calledMembers.Add(memberName, 1);
            };

            _meters[1].Meter.Raise(r => r.MeterChangedEvent += null, new MeterChangedEventArgs(1000));
            _meters.ForEach(l => l.Meter.Raise(r => r.MeterChangedEvent += null, new MeterChangedEventArgs(1000)));
            _meters[3].Meter.Raise(r => r.MeterChangedEvent += null, new MeterChangedEventArgs(1000));

            var member1 = _meters[1].MemberName;
            Assert.IsTrue(calledMembers.ContainsKey(member1));
            Assert.IsTrue(calledMembers[member1] == 2);

            var meter2 = _meters.Except(new[] { _meters[1], _meters[3] });
            meter2.ForEach(l =>
            {
                var member2 = l.MemberName;
                Assert.IsTrue(calledMembers.ContainsKey(member2));
                Assert.IsTrue(calledMembers[member2] == 1);
            });

            var member3 = _meters[3].MemberName;
            Assert.IsTrue(calledMembers.ContainsKey(member3));
            Assert.IsTrue(calledMembers[member3] == 2);

            meter2.ForEach(l => _meterManager.Verify(v => v.IsMeterProvided(It.Is<string>(a => a == l.MeterName)), Times.Exactly(2)));
        }

        [TestMethod]
        public void BankBalanceChangedEventCurrentCreditBaseUnitTest()
        {
            var calledMembers = new Dictionary<string, int>();

            _source.MemberValueChanged += (s, e) =>
            {
                Assert.AreEqual(1, e.Count);
                var memberName = e.First().Key;
                if (calledMembers.ContainsKey(memberName)) calledMembers[memberName]++;
                else calledMembers.Add(memberName, 1);
            };

            _bankBalanceChangedAction(new BankBalanceChangedEvent(1, 1, new Guid()));
            _bankBalanceChangedAction(new BankBalanceChangedEvent(1, 2, new Guid()));
            _bankBalanceChangedAction(new BankBalanceChangedEvent(2, 2, new Guid()));

            Assert.IsTrue(calledMembers.ContainsKey("Current_Credit_Base_Units"));
            Assert.IsTrue(calledMembers["Current_Credit_Base_Units"] == 1);
        }

		[TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //null meter should not throw exception
            _meterManager.Setup(l => l.GetMeter(GamingMeters.PlayedCount)).Returns(() => null);

            //Call dispose twice - should only unsubscribe/deregister from events once
            _source.Dispose();
            _source.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);

            _meters.ForEach(l => _meterManager.Verify(v => v.IsMeterProvided(It.Is<string>(a => a == l.MeterName)), Times.Exactly(2)));
            _meters.ForEach(l => _meterManager.Verify(v => v.GetMeter(It.Is<string>(a => a == l.MeterName)), Times.Exactly(2)));
            //Verify .Net event deregistration
            //_meters.ForEach(f => f.Meter.VerifyRemove(v => v.MeterChangedEvent -= It.IsAny<EventHandler<MeterChangedEventArgs>>(), Times.Once));
        }
    }
}