namespace Aristocrat.Monaco.Bingo.Tests.Services.Reporting
{
    using System;
    using System.Data;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Common;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class TransactionAcknowledgedQueueHelperTests
    {
        private const int TestId = 123;
        private TransactionAcknowledgedQueueHelper _target;
        private readonly Mock<ISystemDisableManager> _systemDisableManager = new(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);

        [TestInitialize]
        public void Initialize()
        {
            _target = new TransactionAcknowledgedQueueHelper(_unitOfWorkFactory.Object, _systemDisableManager.Object);
        }

        [DataRow(true, false, DisplayName = "UnitOfWorkFactory Null")]
        [DataRow(false, true, DisplayName = "SystemDisable Null")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullUnit,
            bool nullDisable)
        {
            _target = new TransactionAcknowledgedQueueHelper(
                nullUnit ? null : _unitOfWorkFactory.Object,
                nullDisable ? null : _systemDisableManager.Object);
        }

        [TestMethod]
        public void GetIdTest()
        {
            var message = new ReportTransactionMessage(
                "1", // machine serial
                DateTime.Now,
                0,  // amount
                0,  // game serial
                0,  // game title
                TestId,
                0,  // paytable id
                0,  // denomination id
                2);

            Assert.AreEqual(TestId, _target.GetId(message));
        }

        [TestMethod]
        public void AlmostFullDisableTest()
        {
            _systemDisableManager.Setup(m => m.Disable(
                BingoConstants.TransactionQueueDisableKey,
                SystemDisablePriority.Immediate,
                It.IsAny<Func<string>>(),
                null))
                .Verifiable();

            _target.AlmostFullDisable();

            _systemDisableManager.Verify(m => m.Disable(
                BingoConstants.TransactionQueueDisableKey,
                SystemDisablePriority.Immediate,
                It.IsAny<Func<string>>(),
                null), Times.Once());
        }

        [TestMethod]
        public void AlmostFullClearTest()
        {
            _systemDisableManager.Setup(m => m.IsDisabled).Returns(true);
            _systemDisableManager.Setup(m => m.Enable(BingoConstants.TransactionQueueDisableKey))
                .Verifiable();

            _target.AlmostFullClear();

            _systemDisableManager.Verify(m => m.Enable(
                BingoConstants.TransactionQueueDisableKey), Times.Once());
        }
    }
}