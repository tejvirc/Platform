namespace Aristocrat.Monaco.Sas.Tests.Exceptions
{
    using System;
    using Aristocrat.Sas.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Exceptions;

    [TestClass]
    public class SasExceptionHandlerTests
    {
        private readonly SasExceptionHandler _target = new SasExceptionHandler();
        private readonly Mock<ISasExceptionQueue> _queue1 = new Mock<ISasExceptionQueue>(MockBehavior.Strict);
        private readonly Mock<ISasExceptionQueue> _queue2 = new Mock<ISasExceptionQueue>(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _queue1.Setup(m => m.ClientNumber).Returns(1);
            _queue2.Setup(m => m.ClientNumber).Returns(2);
        }

        [TestMethod]
        public void AddHandlerTest()
        {
            _target.RegisterExceptionProcessor(SasGroup.Aft, _queue1.Object);
            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);

            _queue1.Setup(m => m.AddHandler(GeneralExceptionCode.AftTransferComplete, It.IsAny<Action>())).Verifiable();

            _target.AddHandler(GeneralExceptionCode.AftTransferComplete, () => {});

            _queue1.Verify(m => m.AddHandler(GeneralExceptionCode.AftTransferComplete, It.IsAny<Action>()), Times.Once);
        }

        [TestMethod]
        public void AddThenRemoveHandlerTest()
        {
            _target.RegisterExceptionProcessor(SasGroup.Aft, _queue1.Object);
            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);

            _queue1.Setup(m => m.AddHandler(GeneralExceptionCode.AftTransferComplete, It.IsAny<Action>())).Verifiable();
            _queue1.Setup(m => m.RemoveHandler(GeneralExceptionCode.AftTransferComplete)).Verifiable();

            _target.AddHandler(GeneralExceptionCode.AftTransferComplete, () => { });
            _target.RemoveHandler(GeneralExceptionCode.AftTransferComplete);

            _queue1.Verify(m => m.AddHandler(GeneralExceptionCode.AftTransferComplete, It.IsAny<Action>()), Times.Once);
            _queue1.Verify(m => m.RemoveHandler(GeneralExceptionCode.AftTransferComplete), Times.Once);
        }

        [TestMethod]
        public void GetExceptionQueueWhenNoQueuesHandleException()
        {
            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);

            _queue1.Setup(m => m.AddHandler(GeneralExceptionCode.AftTransferComplete, It.IsAny<Action>())).Verifiable();

            _target.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.AftTransferComplete));

            _queue1.Verify(m => m.AddHandler(GeneralExceptionCode.AftTransferComplete, It.IsAny<Action>()), Times.Never);
        }

        [TestMethod]
        public void GetExceptionQueueWhenQueuesHandleException()
        {
            var exception = new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied);

            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);
            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue2.Object);

            _queue1.Setup(m => m.QueueException(exception)).Verifiable();
            _queue2.Setup(m => m.QueueException(exception)).Verifiable();

            _target.ReportException(exception);

            _queue1.Verify(m => m.QueueException(exception), Times.Once);
            _queue2.Verify(m => m.QueueException(exception), Times.Once);
        }

        [TestMethod]
        public void RegisterThenRemoveExceptionQueue()
        {
            var exception = new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied);

            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);
            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue2.Object);

            // remove a queue
            _target.RemoveExceptionQueue(SasGroup.PerClientLoad, _queue2.Object);

            _queue1.Setup(m => m.QueueException(exception)).Verifiable();
            _queue2.Setup(m => m.QueueException(exception)).Verifiable();

            _target.ReportException(exception);

            _queue1.Verify(m => m.QueueException(exception), Times.Once);
            _queue2.Verify(m => m.QueueException(exception), Times.Never);
        }

        [TestMethod]
        public void ReportExceptionWithClientNumber()
        {
            var exception = new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied);

            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);

            _queue1.Setup(m => m.QueueException(exception)).Verifiable();
            _queue2.Setup(m => m.QueueException(exception)).Verifiable();

            _target.ReportException(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied), 1);

            _queue1.Verify(m => m.QueueException(exception), Times.Once);
            _queue2.Verify(m => m.QueueException(exception), Times.Never);
        }

        [TestMethod]
        public void RemoveExceptionWithClientNumber()
        {
            var exception = new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied);

            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);

            _queue1.Setup(m => m.RemoveException(exception)).Verifiable();
            _queue2.Setup(m => m.RemoveException(exception)).Verifiable();

            _target.RemoveException(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied), 1);

            _queue1.Verify(m => m.RemoveException(exception), Times.Once);
            _queue2.Verify(m => m.RemoveException(exception), Times.Never);
        }

        [TestMethod]
        public void RemoveException()
        {
            var exception = new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied);

            _target.RegisterExceptionProcessor(SasGroup.PerClientLoad, _queue1.Object);

            _queue1.Setup(m => m.RemoveException(exception)).Verifiable();
            _queue2.Setup(m => m.RemoveException(exception)).Verifiable();

            _target.RemoveException(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));

            _queue1.Verify(m => m.RemoveException(exception), Times.Once);
            _queue2.Verify(m => m.RemoveException(exception), Times.Never);
        }
    }
}
