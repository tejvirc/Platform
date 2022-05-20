namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System.Collections.Generic;
    using System.Linq;
    using Asp.Client.DataSources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MciPropertyDataSourceTests
    {
        private MciPropertyDataSource _mciPropertyDataSource;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _mciPropertyDataSource = new MciPropertyDataSource();
        }

        [TestMethod]
        public void DataSourceNameTest()
        {
            var expectedName = "MciProperty";
            Assert.AreEqual(expectedName, _mciPropertyDataSource.Name);
        }

        [TestMethod]
        public void MembersTest()
        {
            var exptectedMembers = new List<string>()
            {
                "Mci_Asp_Ver",
                "Mci_Model",
                "Mci_Firmware_Id",
                "Mci_Firmware_Ver_No",
                "Mci_Floor_No",
            };

            var actualMembers = _mciPropertyDataSource.Members;

            Assert.AreEqual(exptectedMembers.Count, actualMembers.Count);
            Assert.IsTrue(actualMembers.SequenceEqual(exptectedMembers));
        }

        [TestMethod]
        public void GetSetMembersTest()
        {
            string aspVersion = "mciAspV";
            byte model = 22;
            string firmwareId = "mciFid";
            string firmwareVerNo = "mciFVerNo";
            string floorNo = "mciFloorNo";

            _mciPropertyDataSource.SetMemberValue("Mci_Asp_Ver", aspVersion);
            _mciPropertyDataSource.SetMemberValue("Mci_Model", model);
            _mciPropertyDataSource.SetMemberValue("Mci_Firmware_Id", firmwareId);
            _mciPropertyDataSource.SetMemberValue("Mci_Firmware_Ver_No", firmwareVerNo);
            _mciPropertyDataSource.SetMemberValue("Mci_Floor_No", floorNo);

            string aspVersionActual = (string)_mciPropertyDataSource.GetMemberValue("Mci_Asp_Ver");
            byte modelActual = (byte)_mciPropertyDataSource.GetMemberValue("Mci_Model");
            string firmwareIdActual = (string)_mciPropertyDataSource.GetMemberValue("Mci_Firmware_Id");
            string firmwareVerNoActual = (string)_mciPropertyDataSource.GetMemberValue("Mci_Firmware_Ver_No");
            string floorNoActual = (string)_mciPropertyDataSource.GetMemberValue("Mci_Floor_No");

            Assert.AreEqual(aspVersionActual, aspVersion);
            Assert.AreEqual(modelActual, model);
            Assert.AreEqual(firmwareIdActual, firmwareId);
            Assert.AreEqual(firmwareVerNoActual, firmwareVerNo);
            Assert.AreEqual(floorNoActual, floorNo);
        }
    }
}