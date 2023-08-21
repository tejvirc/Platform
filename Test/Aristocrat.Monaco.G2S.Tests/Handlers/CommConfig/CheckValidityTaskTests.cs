namespace Aristocrat.Monaco.G2S.Tests.Handlers.CommConfig
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Aristocrat.G2S.Client.Devices.v21;
    using Data.CommConfig;
    using Data.Model;
    using G2S.Handlers.CommConfig;
    using G2S.Services;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Monaco.Common.Scheduler;
    using Monaco.Common.Storage;
    using Moq;

    [TestClass]
    public class CheckValidityTaskTests
    {
        private const long DefaultTransactionId = 1000;

        private readonly Mock<ICommChangeLogRepository> _commChangeLogRepository = new Mock<ICommChangeLogRepository>();
        private readonly Mock<IConfigurationService> _configuration = new Mock<IConfigurationService>();
        private readonly Mock<IMonacoContextFactory> _monacoContextFactory = new Mock<IMonacoContextFactory>();
        private readonly Mock<ITaskScheduler> _taskScheduler = new Mock<ITaskScheduler>();

        [TestInitialize]
        public void TestInitialize()
        {
            _commChangeLogRepository.Setup(x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), It.IsAny<long>()))
                .Verifiable();
        }

        [TestMethod]
        public void SerializationAndDeserializationTest()
        {
            var task = new CheckValidityTask(null, null, null, null)
            {
                TransactionId = DefaultTransactionId
            };

            var data = task.SerializeJobData();
            Assert.IsNotNull(data);

            var task2 = new CheckValidityTask(null, null, null, null);
            task2.DeserializeJobData(data);

            Assert.AreEqual(task.TransactionId, task2.TransactionId);
        }

        [TestMethod]
        public void WhenCreateExpectSuccess()
        {
            var task = CheckValidityTask.Create(DefaultTransactionId);

            Assert.AreEqual(task.TransactionId, DefaultTransactionId);
        }

        [TestMethod]
        public void WhenTimedOutExpectAborted()
        {
            _commChangeLogRepository
                .Setup(x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), It.IsAny<long>()))
                .Returns(
                    CreateChangeLog(AuthorizationState.Pending, TimeoutActionType.Abort, DateTime.UtcNow.AddDays(-1)))
                .Verifiable();

            var task = new CheckValidityTask(
                _commChangeLogRepository.Object,
                _monacoContextFactory.Object,
                _configuration.Object,
                _taskScheduler.Object)
            {
                TransactionId = DefaultTransactionId
            };

            task.Execute(null);

            _configuration.Verify(x => x.Abort(DefaultTransactionId, ChangeExceptionErrorCode.Timeout), Times.Once);
        }

        [TestMethod]
        public void WhenNormalCaseExpectNoAbort()
        {
            var log = CreateChangeLog(AuthorizationState.Pending, TimeoutActionType.Abort, DateTime.UtcNow.AddDays(1));
            _commChangeLogRepository
                .Setup(x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), It.IsAny<long>()))
                .Returns(log)
                .Verifiable();

            var task = new CheckValidityTask(
                _commChangeLogRepository.Object,
                _monacoContextFactory.Object,
                _configuration.Object,
                _taskScheduler.Object)
            {
                TransactionId = DefaultTransactionId
            };

            task.Execute(null);

            _configuration.Verify(x => x.Abort(1, ChangeExceptionErrorCode.Timeout), Times.Never);
        }

        [TestMethod]
        public void WhenNoChangeLogExpectNoAbort()
        {
            _commChangeLogRepository
                .Setup(x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), It.IsAny<long>()))
                .Returns((CommChangeLog)null)
                .Verifiable();

            var task = new CheckValidityTask(
                _commChangeLogRepository.Object,
                _monacoContextFactory.Object,
                _configuration.Object,
                _taskScheduler.Object)
            {
                TransactionId = DefaultTransactionId
            };

            task.Execute(null);

            _configuration.Verify(x => x.Abort(1, ChangeExceptionErrorCode.Timeout), Times.Never);
        }

        [TestMethod]
        public void WhenChangeLogNotFoundExpectReturn()
        {
            var task = new CheckValidityTask(
                _commChangeLogRepository.Object,
                _monacoContextFactory.Object,
                _configuration.Object,
                _taskScheduler.Object)
            {
                TransactionId = DefaultTransactionId,
            };

            task.Execute(null);

            _configuration.Verify(x => x.Abort(1, ChangeExceptionErrorCode.Timeout), Times.Never);
        }

        [TestMethod]
        public void WhenHandleCommandInNormalFlowExpectSuccess()
        {
            var commConfigDeviceMock = new Mock<ICommConfigDevice>();
            commConfigDeviceMock.SetupGet(x => x.Id).Returns(3);

            _commChangeLogRepository
                .Setup(x => x.GetPendingByTransactionId(It.IsAny<DbContext>(), DefaultTransactionId))
                .Returns(
                    CreateChangeLog(AuthorizationState.Pending, TimeoutActionType.Ignore, DateTime.UtcNow.AddDays(-1)));

            var task = new CheckValidityTask(
                _commChangeLogRepository.Object,
                _monacoContextFactory.Object,
                _configuration.Object,
                _taskScheduler.Object)
            {
                TransactionId = DefaultTransactionId,
            };

            task.Execute(null);

            _configuration.Verify(x => x.Authorize(It.IsAny<long>(), It.IsAny<int>(), true));
        }

        private CommChangeLog CreateChangeLog(
            AuthorizationState authorizeStatus,
            TimeoutActionType actionType,
            DateTime? authorizeTime = null)
        {
            return new CommChangeLog
            {
                DeviceId = 3,
                AuthorizeItems = new[]
                {
                    new ConfigChangeAuthorizeItem
                    {
                        Id = 1,
                        AuthorizeStatus = authorizeStatus,
                        TimeoutDate = authorizeTime ?? DateTime.MinValue,
                        TimeoutAction = actionType
                    }
                }
            };
        }
    }
}