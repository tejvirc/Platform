namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Monaco.Asp.Client.Contracts;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class MeterSnapshotStatusDataSourceTest
    {
        private Mock<IMeterSnapshotProvider> _meterSnapshotProvider;

        private MeterSnapshotStatusDataSource _meterSnapshotStatusDataSource;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _meterSnapshotProvider = new Mock<IMeterSnapshotProvider>();
            _meterSnapshotProvider.Setup(m => m.GetSnapshotMeter(It.IsAny<string>())).Returns(1);
            _meterSnapshotProvider.Setup(m => m.CreatePersistentSnapshot(false));
            _meterSnapshotProvider.SetupSet(m => m.SnapshotStatus = It.IsAny<MeterSnapshotStatus>()).Callback<MeterSnapshotStatus>(val => { _meterSnapshotProvider.SetupGet(g => g.SnapshotStatus).Returns(val); });
            _meterSnapshotStatusDataSource = new MeterSnapshotStatusDataSource(_meterSnapshotProvider.Object);
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "MeterSnapShotUpdate";
            Assert.AreEqual(expectedName, _meterSnapshotStatusDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var expectedMembers = new List<string>()
            {
                "Audit_Update"
            };

            var actualMembers = _meterSnapshotStatusDataSource.Members;
            Assert.AreEqual(expectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(expectedMembers));
        }

        [TestMethod]
        public void GetMemberTest()
        {
            Assert.AreEqual(_meterSnapshotStatusDataSource.GetMemberValue("Audit_Update") is MeterSnapshotStatus, true);
        }

        [TestMethod]
        public void SetAuditUpdateTest()
        {
            string value = "Disabled";
            _meterSnapshotStatusDataSource.SetMemberValue("Audit_Update", value);
            MeterSnapshotStatus meterSnapshotStatus = (MeterSnapshotStatus)_meterSnapshotStatusDataSource.GetMemberValue("Audit_Update");
            Assert.AreEqual(meterSnapshotStatus, MeterSnapshotStatus.Disabled);

            value = "Enabled";
            _meterSnapshotStatusDataSource.SetMemberValue("Audit_Update", value);
            meterSnapshotStatus = (MeterSnapshotStatus)_meterSnapshotStatusDataSource.GetMemberValue("Audit_Update");
            Assert.AreEqual(meterSnapshotStatus, MeterSnapshotStatus.Enabled);

            value = null;
            _meterSnapshotStatusDataSource.SetMemberValue("Audit_Update", value);
            meterSnapshotStatus = (MeterSnapshotStatus)_meterSnapshotStatusDataSource.GetMemberValue("Audit_Update");
            Assert.AreEqual(meterSnapshotStatus, MeterSnapshotStatus.Enabled);

            value = "Test Invalid Status";
            _meterSnapshotStatusDataSource.SetMemberValue("Audit_Update", value);
            meterSnapshotStatus = (MeterSnapshotStatus)_meterSnapshotStatusDataSource.GetMemberValue("Audit_Update");
            Assert.AreEqual(meterSnapshotStatus, MeterSnapshotStatus.Enabled);
        }
    }
}
