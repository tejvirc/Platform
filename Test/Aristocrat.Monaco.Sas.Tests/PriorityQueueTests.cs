namespace Aristocrat.SasClient.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.Monaco.Protocol.Common.Storage.Repositories;
    using Aristocrat.Monaco.Sas.Exceptions;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Client;

    /// <summary>
    ///     This class contains tests for the SasPriorityExceptionQueue class
    /// </summary>
    
    [TestClass]
    public class PriorityQueueTests
    {
        private const int waitTimeout = 1000;
        private const int ReturnedQueueSize = 30;
        private const int ClientId = 3;
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Default);
        private readonly Mock<IRepository<ExceptionQueue>> _repository = new Mock<IRepository<ExceptionQueue>>(MockBehavior.Default);
        private SasPriorityExceptionQueue _target;

        [TestInitialize]
        public void Initialize()
        {
            // note that the queue size in the class is set to 25 elements. We pass in 30 to test the resize feature
            var exception = new GenericExceptionBuilder(GeneralExceptionCode.None);
            var persistedQueue = Enumerable.Range(0, ReturnedQueueSize).Select(i => exception).Cast<ISasExceptionCollection>().ToList();
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, ExceptionQueue>>())).Returns((ExceptionQueue)null);
            _unitOfWorkFactory.Setup(x => x.Create()).Returns(_unitOfWork.Object);
            _unitOfWork.Setup(x => x.Repository<ExceptionQueue>()).Returns(_repository.Object);
            _repository.Setup(x => x.Queryable()).Returns(Enumerable.Empty<ExceptionQueue>().AsQueryable());
            _target = new SasPriorityExceptionQueue(ClientId, _unitOfWorkFactory.Object, _exceptionHandler.Object, new SasClientConfiguration { LegacyHandpayReporting = true, DiscardOldestException = true, IsNoneValidation = false }, _eventBus.Object);

            // now remove the 30 elements we passed in thru persistence so the queue starts with nothing in it
            foreach (var _ in Enumerable.Range(0, ReturnedQueueSize))
            {
                _target.GetNextException();
                _target.ExceptionAcknowledged();
            }
        }

        [TestMethod]
        public void DisposeTest()
        {
            _exceptionHandler.Setup(m => m.RemoveExceptionQueue(It.IsAny<SasGroup>(), It.IsAny<SasPriorityExceptionQueue>())).Verifiable();

            // dispose twice to check all paths
            _target.Dispose();
            _target.Dispose();

            // depends on the configuration settings done in the Initialize method for the constructor.
            var expectedGroups = 1;
            _exceptionHandler.Verify(m => m.RemoveExceptionQueue(It.IsAny<SasGroup>(), It.IsAny<SasPriorityExceptionQueue>()), Times.Exactly(expectedGroups));
        }

        [TestMethod]
        public void TestNoExceptionsReturnsNone()
        {
            var actual = _target.GetNextException();
            Assert.AreEqual((byte)GeneralExceptionCode.None, actual.First());
        }

        [TestMethod]
        public void TestPendingExceptionsReturnsExceptionCode()
        {
            var expected = new GenericExceptionBuilder(GeneralExceptionCode.SlotDoorWasOpened);
            _target.QueueException(expected);

            var actual = _target.GetNextException();
            Assert.AreEqual(expected.First(), actual.First());

            _target.ExceptionAcknowledged();
            // queue should be empty and return None exception code
            actual = _target.GetNextException();
            Assert.AreEqual((byte)GeneralExceptionCode.None, actual.First());
        }

        [TestMethod]
        public void Test2PendingExceptionsReturnsExceptionCodesInRightOrder()
        {
            var expected1 = new GenericExceptionBuilder(GeneralExceptionCode.SlotDoorWasOpened);
            _target.QueueException(expected1);

            var expected2 = new GenericExceptionBuilder(GeneralExceptionCode.SlotDoorWasClosed);
            _target.QueueException(expected2);

            var actual = _target.GetNextException();
            Assert.AreEqual(expected1.First(), actual.First());

            _target.ExceptionAcknowledged();
            actual = _target.GetNextException();
            Assert.AreEqual(expected2.First(), actual.First());

            _target.ExceptionAcknowledged();
            // queue should be empty and return None exception code
            actual = _target.GetNextException();
            Assert.AreEqual((byte)GeneralExceptionCode.None, actual.First());
        }

        [TestMethod]
        public void TestPendingPriorityExceptionsReturnsExceptionCode()
        {
            var expected = GeneralExceptionCode.SystemValidationRequest;
            _target.QueuePriorityException(expected);

            var actual = _target.GetNextException();
            Assert.AreEqual(expected, actual.ExceptionCode);

            _target.ExceptionAcknowledged();
            // queue should be empty and return None exception code
            actual = _target.GetNextException();
            Assert.AreEqual((byte)GeneralExceptionCode.None, actual.First());
        }

        [TestMethod]
        public void TestMixingPriorityAndNormalExceptionsReturnsExceptionCodesInRightOrder()
        {
            // insert a normal priority exception first
            var expected1 = new GenericExceptionBuilder(GeneralExceptionCode.SlotDoorWasOpened);
            _target.QueueException(expected1);

            // add a 'higher priority exception. This should be reported before the normal priority exception
            var expected2 = GeneralExceptionCode.GameLocked;
            _target.QueuePriorityException(expected2);

            // expect the higher priority exception first
            var actual = _target.GetNextException();
            Assert.AreEqual(expected2, actual.ExceptionCode);

            _target.ExceptionAcknowledged();
            actual = _target.GetNextException();
            CollectionAssert.AreEquivalent(expected1, actual.ToList());

            _target.ExceptionAcknowledged();
            // queue should be empty and return None exception code
            actual = _target.GetNextException();
            Assert.AreEqual(GeneralExceptionCode.None, actual.ExceptionCode);
        }

        [TestMethod]
        public void Test2PendingPriorityExceptionsReturnsExceptionCodesInRightOrder()
        {
            // insert a 'lower' priority exception first
            var expected1 = GeneralExceptionCode.HandPayWasReset;
            _target.QueuePriorityException(expected1);

            // add a 'higher priority exception. This should be reported before the 'lower' priority exception
            var expected2 = GeneralExceptionCode.GameLocked;
            _target.QueuePriorityException(expected2);

            // expect the higher priority exception first
            var actual = _target.GetNextException();
            Assert.AreEqual(expected2, actual.ExceptionCode);

            _target.ExceptionAcknowledged();
            actual = _target.GetNextException();
            Assert.AreEqual(expected1, actual.ExceptionCode);

            _target.ExceptionAcknowledged();
            // queue should be empty and return None exception code
            actual = _target.GetNextException();
            Assert.AreEqual(GeneralExceptionCode.None, actual.ExceptionCode);
        }

        [TestMethod]
        public void PriorityQueueExceptionAcknowledgeClearsSentException()
        {
            const GeneralExceptionCode expectedException = GeneralExceptionCode.ValidationIdNotConfigured;
            const GeneralExceptionCode highPriorityException = GeneralExceptionCode.SystemValidationRequest;

            _target.QueuePriorityException(expectedException);
            var actualException = _target.GetNextException();
            Assert.AreEqual(expectedException, actualException.ExceptionCode);

            _target.QueuePriorityException(highPriorityException);
            _target.ExceptionAcknowledged();
            actualException = _target.GetNextException();
            Assert.AreEqual(highPriorityException, actualException.ExceptionCode);
        }

        [TestMethod]
        public void GettingNextExceptionFlagsForWaitingAcknowledgement()
        {
            const GeneralExceptionCode expectedException = GeneralExceptionCode.ValidationIdNotConfigured;
            const GeneralExceptionCode generalException = GeneralExceptionCode.SlotDoorWasOpened;

            _target.QueuePriorityException(expectedException);
            _target.QueueException(new GenericExceptionBuilder(generalException));

            Assert.AreEqual(expectedException, _target.GetNextException().ExceptionCode);

            // The exception has not been removed
            Assert.AreEqual(expectedException, _target.GetNextException().ExceptionCode);

            _target.ExceptionAcknowledged();
            _target.ExceptionAcknowledged(); // Make sure we still can get the general exception

            Assert.AreEqual(generalException, _target.GetNextException().ExceptionCode);

            _target.ClearPendingException();
            _target.ExceptionAcknowledged(); // We cleared so nothing should be acknowledged
            Assert.AreEqual(generalException, _target.GetNextException().ExceptionCode);

            Assert.AreEqual(generalException, _target.GetNextException().ExceptionCode);
            _target.ExceptionAcknowledged();

            Assert.AreEqual((byte)GeneralExceptionCode.None, _target.GetNextException().First());
        }

        [TestMethod]
        public void NoExceptionDoesNotFlagForWaitingAcknowledgement()
        {
            Assert.AreEqual((byte)GeneralExceptionCode.None, _target.GetNextException().First());
            const GeneralExceptionCode generalException = GeneralExceptionCode.SlotDoorWasOpened;
            _target.QueueException(new GenericExceptionBuilder(generalException));

            _target.ExceptionAcknowledged();
            Assert.AreEqual(generalException, _target.GetNextException().ExceptionCode);
        }

        [TestMethod]
        public void PriorityExceptionDoesGetClearedIfNormalExceptionIsRead()
        {
            const GeneralExceptionCode expectedException = GeneralExceptionCode.ValidationIdNotConfigured;
            const GeneralExceptionCode generalException = GeneralExceptionCode.SlotDoorWasOpened;

            _target.QueueException(new GenericExceptionBuilder(generalException));
            Assert.AreEqual(generalException, _target.GetNextException().ExceptionCode);

            _target.QueuePriorityException(expectedException);
            _target.ExceptionAcknowledged();

            Assert.AreEqual(expectedException, _target.GetNextException().ExceptionCode);
        }

        [TestMethod]
        public void TestFillingQueueAddsBufferOverflowException()
        {
            // add fake exceptions until we get the queue full indication
            byte i = 1;
            while (!_target.ExceptionQueueIsFull)
            {
                _target.QueueException(new GenericExceptionBuilder(GeneralExceptionCode.BillAccepted));
                i++;
            }

            // An ExceptionBufferOverflow priority exception should be automatically generated

            // add another exception and ensure that it drops the oldest entry (1 in this case)
            _target.QueueException(new GenericExceptionBuilder(GeneralExceptionCode.BillRejected));

            // expect the higher priority exception first
            var actual = _target.GetNextException();
            Assert.AreEqual(GeneralExceptionCode.ExceptionBufferOverflow, actual.ExceptionCode);
            _target.ExceptionAcknowledged();

            for (i = 1; i < 25; i++)
            {
                actual = _target.GetNextException();
                Assert.AreEqual(GeneralExceptionCode.BillAccepted, actual.ExceptionCode);
                _target.ExceptionAcknowledged();
            }

            // expect 100 to replace the original 1 entry
            actual = _target.GetNextException();
            Assert.AreEqual(GeneralExceptionCode.BillRejected, actual.ExceptionCode);

            _target.ExceptionAcknowledged();
            // queue should be empty and return None exception code
            actual = _target.GetNextException();
            Assert.AreEqual(GeneralExceptionCode.None, actual.ExceptionCode);
        }

        [TestMethod]
        public void TestAddingPriorityExceptionGetPulledFirst()
        {
            // queue a normal exception first
            var normalException = new GenericExceptionBuilder(GeneralExceptionCode.SlotDoorWasOpened);
            _target.QueueException(normalException);

            var priorityException = new GenericExceptionBuilder(GeneralExceptionCode.SystemValidationRequest);
            // queue priority exception before getting next
            _target.QueueException(priorityException);

            var actual = _target.GetNextException();
            CollectionAssert.AreEquivalent(priorityException, actual.ToList());

            _target.ExceptionAcknowledged();
            actual = _target.GetNextException();
            CollectionAssert.AreEquivalent(normalException, actual.ToList());

            _target.ExceptionAcknowledged();
            // queue should be empty and return None exception code
            actual = _target.GetNextException();
            Assert.AreEqual((byte)GeneralExceptionCode.None, actual.First());
        }

        [TestMethod]
        public void LegacyHandpayReportingHandpayExceptionPrioritiesTest()
        {
            _target = new SasPriorityExceptionQueue(ClientId, _unitOfWorkFactory.Object, _exceptionHandler.Object, new SasClientConfiguration { LegacyHandpayReporting = true }, _eventBus.Object);
            _target.QueuePriorityException(GeneralExceptionCode.HandPayIsPending);
            _target.QueuePriorityException(GeneralExceptionCode.HandPayWasReset);

            var actual = _target.GetNextException();
            CollectionAssert.AreEqual(new List<byte> { (byte)GeneralExceptionCode.HandPayIsPending }, actual.ToList());
            _target.ExceptionAcknowledged();
            actual = _target.GetNextException();

            CollectionAssert.AreEqual(new List<byte> { (byte)GeneralExceptionCode.HandPayWasReset }, actual.ToList());
        }

        [TestMethod]
        public void SecureHandpayReportingHandpayExceptionPrioritiesTest()
        {
            _target = new SasPriorityExceptionQueue(ClientId, _unitOfWorkFactory.Object, _exceptionHandler.Object, new SasClientConfiguration { LegacyHandpayReporting = false }, _eventBus.Object);
            _target.QueuePriorityException(GeneralExceptionCode.HandPayIsPending);
            _target.QueuePriorityException(GeneralExceptionCode.HandPayWasReset);

            var actual = _target.GetNextException();
            CollectionAssert.AreEqual(new List<byte> { (byte)GeneralExceptionCode.HandPayWasReset }, actual.ToList());
            _target.ExceptionAcknowledged();
            actual = _target.GetNextException();

            CollectionAssert.AreEqual(new List<byte> { (byte)GeneralExceptionCode.HandPayIsPending }, actual.ToList());
        }

        [TestMethod]
        public void RemovingPriorityExceptionTest()
        {
            var priorityException = new GenericExceptionBuilder(GeneralExceptionCode.SystemValidationRequest);
            var nonPriorityException = new GenericExceptionBuilder(GeneralExceptionCode.BillAccepted);
            _target.QueueException(priorityException);
            _target.QueueException(nonPriorityException);
            _target.RemoveException(priorityException);

            var actual = _target.GetNextException();
            CollectionAssert.AreEquivalent(nonPriorityException, actual.ToList());
        }

        [TestMethod]
        public void RemovingNormalExceptionTest()
        {
            var removingException = new GenericExceptionBuilder(GeneralExceptionCode.BillJam);
            var nonPriorityException = new GenericExceptionBuilder(GeneralExceptionCode.BillAccepted);
            _target.QueueException(removingException);
            _target.QueueException(nonPriorityException);
            _target.RemoveException(removingException);

            var actual = _target.GetNextException();
            CollectionAssert.AreEquivalent(nonPriorityException, actual.ToList());
        }

        [TestMethod]
        public void AddHandlerTest()
        {
            var waiter = new ManualResetEvent(false);
            var handlerRun = 1;

            // add twice, both will be added to list
            _target.AddHandler(GeneralExceptionCode.EgmPowerApplied, () => {
                handlerRun = 2;
                waiter.Set();
            });

            _target.AddHandler(GeneralExceptionCode.EgmPowerApplied, () =>
                {
                    handlerRun = 3;
                    waiter.Set();
                });

            // queue the exception twice, then get the exception to set up class flags
            _target.QueueException(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));
            _target.QueueException(new GenericExceptionBuilder(GeneralExceptionCode.EgmPowerApplied));
            var exception = _target.GetNextException();

            Assert.AreEqual(GeneralExceptionCode.EgmPowerApplied, exception.ExceptionCode);

            // invoke the handler
            _target.ExceptionAcknowledged();

            Assert.IsTrue(waiter.WaitOne(waitTimeout));

            // handler should be the one that sets value to 2
            Assert.AreEqual(2, handlerRun);

            // repeat for 2nd exception
            waiter.Reset();
            exception = _target.GetNextException();

            Assert.AreEqual(GeneralExceptionCode.EgmPowerApplied, exception.ExceptionCode);

            // invoke the handler
            _target.ExceptionAcknowledged();

            Assert.IsTrue(waiter.WaitOne(waitTimeout));

            // handler should be the one that sets value to 3
            Assert.AreEqual(3, handlerRun);
        }

        [TestMethod]
        public void PeekPriorityExceptionTest()
        {
            _target.QueuePriorityException(GeneralExceptionCode.SystemValidationRequest);

            var result = _target.Peek();
            Assert.AreEqual(GeneralExceptionCode.SystemValidationRequest, result.ExceptionCode);
        }

        [TestMethod]
        public void PeekNormalExceptionTest()
        {
            var peekException = new GenericExceptionBuilder(GeneralExceptionCode.BillJam);
            _target.QueueException(peekException);

            var result = _target.Peek();
            Assert.AreEqual(GeneralExceptionCode.BillJam, result.ExceptionCode);
        }

        [TestMethod]
        public void PeekNoExceptionTest()
        {
            var result = _target.Peek();
            Assert.AreEqual(GeneralExceptionCode.None, result.ExceptionCode);
        }

        [TestMethod]
        public void NoneValidationChangesPriorityDictionaryTest()
        {
            // set configuration for NONE validation
            var configuration = new SasClientConfiguration
            {
                LegacyHandpayReporting = true, DiscardOldestException = true, IsNoneValidation = true
            };

            // make the exception to queue one that normally is a priority exception
            var exception = new GenericExceptionBuilder(GeneralExceptionCode.CashOutTicketPrinted);

            _target = new SasPriorityExceptionQueue(ClientId, _unitOfWorkFactory.Object, _exceptionHandler.Object, configuration, _eventBus.Object);

            // now remove the 30 elements we passed in thru persistence so the queue starts with nothing in it
            foreach (var _ in Enumerable.Range(0, ReturnedQueueSize))
            {
                _target.GetNextException();
                _target.ExceptionAcknowledged();
            }

            Assert.IsFalse(_target.ExceptionQueueIsFull);

            // the queue is 25 items deep. Add 24 items and ensure the ExceptionQueueIsFull property is false
            for (var i = 0; i < 24; i++)
            {
                _target.QueueException(exception);
            }

            Assert.IsFalse(_target.ExceptionQueueIsFull);

            // adding one more exception should make the queue full
            _target.QueueException(exception);

            Assert.IsTrue(_target.ExceptionQueueIsFull);
        }
    }
}
