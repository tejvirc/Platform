namespace Aristocrat.Monaco.Asp.Tests.Client.Devices
{
    using System;
    using System.Linq;
    using Asp.Client.Contracts;
    using Asp.Client.Devices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ParameterFactoryTests : AspUnitTestBase<TestDataContext>
    {
        private IParameterFactory CreateFactory()
        {
            SetupDataSource();
            SetupParameterFactory();
            return ParameterFactory;
        }

        private IParameterFactory CreateParameterFactoryWithDummySource()
        {
            SetupDataSourceDummyDataSource();
            SetupParameterFactory();
            return ParameterFactory;
        }

        [TestMethod]
        public void ParameterFactoryTest()
        {
            Assert.ThrowsException<NullReferenceException>(
                () =>
                {
                    var factory = new ParameterFactory(
                        new ProtocolSettings { ProtocolVariation = "TEST", DeviceDefinitionFile = "NonExistent.File" },
                        null);
                }
            );
            Assert.IsNotNull(CreateFactory());
        }

        [TestMethod]
        public void InvalidDataMemberTest()
        {
            TestData.DataMemberNames.Clear();
            Assert.ThrowsException<Exception>(CreateFactory);
        }

        [TestMethod]
        public void CreateTestParameterFactoryOnlyCreateDummySource()
        {
            var factory = CreateParameterFactoryWithDummySource();
            var param = factory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);

            foreach (var field in param.Prototype.FieldsPrototype)
            {
                Assert.AreEqual(field.DataSource.Name, "DummyDataSource");
                Assert.AreEqual(field.DefaultValue, Convert.ToString(field.DataSource.GetMemberValue(field.DataMemberName)));
            }
        }

        [TestMethod]
        public void DummySourceParameterFactoryCreateTests()
        {
            var factory = CreateParameterFactoryWithDummySource();
            var param = factory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);

            Assert.IsNotNull(param);
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(null, null, null));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(null, param.TypeId, param));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(param.ClassId, null, param));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(param.ClassId, param.TypeId, null));

            Assert.ThrowsException<NullReferenceException>(() => factory.Create(-1, -1, -1));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(-1, param.TypeId.Id, param.Id));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(param.ClassId.Id, -1, param.Id));
            Assert.ThrowsException<ArgumentNullException>(() => factory.Create(param.ClassId.Id, param.TypeId.Id, -1));
        }

        [TestMethod]
        public void CreateTest()
        {
            var factory = CreateFactory();
            var param = factory.Create(DefaultDeviceClass, DefaultDeviceType, DefaultParameter);
            Assert.IsNotNull(param);
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(null, null, null));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(null, param.TypeId, param));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(param.ClassId, null, param));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(param.ClassId, param.TypeId, null));

            Assert.ThrowsException<NullReferenceException>(() => factory.Create(-1, -1, -1));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(-1, param.TypeId.Id, param.Id));
            Assert.ThrowsException<NullReferenceException>(() => factory.Create(param.ClassId.Id, -1, param.Id));
            Assert.ThrowsException<ArgumentNullException>(() => factory.Create(param.ClassId.Id, param.TypeId.Id, -1));
        }

        [TestMethod]
        public void ExistsTest()
        {
            var factory = CreateFactory();
            Assert.AreEqual(
                (true, true, true),
                factory.Exists(DefaultDeviceClass, DefaultDeviceType, DefaultParameter));
            Assert.AreEqual((true, true, false), factory.Exists(DefaultDeviceClass, DefaultDeviceType, -1));
            Assert.AreEqual((true, false, false), factory.Exists(DefaultDeviceClass, -1, -1));
            Assert.AreEqual((false, false, false), factory.Exists(-1, -1, -1));
        }

        [TestMethod]
        public void SelectParameterPrototypesTest()
        {
            var factory = CreateFactory();
            Assert.AreEqual(0, factory.SelectParameterPrototypes(x => x.Id == -1).Count);
            Assert.AreEqual(
                1,
                factory.SelectParameterPrototypes(
                    x => x.Id == DefaultParameter && x.ClassId.Id == DefaultDeviceClass &&
                         x.TypeId.Id == DefaultDeviceType).Count);
        }

        [TestMethod]
        public void AggregatedParameterTest()
        {
            var factory = CreateFactory();
            var aggregatedParameters = factory.SelectParameterPrototypes(x => x.Id == 0);
            foreach (var aggregatedParameter in aggregatedParameters)
            {
                var parameters = factory.SelectParameterPrototypes(
                        x =>
                            x.ClassId == aggregatedParameter.ClassId && x.TypeId == aggregatedParameter.TypeId &&
                            x.Id != 0)
                    .ToList();
                parameters.Sort((x, y) => x.Id.CompareTo(y.Id));
                var fields = parameters.SelectMany(x => x.FieldsPrototype).ToList().AsReadOnly();
                Assert.IsTrue(fields.SequenceEqual(aggregatedParameter.FieldsPrototype));
            }
        }

        [TestMethod]
        public void MainDeviceClassTest()
        {
            var factory = CreateFactory();
            var mainDeviceParameters =
                factory.SelectParameterPrototypes(x => x.ClassId.Id == 1 && x.TypeId.Id == 1 && x.Id != 0 && x.Id != 1);
            var allClassDevices = factory.SelectParameterPrototypes(x => true)
                .Select(x => (DeviceClass: x.ClassId.Id, DeviceType: x.TypeId.Id))
                .Distinct().ToList();
            Assert.AreEqual(allClassDevices.Count, mainDeviceParameters.Count);
            var uniqueClasses = allClassDevices.Select(x => x.DeviceClass).Distinct().ToList();
            var numberOfDevicesParameter = factory.Create(
                1,
                1,
                1);
            Assert.AreEqual(allClassDevices.Count, Convert.ToInt32(numberOfDevicesParameter.Fields[0].Value));
            foreach (var mainDeviceParameter in mainDeviceParameters)
            {
                var parameter = factory.Create(
                    mainDeviceParameter.ClassId,
                    mainDeviceParameter.TypeId,
                    mainDeviceParameter);
                Assert.IsTrue(
                    allClassDevices.Remove(
                        (Convert.ToInt32(parameter.Fields[0].Value),
                            Convert.ToInt32(parameter.Fields[1].Value))));
            }

            Assert.AreEqual(0, allClassDevices.Count);
        }

        [TestMethod]
        public void DeviceClassVersionTest()
        {
            var factory = CreateFactory();

            // Version is defined in TestDevices.xml
            foreach (var item in factory.DeviceDefinition.DeviceClasses)
            {
                if (item.Name.Equals("ParameterProcessorTest"))
                {
                    Assert.AreEqual(item.Version, "2.0");
                }
                else
                {
                    Assert.AreEqual(item.Version, "1.0");
                }
            }
        }
    }
}