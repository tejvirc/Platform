
namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Asp.Extensions;
    using Aristocrat.Monaco.Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class UsaMeterSnapshotMetersDataSourceTests
    {
        private Mock<IMeterSnapshotProvider> _meterSnapshotProvider; 
        private Mock<IEventBus> _eventBus;

        private Action<MeterSnapshotCompletedEvent> _meterSnapshotCompletedEvent;
        private UsaMeterSnapshotMetersDataSource _usaMeterSnapshotMetersDataSource;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>(); 
            _meterSnapshotProvider = new Mock<IMeterSnapshotProvider>(); 
            _meterSnapshotProvider.Setup(m => m.GetSnapshotMeter(It.IsAny<string>())).Returns(0);
            _meterSnapshotProvider.Setup(m => m.CreatePersistentSnapshot(false));

            _eventBus.Setup(m => m.Subscribe<MeterSnapshotCompletedEvent>(It.IsAny<UsaMeterSnapshotMetersDataSource>(), It.IsAny<Action<MeterSnapshotCompletedEvent>>()))
              .Callback<object, Action<MeterSnapshotCompletedEvent>>((subscriber, meterSnapshotCompletedAction) => _meterSnapshotCompletedEvent = meterSnapshotCompletedAction);

            _usaMeterSnapshotMetersDataSource = new UsaMeterSnapshotMetersDataSource(_meterSnapshotProvider.Object, _eventBus.Object);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new UsaMeterSnapshotMetersDataSource(null, _eventBus.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new UsaMeterSnapshotMetersDataSource(_meterSnapshotProvider.Object, null));
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "UsaMeterSnapShot";
            Assert.AreEqual(expectedName, _usaMeterSnapshotMetersDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Time_Stamp",
                "Credit_Meter",
                "USA_Coin_In_Meter",
                "USA_Coin_Out_Meter",
                "USA_Drop_Meter",
                "USA_Jackpot_Meter",
                "USA_Cancel_Credit_Meter",
                "Total_Games_Completed",
                "Total_Games_Won",
                "Total_MoneyIn_As_Coins",
                "Total_MoneyIn_As_Bills",
                "Total_MoneyOut_As_Coins",
                "Total_MoneyIn_CashBox",
                "Total_Cashless_Credit_Transfer_In",
                "Total_Cashless_Credit_Transfer_Out"
            };

            var actualMembers = _usaMeterSnapshotMetersDataSource.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberTest()
        {
            _meterSnapshotProvider.SetupSequence(m => m.GetSnapshotMeter(AspConstants.AuditUpdateTimeStampField)).Returns(3).Returns(4);

            _usaMeterSnapshotMetersDataSource.Members.Where(m => m != "Time_Stamp")
                .ForEach(memberName => Assert.AreEqual((long)0, _usaMeterSnapshotMetersDataSource.GetMemberValue(memberName)));

            Assert.AreEqual((long)3, _usaMeterSnapshotMetersDataSource.GetMemberValue("Time_Stamp"));
            Assert.AreEqual((long)4, _usaMeterSnapshotMetersDataSource.GetMemberValue("Time_Stamp"));
        }


        [TestMethod]
        public void PreloadTest()
        {
            _usaMeterSnapshotMetersDataSource.PreLoad();
            _meterSnapshotProvider.Verify(m => m.CreatePersistentSnapshot(false), Times.Once());
        }

        [TestMethod]
        public void MeterSnapshotCompletedEventTest()
        {
            var calledMembers = new Dictionary<string, int>();

            _usaMeterSnapshotMetersDataSource.MemberValueChanged += (s, e) =>
            {
                Assert.AreEqual(_usaMeterSnapshotMetersDataSource.Members.Count, e.Count);
                foreach (var memberName in e.Keys)
                {
                    if (calledMembers.ContainsKey(memberName)) calledMembers[memberName]++;
                    else calledMembers.Add(memberName, 1);
                }
            };

            _meterSnapshotCompletedEvent(new MeterSnapshotCompletedEvent());

            Assert.IsTrue(calledMembers.ContainsKey("Credit_Meter"));
            Assert.IsTrue(calledMembers["Credit_Meter"] == 1);
        }

        [TestMethod]
        public void Dispose_ShouldUnsubscribeAll()
        {
            //Call dispose twice - should only unsubscribe/deregister from events once
            _usaMeterSnapshotMetersDataSource.Dispose();
            _usaMeterSnapshotMetersDataSource.Dispose();

            _eventBus.Verify(v => v.UnsubscribeAll(It.IsAny<object>()), Times.Once);
        }
    }
}
