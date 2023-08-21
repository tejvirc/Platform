namespace Aristocrat.Monaco.Asp.Tests.Client.Devices.Fields
{
    using System.Collections.Generic;
    using System.Linq;
    using Asp.Client.Contracts;
    using Asp.Client.Devices.Fields;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class FieldTests : AspUnitTestBase<TestDataContext>
    {
        private Mock<IFieldPrototype> _mockPrototype;

        private IField SetupField(bool setupDataSource = true)
        {
            _mockPrototype = MockRepository.Create<IFieldPrototype>();
            _mockPrototype.SetupGet(x => x.Type).Returns(FieldType.BYTE);
            _mockPrototype.SetupGet(x => x.DefaultValue).Returns("0");
            _mockPrototype.SetupGet(x => x.Masks).Returns(new List<IMask>());

            var field = FieldFactory.CreateField(_mockPrototype.Object);
            Assert.IsNotNull(field as ByteField);
            if (!setupDataSource)
            {
                return field;
            }

            SetupDataSource();

            _mockPrototype.SetupGet(x => x.DataMemberName).Returns("DM1");
            _mockPrototype.SetupGet(x => x.DataSource).Returns(DataSourceRegistryMock.Object.GetDataSource("DS1"));

            return field;
        }

        private IField SetupFieldForMaskTest()
        {
            _mockPrototype = MockRepository.Create<IFieldPrototype>();
            _mockPrototype.SetupGet(x => x.Type).Returns(FieldType.BYTE);
            _mockPrototype.SetupGet(x => x.DefaultValue).Returns("0");
            _mockPrototype.SetupGet(x => x.DataSourceName).Returns("TestDataSource");
            _mockPrototype.SetupGet(x => x.Masks).Returns(CreateTestMaskList());

            var field = FieldFactory.CreateField(_mockPrototype.Object);
            Assert.IsNotNull(field as ByteField);
            SetupDataSourceForMaskOperations();
            _mockPrototype.SetupGet(x => x.DataSource).Returns(DataSourceRegistryMock.Object.GetDataSource("DS1"));

            return field;
        }

        [TestMethod]
        public void LoadTest()
        {
            var field = SetupField();
            DataSourceMock.Setup(x => x.GetMemberValue("DM1")).Returns("5");
            field.Load();
            Assert.AreEqual((byte)5, field.Value);
            Assert.IsTrue(DataSourceMock.Object.Members.Any()); // To pass the Setup for Members
        }

        [TestMethod]
        public void SaveTest()
        {
            var field = SetupField();
            field.Value = 5;
            DataSourceMock.Setup(x => x.SetMemberValue("DM1", (byte)5));
            field.Save();
            Assert.IsTrue(DataSourceMock.Object.Members.Any()); // To pass the Setup for Members
        }

        [TestMethod]
        public void MaskLoadTest()
        {
            var field = SetupFieldForMaskTest();
            DataSourceMock.Setup(x => x.GetMemberValue("DM1")).Returns(true);

            field.Load();
            Assert.AreEqual((byte)1, field.Value);
            Assert.AreEqual("TestDataSource", field.DataSourceName);
            foreach (var mask in field.Masks)
            {
                Assert.AreEqual(mask.TrueText, "Test");
                Assert.AreEqual(mask.FalseText, "TestFalse");
            }

        }

        [TestMethod]
        public void MaskSaveTest()
        {
            var field = SetupFieldForMaskTest();
            field.Value = 5;
            DataSourceMock.Setup(x => x.SetMemberValue("DM1", true));
            field.Save();
            Assert.AreEqual("TestDataSource", field.DataSourceName);
            foreach (var mask in field.Masks)
            {
                Assert.AreEqual(mask.TrueText, "Test");
                Assert.AreEqual(mask.FalseText, "TestFalse");
            }
        }

        [TestMethod]
        public void WriteBytesTest()
        {
            var field = SetupField(false);
            field.Value = 5;
            var mockWriter = MockRepository.Create<IByteArrayWriter>();
            mockWriter.Setup(x => x.Write((byte)5));
            field.WriteBytes(mockWriter.Object);
        }

        [TestMethod]
        public void ReadBytesTest()
        {
            var field = SetupField(false);
            var mockWriter = MockRepository.Create<IByteArrayReader>();
            mockWriter.Setup(x => x.ReadByte()).Returns(5);
            field.ReadBytes(mockWriter.Object);
            Assert.AreEqual((byte)5, field.Value);
        }

        [TestMethod]
        public void ToStringTest()
        {
            var field = SetupField(false);
            _mockPrototype.SetupGet(x => x.Name).Returns("Test");
            field.Value = 5;
            Assert.AreEqual("Test:5", field.ToString());
        }

        private List<IMask> CreateTestMaskList()
        {
            var mask = new Mask();
            mask.MaskOperation = MaskOperation.Equal;
            mask.TrueText = "Test";
            mask.FalseText = "TestFalse";
            mask.Value = 1;
            mask.DataMemberName = "DM1";
            var maskList = new List<IMask>();
            maskList.Add(mask);
            maskList.Add(mask);
            return maskList;
        }
    }
}