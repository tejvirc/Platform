namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Hardware.Contracts.Door;
    using Aristocrat.Monaco.Hardware.Contracts.Persistence;
    using Asp.Client.Contracts;
    using Asp.Client.DataSources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class DataSourceRegistryTests : AspUnitTestBase<TestDataContext>
    {
        [TestMethod]
        public void DataSourceRegistryTest()
        {
            var dataSourceRegistry = new DataSourceRegistry(new List<IDataSource>());
            Assert.IsInstanceOfType(dataSourceRegistry.GetDataSource("DS1"), typeof(DummyDataSource));
            var dataSourceMock = MockRepository.Create<IDataSource>();
            dataSourceMock.SetupSequence(x => x.Name).Returns("DS1").Returns("DS2");
            dataSourceRegistry = new DataSourceRegistry(
                new List<IDataSource> { dataSourceMock.Object, dataSourceMock.Object });
            Assert.AreSame(dataSourceMock.Object, dataSourceRegistry.GetDataSource("DS1"));
            Assert.AreSame(dataSourceMock.Object, dataSourceRegistry.GetDataSource("DS2"));
            Assert.IsInstanceOfType(dataSourceRegistry.GetDataSource("DS3"), typeof(DummyDataSource));
            dataSourceMock.Verify(x => x.Name, Times.AtLeast(2));

            Assert.ThrowsException<NullReferenceException>(() => new DataSourceRegistry(null));
        }

        [TestMethod]
        public void GetDataSourceTest()
        {
            var dataSourceMock = MockRepository.Create<IDataSource>();
            dataSourceMock.SetupSequence(x => x.Name).Returns("DS1").Returns("DS2");
            var dataSourceRegistry = new DataSourceRegistry(
                new List<IDataSource> { dataSourceMock.Object, dataSourceMock.Object });
            Assert.AreSame(dataSourceMock.Object, dataSourceRegistry.GetDataSource("DS1"));
            Assert.AreSame(dataSourceMock.Object, dataSourceRegistry.GetDataSource("DS2"));
            Assert.IsInstanceOfType(dataSourceRegistry.GetDataSource("DS3"), typeof(DummyDataSource));
        }

        [TestMethod]
        public void RegisterDataSourceTest()
        {
            var dataSourceMock = MockRepository.Create<IDataSource>();
            dataSourceMock.SetupSequence(x => x.Name).Returns("DS1").Returns("DS2");
            var dataSourceRegistry = new DataSourceRegistry(
                new List<IDataSource> { dataSourceMock.Object, dataSourceMock.Object });

            var dataSourceMock2 = MockRepository.Create<IDataSource>();
            dataSourceMock2.SetupSequence(x => x.Name).Returns("DS1").Returns("DS3");
            Assert.ThrowsException<ArgumentException>(
                () => dataSourceRegistry.RegisterDataSource(dataSourceMock2.Object));
            dataSourceRegistry.RegisterDataSource(dataSourceMock2.Object);
            Assert.AreSame(dataSourceMock2.Object, dataSourceRegistry.GetDataSource("DS3"));
        }

        [TestMethod]
        public void RegisterDataSourceWithDatasourceRegistryAttributeTest()
        {
            var doorServiceMock = new Mock<IDoorService>();
            var meterManagerMock = new Mock<IMeterManager>();
            var persistentStorageManagerMock = new Mock<IPersistentStorageManager>();
            var doorsDataSourceMock = new Mock<DoorsDataSource>(
                doorServiceMock.Object,
                meterManagerMock.Object,
                persistentStorageManagerMock.Object);

            var dataSourceRegistry = new DataSourceRegistry(
                new List<IDataSource> { doorsDataSourceMock.Object });

            var propertyInfo = doorsDataSourceMock.Object.GetType().GetProperties().
                Where(o => o.GetCustomAttribute(typeof(DatasourceRegistryAttribute)) is DatasourceRegistryAttribute
                && o.PropertyType == typeof(IDataSourceRegistry)).FirstOrDefault();
                
            Assert.AreSame(dataSourceRegistry, propertyInfo.GetValue(doorsDataSourceMock.Object));
        }
    }
}