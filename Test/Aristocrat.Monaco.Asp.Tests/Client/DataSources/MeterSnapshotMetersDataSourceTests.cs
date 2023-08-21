namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Asp.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class MeterSnapshotMetersDataSourceTests
    {
        private Mock<IMeterSnapshotProvider> _meterSnapshotProvider;

        private MeterSnapshotMetersDataSource _meterSnapshotMetersDataSource;

        [TestInitialize]
        public virtual void TestInitialize()
        { 
            _meterSnapshotProvider = new Mock<IMeterSnapshotProvider>(); 
            _meterSnapshotProvider.Setup(m => m.GetSnapshotMeter(It.IsAny<string>())).Returns(0);
            _meterSnapshotMetersDataSource = new MeterSnapshotMetersDataSource(_meterSnapshotProvider.Object);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new MeterSnapshotMetersDataSource(null));
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "MeterSnapShot";
            Assert.AreEqual(expectedName, _meterSnapshotMetersDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Time_Stamp",
                "Credit_Meter",
                "Total_Games_Completed",
                "Total_Games_Won",
                "Total_Turnover",
                "Total_MoneyIn_As_Coins",
                "Total_MoneyIn_As_Bills",
                "Total_MoneyOut_As_Coins",
                "Total_MoneyIn_CashBox",
                "Total_Cashless_Credit_Transfer_In",
                "Total_Cashless_Credit_Transfer_Out",
                "Total_Money_Won_ExBonus",
                "Total_BMoney_Won_ToHandpay",
                "Total_BMoney_Won_ToCrdMeter",
                "Total_MoneyOut_As_Handpay",
                "Total_MoneyOut_As_Tickets",
                "Total_MoneyIn_As_Tickets"
            };

            var actualMembers = _meterSnapshotMetersDataSource.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberTest()
        {
            _meterSnapshotProvider.SetupSequence(m => m.GetSnapshotMeter(AspConstants.AuditUpdateTimeStampField)).Returns(3).Returns(4);

            _meterSnapshotMetersDataSource.Members.Where(m => m != "Time_Stamp")
                .ForEach(memberName => Assert.AreEqual((long)0, _meterSnapshotMetersDataSource.GetMemberValue(memberName)));

            Assert.AreEqual((long)3, _meterSnapshotMetersDataSource.GetMemberValue("Time_Stamp"));
            Assert.AreEqual((long)4, _meterSnapshotMetersDataSource.GetMemberValue("Time_Stamp"));
        }

        [TestMethod]
        public void PreLoadTest()
        {
            _meterSnapshotMetersDataSource.PreLoad();

            _meterSnapshotProvider.Verify(m => m.CreatePersistentSnapshot(false), Times.Once);
        }
    }
}
