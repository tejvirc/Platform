namespace Aristocrat.Monaco.Asp.Tests.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Asp.Client.Contracts;
    using Asp.Client.Utilities;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using static Aristocrat.Monaco.Asp.Client.Devices.Parameter;

    [TestClass]
    public class ParameterTests : AspUnitTestBase<TestDataContext>
    {
        private IParameter CreateParameter(bool implementsParameterLoadAction = false)
        {
            SetupDataSource(implementsParameterLoadAction);
            SetupParameterFactory();
            return ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);
        }

        private IParameter CreateParameterWithTransactionDataSource()
        {
            SetupTransactionDataSource();
            SetupParameterFactoryForTransaction();
            return ParameterFactory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);
        }

        /// <summary>
        ///     &lt;Parameter EGMAccess="ReadWrite" EventAccess="Never" ID="1" MCIAccess="ReadOnly" Name="ReadWrite"&gt;
        ///     &lt;Field DataMemberName="DM1" DataSourceName="DS1" ID="1" Name="Test" Size="1"
        ///     Type="BYTE" Value="0" /&gt;
        ///     &lt;/Parameter&gt;
        /// </summary>
        [TestMethod]
        public void ParameterTest()
        {
            var parameter = CreateParameter();
            Assert.AreSame(parameter.Name, parameter.Prototype.Name);
            Assert.AreSame(parameter.ClassId, parameter.Prototype.ClassId);
            Assert.AreSame(parameter.TypeId, parameter.Prototype.TypeId);
            Assert.AreEqual(parameter.SizeInBytes, parameter.Prototype.SizeInBytes);
            Assert.AreNotSame(parameter.Fields, parameter.Prototype.FieldsPrototype);
            Assert.AreEqual(parameter.EventAccessType, parameter.Prototype.EventAccessType);
            Assert.AreEqual(parameter.EgmAccessType, parameter.Prototype.EgmAccessType);
            Assert.AreEqual(parameter.MciAccessType, parameter.Prototype.MciAccessType);
            Assert.AreEqual(parameter.Id, parameter.Prototype.Id);

            Assert.AreEqual(parameter.EventAccessType, EventAccessType.Never);
            Assert.AreEqual(parameter.EgmAccessType, AccessType.ReadWrite);
            Assert.AreEqual(parameter.MciAccessType, AccessType.ReadOnly);

            Assert.AreEqual(DefaultParameter, parameter.Id);
            Assert.AreEqual(DefaultDeviceClass, parameter.ClassId.Id);
            Assert.AreEqual(DefaultDeviceType, parameter.TypeId.Id);

            Assert.AreEqual(parameter.Name, "ReadWrite");
            Assert.AreEqual(parameter.ClassId.Name, "ParameterProcessorTest");
            Assert.AreEqual(parameter.TypeId.Name, "EGMAccess");

            Assert.AreEqual(1, parameter.Fields.Count);
            Assert.AreEqual("Test", parameter.Fields[0].Name);
        }

        [TestMethod]
        public void ReadBytesTest()
        {
            byte[] data = { 3 };
            var reader = new ByteArrayReader(data);
            var parameter = CreateParameter();
            parameter.ReadBytes(reader);
            Assert.AreEqual((byte)3, (byte)parameter.Fields[0].Value);
            Assert.ThrowsException<InvalidOperationException>(() => parameter.ReadBytes(reader));
            data = new byte[0];
            reader.Reset(data);
            Assert.ThrowsException<InvalidOperationException>(() => parameter.ReadBytes(reader));
        }

        [TestMethod]
        public void WriteBytesTest()
        {
            byte[] data = { 3 };
            var writer = new ByteArrayWriter(data);
            var parameter = CreateParameter();
            parameter.Fields[0].Value = (byte)5;
            parameter.WriteBytes(writer);
            Assert.AreEqual((byte)5, data[0]);
            Assert.ThrowsException<IndexOutOfRangeException>(() => parameter.WriteBytes(writer));
            data = new byte[0];
            writer.Reset(data);
            Assert.ThrowsException<IndexOutOfRangeException>(() => parameter.WriteBytes(writer));
        }

        [TestMethod]
        public void LoadTest()
        {
            var parameter = CreateParameter();
            DataSourceMock.SetupSequence(x => x.GetMemberValue("DM1")).Returns(1).Returns(1.123).Returns('a')
                .Returns("123").Returns("xyz");
            parameter.Load();
            Assert.AreEqual((byte)1, (byte)parameter.Fields[0].Value);

            parameter.Load();
            Assert.AreEqual((byte)1.123, (byte)parameter.Fields[0].Value);

            parameter.Load();
            Assert.AreEqual((byte)'a', (byte)parameter.Fields[0].Value);

            parameter.Load();
            Assert.AreEqual(Convert.ToByte("123"), (byte)parameter.Fields[0].Value);

            Assert.ThrowsException<FormatException>(() => parameter.Load());

            parameter = CreateParameter(implementsParameterLoadAction: true);

            DataSourceMock.SetupSequence(x => x.GetMemberValue("DM1")).Returns(11).Returns(22);
            parameter.Load();
            parameter.Load();

            DataSourceParameterLoadAction.Verify(m => m.PreLoad(), Times.Exactly(2));
        }

        [TestMethod]
        public void SetFieldsTest()
        {
            var parameter = CreateParameter();
            parameter.Fields[0].Value = 1;

            var emptyMemberList = new Dictionary<string, object>();
            Assert.ThrowsException<MissingDataMembersException>(() => parameter.SetFields(emptyMemberList));

            var newMemberValue = new Dictionary<string, object> { { "DM1", 234 } };
            parameter.SetFields(newMemberValue);
            Assert.AreEqual((byte)234, (byte)parameter.Fields[0].Value);
        }

        [TestMethod]
        public void SaveTest()
        {
            var parameter = CreateParameter();
            DataSourceMock.Setup(x => x.SetMemberValue("DM1", (byte)6));
            parameter.Fields[0].Value = "6";
            parameter.Save();
            DataSourceMock.Setup(x => x.SetMemberValue("DM1", (byte)6)).Throws<InvalidOperationException>();
            Assert.ThrowsException<InvalidOperationException>(() => parameter.Save());
            DataSourceMock.Verify(x => x.SetMemberValue("DM1", (byte)6), Times.Exactly(2));
        }

        [TestMethod]
        public void SaveTestWithDataSourceAsTransaction()
        {
            var parameter = CreateParameterWithTransactionDataSource();

            parameter.Save();
            DisposableDataSourceMock.Verify(x => x.SetMemberValue("Status", (byte)0), Times.Once);
            DataSourceTransactionMock.Verify(x => x.Begin(new List<string>() { "Status" }), Times.Once);
            DataSourceTransactionMock.Verify(x => x.Commit(), Times.Once);
        }

        [TestMethod]
        public void ToStringTest()
        {
            var parameter = CreateParameter();
            Assert.AreEqual("Name:ReadWrite, Fields:[Test:0]", parameter.ToString());
            Debug.WriteLine(parameter.ToString());
        }
    }
}