namespace Aristocrat.Monaco.Asp.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Aristocrat.Monaco.Asp.Client.DataSources;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Kernel;
    using Asp.Client.Contracts;
    using Asp.Client.Devices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    public class TestDataContext
    {
        public IParameterFactory ParameterFactory { get; set; }
        public Mock<IDataSource> DataSourceMock { get; set; }
        public Mock<IParameterLoadActions> DataSourceParameterLoadActionMock { get; set; }
        public Mock<IDataSourceRegistry> DataSourceRegistryMock { get; set; }
        public Mock<ITransaction> DataSourceTransactionMock { get; set; }
        public Mock<IDisposableDataSource> DisposableDataSourceMock { get; set; }
        public Mock<GameStatusDataSource> TransactionAsDataSourceMock { get; set; }
        public MockRepository MockRepository { get; set; }
        public List<string> DataMemberNames { get; set; } = new List<string> { "DM1", "DM2" };
    }

    public class AspUnitTestBase<T> where T : TestDataContext, new()
    {
        protected const int DefaultDeviceClass = 2;
        protected const int DefaultDeviceType = 1;
        protected const int DefaultParameter = 1;

        protected T TestData { get; set; } = new T();
        protected IParameterFactory ParameterFactory => TestData.ParameterFactory;
        protected Mock<IDataSource> DataSourceMock => TestData.DataSourceMock;
        protected Mock<GameStatusDataSource> TransactionDataSourceMock => TestData.TransactionAsDataSourceMock;
        protected Mock<ITransaction> DataSourceTransactionMock => TestData.DataSourceTransactionMock;
        protected Mock<IDisposableDataSource> DisposableDataSourceMock => TestData.DisposableDataSourceMock;
        protected Mock<IParameterLoadActions> DataSourceParameterLoadAction => TestData.DataSourceParameterLoadActionMock;
        protected Mock<IDataSourceRegistry> DataSourceRegistryMock => TestData.DataSourceRegistryMock;
        protected MockRepository MockRepository => TestData.MockRepository;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            TestData = new T { MockRepository = new MockRepository(MockBehavior.Strict) };
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            MockRepository.VerifyAll();
            TestData = null;
        }

        protected void SetupDataSource(bool implementsParameterLoadAction = false)
        {
            TestData.DataSourceMock = MockRepository.Create<IDataSource>();
            TestData.DataSourceRegistryMock = MockRepository.Create<IDataSourceRegistry>();

            if (implementsParameterLoadAction)
            {
                TestData.DataSourceParameterLoadActionMock = DataSourceMock.As<IParameterLoadActions>();
                TestData.DataSourceParameterLoadActionMock.Setup(m => m.PreLoad());
            }
            DataSourceRegistryMock.Setup(x => x.GetDataSource("DS1")).Returns(DataSourceMock.Object);

            TestData.DataSourceMock.SetupGet(x => x.Members).Returns(TestData.DataMemberNames);
        }

        protected void SetupTransactionDataSource()
        {
            TestData.DataMemberNames = new List<string> { "Status" };
            TestData.DataSourceMock = MockRepository.Create<IDataSource>();
            TestData.TransactionAsDataSourceMock = MockRepository.Create<GameStatusDataSource>(
                                                                        new Mock<IGameStatusProvider>().Object
                                                                       ,new Mock<IEventBus>().Object
                                                                       ,new Mock<ISystemDisableManager>().Object
                                                                       ,new Mock<IGamePlayState>().Object);
            TestData.DataSourceTransactionMock = TestData.TransactionAsDataSourceMock.As<ITransaction>();
            TestData.DisposableDataSourceMock = TestData.TransactionAsDataSourceMock.As<IDisposableDataSource>();

            DisposableDataSourceMock.SetupGet(x => x.Members).Returns(TestData.DataMemberNames);
            DataSourceTransactionMock.Setup(x => x.Begin(It.IsAny<List<string>>()));
            DataSourceTransactionMock.Setup(x => x.Commit());

            DisposableDataSourceMock.Setup(x => x.SetMemberValue("Status", It.IsAny<byte>()));
            TestData.DataSourceRegistryMock = MockRepository.Create<IDataSourceRegistry>();

            DataSourceRegistryMock.Setup(x => x.GetDataSource("TestStatus")).Returns(TestData.TransactionAsDataSourceMock.Object);

        }

        protected void SetupDataSourceForMaskOperations()
        {
            TestData.DataSourceMock = MockRepository.Create<IDataSource>();
            TestData.DataSourceRegistryMock = MockRepository.Create<IDataSourceRegistry>();

            DataSourceRegistryMock.Setup(x => x.GetDataSource("DS1")).Returns(DataSourceMock.Object);
        }

        protected void SetupDataSourceDummyDataSource()
        {
            TestData.DataSourceRegistryMock = MockRepository.Create<IDataSourceRegistry>();
            DataSourceRegistryMock.Setup(x => x.GetDataSource(It.IsAny<string>())).Returns(new DummyDataSource());
        }

        protected void SetupParameterFactory()
        {
            TestData.ParameterFactory = new ParameterFactory(
                new ProtocolSettings
                {
                    ProtocolVariation = "TEST",
                    DeviceDefinitionFile = @"Client\Devices\TestDevices.xml"
                },
                DataSourceRegistryMock.Object);
        }

        protected void SetupParameterFactoryForTransaction()
        {
            TestData.ParameterFactory = new ParameterFactory(
                new ProtocolSettings
                {
                    ProtocolVariation = "TEST",
                    DeviceDefinitionFile = @"Client\Devices\TestDeviceTransactionDataSource.xml"
                },
                DataSourceRegistryMock.Object);
        }

        protected static bool WaitFor(int milliseconds, Func<bool> condition)
        {
            var watch = new Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < milliseconds && !condition())
            {
                Thread.Yield();
            }

            watch.Stop();
            return condition();
        }

        protected static long GetWaitTimeFor(int maxWaitMilliseconds, Func<bool> condition)
        {
            var watch = new Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < maxWaitMilliseconds && !condition())
            {
                Thread.Yield();
            }

            watch.Stop();
            if (condition())
            {
                return watch.ElapsedMilliseconds;
            }

            return -1;
        }
    }
}