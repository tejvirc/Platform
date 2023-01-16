namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Data;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Kernel.Contracts.MessageDisplay;
    using Common;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class EventAcknowledgedQueueHelperTests
    {
        private const int TestId = 123;
        private EventAcknowledgedQueueHelper _target;
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);

        [TestInitialize]
        public void Initialize()
        {
            _target = new EventAcknowledgedQueueHelper(_unitOfWorkFactory.Object, _systemDisableManager.Object);
        }

        [DataRow(true, false, DisplayName = "UnitOfWorkFactory Null")]
        [DataRow(false, true, DisplayName = "SystemDisable Null")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullUnit,
            bool nullDisable)
        {
            _target = new EventAcknowledgedQueueHelper(
                nullUnit ? null : _unitOfWorkFactory.Object,
                nullDisable ? null : _systemDisableManager.Object);
        }

        [TestMethod]
        public void GetIdTest()
        {
            var message = new ReportEventMessage("1", DateTime.Now, TestId, 2);

            Assert.AreEqual(TestId, _target.GetId(message));
        }

        [TestMethod]
        public void AlmostFullDisableTest()
        {
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.EventQueueDisableKey,
                SystemDisablePriority.Normal,
                It.IsAny<string>(),
                It.IsAny<CultureProviderType>(),
                It.IsAny<object[]>()))
                .Verifiable();

            _target.AlmostFullDisable();

            _systemDisableManager.Verify(m => m.Disable(
                BingoConstants.EventQueueDisableKey,
                SystemDisablePriority.Normal,
                It.IsAny<string>(),
                It.IsAny<CultureProviderType>(),
                It.IsAny<object[]>()), Times.Once());
        }

        [TestMethod]
        public void AlmostFullClearTest()
        {
            _systemDisableManager.Setup(m => m.IsDisabled).Returns(true);
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.EventQueueDisableKey))
                .Verifiable();

            _target.AlmostFullClear();

            _systemDisableManager.Verify(m => m.Enable(
                BingoConstants.EventQueueDisableKey), Times.Once());
        }
    }
}