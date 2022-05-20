namespace Aristocrat.Monaco.G2S.Tests.Handlers.OptionConfig
{
    using System;
    using System.Data.Entity;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.Model;
    using Data.OptionConfig;
    using G2S.Handlers.OptionConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class CheckOptionChangeStatusTests
    {
        private const long TransactionId = 10;

        private const long AuthorizeItemId = 20;

        private const int DeviceId = 30;

        private Mock<IConfigurationService> _configurationMock;

        private Mock<IMonacoContextFactory> _contextFactoryMock;

        private Mock<IG2SEgm> _egmMock;

        private Mock<IEventLift> _eventLiftMock;

        private Mock<IOptionChangeLogRepository> _optionChangeLogRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _egmMock = new Mock<IG2SEgm>();
            _optionChangeLogRepositoryMock = new Mock<IOptionChangeLogRepository>();
            _eventLiftMock = new Mock<IEventLift>();
            _contextFactoryMock = new Mock<IMonacoContextFactory>();
            _configurationMock = new Mock<IConfigurationService>();
        }

        [TestMethod]
        public void WhenCreateCheckOptionChangeStatusExpectSuccess()
        {
            var checkOptionChangeStatus = CheckOptionChangeStatus.Create(TransactionId, AuthorizeItemId);

            Assert.AreEqual(TransactionId, checkOptionChangeStatus.TransactionId);
            Assert.AreEqual(AuthorizeItemId, checkOptionChangeStatus.AuthorizeItemId);
        }

        [TestMethod]
        public void WhenSerializeJobDataExpectSuccess()
        {
            var checkOptionChangeStatus = CheckOptionChangeStatus.Create(TransactionId, AuthorizeItemId);

            var serializedData = checkOptionChangeStatus.SerializeJobData();

            Assert.AreEqual($"{TransactionId};{AuthorizeItemId}", serializedData);
        }

        [TestMethod]
        public void WhenDeserializeJobDataExpectSuccess()
        {
            var checkOptionChangeStatus = CheckOptionChangeStatus.Create(TransactionId, AuthorizeItemId);

            checkOptionChangeStatus.DeserializeJobData($"{TransactionId + 1};{AuthorizeItemId + 1}");

            Assert.AreEqual(TransactionId + 1, checkOptionChangeStatus.TransactionId);
            Assert.AreEqual(AuthorizeItemId + 1, checkOptionChangeStatus.AuthorizeItemId);
        }

        [TestMethod]
        public void WhenExecuteExpectNoAbortChangeLog()
        {
            var deviceMock = new Mock<IOptionConfigDevice>();
            _egmMock.Setup(m => m.GetDevice<IOptionConfigDevice>(DeviceId))
                .Returns(deviceMock.Object);

            var changeLog = CreateOptionChangeLog();
            changeLog.ChangeStatus = ChangeStatus.Authorized;
            _optionChangeLogRepositoryMock
                .Setup(m => m.GetByTransactionId(It.IsAny<DbContext>(), TransactionId))
                .Returns(changeLog);

            var checkOptionChangeStatus = new CheckOptionChangeStatus(
                _optionChangeLogRepositoryMock.Object,
                _eventLiftMock.Object,
                _egmMock.Object,
                _configurationMock.Object,
                _contextFactoryMock.Object)
            {
                TransactionId = TransactionId,
                AuthorizeItemId = AuthorizeItemId
            };

            checkOptionChangeStatus.Execute(new TaskSchedulerContext());

            _eventLiftMock.Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void WhenExecuteChangeLogNotExistsExpectNoAbort()
        {
            _optionChangeLogRepositoryMock
                .Setup(m => m.GetByTransactionId(It.IsAny<DbContext>(), TransactionId))
                .Returns((OptionChangeLog)null);

            var checkOptionChangeStatus = new CheckOptionChangeStatus(
                _optionChangeLogRepositoryMock.Object,
                _eventLiftMock.Object,
                _egmMock.Object,
                _configurationMock.Object,
                _contextFactoryMock.Object)
            {
                TransactionId = TransactionId,
                AuthorizeItemId = AuthorizeItemId
            };

            checkOptionChangeStatus.Execute(new TaskSchedulerContext());

            _eventLiftMock.Verify(m => m.Report(It.IsAny<IDevice>(), It.IsAny<string>()), Times.Never);
        }

        private OptionChangeLog CreateOptionChangeLog()
        {
            return new OptionChangeLog
            {
                DeviceId = DeviceId,
                TransactionId = TransactionId,
                ChangeStatus = ChangeStatus.Pending,
                AuthorizeItems = new[]
                {
                    new ConfigChangeAuthorizeItem
                    {
                        Id = AuthorizeItemId,
                        TimeoutDate = DateTime.MinValue
                    }
                }
            };
        }
    }
}