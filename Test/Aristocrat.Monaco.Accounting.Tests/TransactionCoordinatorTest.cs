namespace Aristocrat.Monaco.Accounting.Tests
{
    using Contracts;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Test.Common;

    /// <summary>
    ///     This is a test class for TransactionCoordinatorTest and is intended
    ///     to contain all TransactionCoordinatorTest Unit Tests
    /// </summary>
    [TestClass]
    public class TransactionCoordinatorTest
    {
        private dynamic _accessor;
        private Mock<IPersistentStorageAccessor> _block;
        private Mock<IEventBus> _eventBus;
        private Mock<IPersistentStorageManager> _persistentStorage;

        private readonly Mock<IPersistentStorageTransaction> _storageTransaction =
            new Mock<IPersistentStorageTransaction>(MockBehavior.Strict);

        private TransactionCoordinator _target;
        private Mock<ITransactionCoordinator> _transactionCoordinator;

        /// <summary>
        ///     Code to run before each test
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());

            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Strict);
            _block = MoqServiceManager.CreateAndAddService<IPersistentStorageAccessor>(MockBehavior.Strict);
            _block.Setup(m => m["CurrentTransactionId"]).Returns(new Guid("{00000000-0000-0000-1234-000000000000}"));
            _block.Setup(m => m["CurrentRequestId"]).Returns(new Guid("{00000000-0000-0001-1234-000000000000}"));
            _block.Setup(m => m["CurrentTransactionRequestorId"])
                .Returns(new Guid("{00000000-0000-0002-1234-000000000000}"));
            _block.Setup(m => m["CurrentRequestorId"]).Returns(new Guid("{00000000-0000-0003-1234-000000000000}"));

            _persistentStorage = MoqServiceManager.CreateAndAddService<IPersistentStorageManager>(MockBehavior.Strict);
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(true);
            _persistentStorage.Setup(m => m.GetBlock(It.IsAny<string>())).Returns(_block.Object);

            _transactionCoordinator = MoqServiceManager.CreateAndAddService<ITransactionCoordinator>(MockBehavior.Strict);
            
            _target = new TransactionCoordinator();
            _accessor = new DynamicPrivateObject(_target);
            _target.Initialize();
        }

        /// <summary>
        ///     Cleans up after each test
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            MoqServiceManager.RemoveInstance();
            try
            {
                AddinManager.Shutdown();
            }
            catch (InvalidOperationException)
            {
                // temporarily swallow exception
            }
        }

        [TestMethod]
        public void ConstructorWhenBlockDoesNotExistsTest()
        {
            _persistentStorage.Setup(m => m.BlockExists(It.IsAny<string>())).Returns(false);
            _persistentStorage.Setup(m => m.CreateBlock(PersistenceLevel.Transient, It.IsAny<string>(), 1))
                .Returns(_block.Object);

            _target = new TransactionCoordinator();

            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void PersistAndConfirmTest()
        {
            var requestorGuid = new Guid("f1bc6cac-6096-42ae-b5ec-9edaf9de7b82");
            _storageTransaction.SetupSet(m => m["CurrentRequestorId"] = requestorGuid).Verifiable();
            _accessor.PersistGuid(_storageTransaction.Object, "CurrentRequestorId", requestorGuid);
        }

        [TestMethod]
        public void ServiceTypeTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(ITransactionCoordinator)));
        }

        [TestMethod]
        public void NameTest()
        {
            var actual = _target.Name;
            var expected = "TransactionCoordinator";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsTransactionActiveTest()
        {
            Assert.IsTrue(_target.IsTransactionActive);
        }

        [TestMethod]
        public void VerifyCurrentTransactionTestTrue()
        {
            var expected = Guid.NewGuid();

            _accessor._currentTransactionId = expected;

            Assert.IsTrue(_target.VerifyCurrentTransaction(expected));
        }

        [TestMethod]
        public void VerifyCurrentTransactionTestFalse()
        {
            var expected = Guid.NewGuid();

            _accessor._currentTransactionId = expected;

            // Test functionality results
            Assert.IsFalse(_target.VerifyCurrentTransaction(Guid.NewGuid()));
        }

        [TestMethod]
        public void VerifyEmptyCurrentTransactionTestFalse()
        {
            var expected = Guid.Empty;

            _accessor._currentTransactionId = expected;

            // Test functionality results
            Assert.IsFalse(_target.VerifyCurrentTransaction(Guid.Empty));
        }

        [TestMethod]
        [ExpectedException(typeof(TransactionException))]
        public void RetrieveTransactionEmptyCurrentTransactionTest()
        {
            _accessor._currentTransactionId = Guid.Empty;
            _accessor.RetrieveTransaction(Guid.Empty);
        }

        [TestMethod]
        public void RetrieveTransactionWhereTransactionRetrievedTest()
        {
            var guid = new Guid("{00000000-0000-0001-1234-000000000000}");
            var requestorId = new Guid("{00000000-0000-0003-1234-000000000000}");
            var transactionId = new Guid("{00000000-0000-0000-1234-000000000000}");
            _storageTransaction.Setup(m => m.Commit()).Verifiable();
            _storageTransaction.Setup(m => m.Dispose()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentRequestId"] = Guid.Empty).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentRequestorId"] = Guid.Empty).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionRequestorId"] = requestorId).Verifiable();
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);

            var result = (Guid)_accessor.RetrieveTransaction(guid);
            Assert.AreEqual(transactionId, result);
        }

        [TestMethod]
        public void RequestorExistsInQueueWithNoMatchTest()
        {
            var queueOfRequestors =
                new Queue<KeyValuePair<ITransactionRequestor, TransactionType>>();
            queueOfRequestors.Enqueue(
                new KeyValuePair<ITransactionRequestor, TransactionType>(
                    new TestTransactionRequestor(),
                    TransactionType.Write));

            Assert.IsFalse(_accessor.RequestorExistsInQueue(Guid.Empty, queueOfRequestors));
        }

        [TestMethod]
        public void RequestTransactionWithTimedOutRequestTest()
        {
            var requestorGuid = Guid.NewGuid();
            _transactionCoordinator.Setup(
                    m => m.RequestTransaction(It.IsAny<ITransactionRequestor>(), TransactionType.Read, false))
                .Returns(requestorGuid);

            var transactionGuid = _target.RequestTransaction(requestorGuid, 5000, TransactionType.Read);
            Assert.AreNotEqual(Guid.Empty, transactionGuid);

            _transactionCoordinator.Setup(
                m => m.RequestTransaction(It.IsAny<ITransactionRequestor>(), TransactionType.Read, false)).Returns(Guid.Empty);
            var timedoutRequestorGuid = Guid.NewGuid();

            var timedoutTransactionGuid = _target.RequestTransaction(timedoutRequestorGuid, 1, TransactionType.Read);
            Assert.AreEqual(Guid.Empty, timedoutTransactionGuid);
        }

        /// <summary>
        ///     A test for RequestTransaction(ITransactionRequestor, TransactionType) where no
        ///     transaction is in progress and a 'Read' transaction is requested.
        /// </summary>
        [TestMethod]
        public void QueueRequestTransactionForReadWithNoCurrentTransactionTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionStartedEvent>())).Verifiable();
            var requestor = new TestTransactionRequestor();
            var transactionType = TransactionType.Read;
            _accessor._currentTransactionId = Guid.Empty;
            _storageTransaction.SetupSet(m => m["CurrentTransactionId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionRequestorId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.Setup(m => m.Dispose()).Verifiable();
            _storageTransaction.Setup(m => m.Commit()).Verifiable();
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);
            Guid transactionGuid = _accessor.RequestTransaction(requestor, transactionType);

            Assert.AreEqual(_accessor._currentTransactionId, transactionGuid);
        }

        [TestMethod]
        public void ReleaseTransactionTest()
        {
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionCompletedEvent>())).Verifiable();
            var requestorGuid = Guid.NewGuid();
            _accessor._currentTransactionId = Guid.Empty;
            _storageTransaction.SetupSet(m => m["CurrentTransactionId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionRequestorId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.Setup(m => m.Dispose()).Verifiable();
            _storageTransaction.Setup(m => m.Commit()).Verifiable();
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);

            Guid transactionGuid = _accessor.RequestTransaction(requestorGuid, 0, TransactionType.Write);

            _target.ReleaseTransaction(transactionGuid);

            Assert.AreEqual(Guid.Empty, _accessor._currentTransactionId);

            _eventBus.Verify();
        }

        [TestMethod]
        public void InitializeTest()
        {
            // Currently Initialize does nothing but is required to be implemented by the IService/IRunnable interfaces
            var coordinator = new TransactionCoordinator();

            Assert.AreEqual(RunnableState.Uninitialized, coordinator.RunState);

            coordinator.Initialize();
        }

        [TestMethod]
        public void ClearAndPersistGuidTest()
        {
            var name = "CurrentTransactionId";

            var actual = Guid.NewGuid();
            _storageTransaction.SetupSet(m => m[name] = It.IsAny<Guid>()).Verifiable();
            _accessor.ClearAndPersistGuid(_storageTransaction.Object, name, actual);

            _block.Verify();
            _storageTransaction.Verify();
        }

        /// <summary>
        ///     A test for AbandonTransactions() where a single, non-queued transaction
        ///     is abandoned.
        /// </summary>
        [TestMethod]
        public void AbandonTransactionsTest()
        {
            var requestorGuid = Guid.NewGuid();
            _storageTransaction.Setup(m => m.Commit()).Verifiable();
            _storageTransaction.Setup(m => m.Dispose()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionRequestorId"] = It.IsAny<Guid>()).Verifiable();
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionStartedEvent>())).Verifiable();
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionCompletedEvent>())).Verifiable();

            _accessor._currentTransactionId = Guid.Empty;
            var transactionId = _target.RequestTransaction(requestorGuid, 0, TransactionType.Write);

            // First assert that we have a non-empty Guid for the current transaction id and that it's the one we expect
            Assert.AreEqual(_accessor._currentTransactionId, transactionId);

            _storageTransaction.SetupSet(m => m["CurrentTransactionRequestorId"] = Guid.Empty).Verifiable();
            _target.AbandonTransactions(requestorGuid);

            Assert.AreEqual(Guid.Empty, _accessor._currentTransactionId);
            _eventBus.Verify();
            _storageTransaction.Verify();
            _block.Verify();
        }

        [TestMethod]
        public void AbandonTransactionsWithQueuedTransactionsTest()
        {
            var requestorGuid = Guid.NewGuid();
            var transactionGuid = _target.RequestTransaction(requestorGuid, 0, TransactionType.Write);
            var requestor = new TestTransactionRequestor();
            var requestor2 = new TestTransactionRequestor();

            _target.RequestTransaction(requestor, TransactionType.Write);
            _target.RequestTransaction(requestor, TransactionType.Write);
            _target.RequestTransaction(requestor2, TransactionType.Write);
            _target.RequestTransaction(requestor, TransactionType.Write);
            Assert.AreEqual(
                4,
                ((Queue<KeyValuePair<ITransactionRequestor, TransactionType>>)_accessor._queuedRequestors).Count);

            _target.ReleaseTransaction(transactionGuid);

            var dateTime = DateTime.Now;
            while (requestor.RequestGuid == Guid.Empty &&
                   DateTime.Now - dateTime < TimeSpan.FromSeconds(5))
            {
                // If the requestId has not been delivered within 5 seconds then something is seriously wrong, consider test failed
            }

            _target.AbandonTransactions(requestor.RequestorGuid);
            Thread.Sleep(30);

            Guid currentTransactionGuid = _accessor._currentTransactionId;

            Assert.AreEqual(currentTransactionGuid, _accessor._currentTransactionId);
            Assert.AreEqual(
                1,
                ((Queue<KeyValuePair<ITransactionRequestor, TransactionType>>)_accessor._queuedRequestors).Count);

            _target.AbandonTransactions(requestor2.RequestorGuid);
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(new Guid("{00000000-0000-0000-1234-000000000000}"), _accessor._currentTransactionId);
            Assert.AreEqual(
                new Guid("{00000000-0000-0002-1234-000000000000}"),
                _accessor._currentTransactionRequestorId);
            Assert.AreEqual(new Guid("{00000000-0000-0003-1234-000000000000}"), _accessor._currentRequestorId);
            Assert.AreEqual(new Guid("{00000000-0000-0001-1234-000000000000}"), _accessor._currentRequestId);
            Assert.AreEqual(
                0,
                ((Queue<KeyValuePair<ITransactionRequestor, TransactionType>>)_accessor._queuedRequestors).Count);
        }

        /// <summary>
        ///     Tests that nothing happens when a transaction that doesnt exist is abandoned.
        /// </summary>
        [TestMethod]
        public void AbandonNoTransactionTest()
        {
            _target.AbandonTransactions(new Guid("{16A6BAD1-A789-47e7-9922-28C29AB5E59A}"));
        }

        /// <summary>
        ///     Tests that an exception is thrown when no transaction is set.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TransactionException))]
        public void RetrieveNonexistantTransaction()
        {
            _target.RetrieveTransaction(new Guid());
        }

        /// <summary>
        ///     A test for RequestTransaction(Guid, int, TransactionType) that verifies that
        ///     the call will only wait(block) for the specified amount of time before returning
        ///     an empty Guid to the caller.
        /// </summary>
        /// <remarks>
        ///     This test is highly dependent on the precision of Thread.Sleep(int) and
        ///     WaitHandle.WaitOne(int). Timing requirements must take into account the following
        ///     performance statistics of the WaitOne(int) call over 100 iterations:
        ///     <list type="bullet">
        ///         <item>Avg. WaitOne(10) timeout: 15.629372 ms (precision: +/-36.0179027026806%); min: 10.006 ms; max: 21.0126 ms</item>
        ///         <item>
        ///             Avg. WaitOne(100) timeout: 109.235502 ms (precision: +/-8.45467071685178%); min: 106.0636 ms; max: 114.0684
        ///             ms
        ///         </item>
        ///         <item>
        ///             Avg. WaitOne(1000) timeout: 1014.498334 ms (precision: +/-1.42911363322145%); min: 1008.6048 ms; max:
        ///             1014.6084 ms
        ///         </item>
        ///         <item>
        ///             Avg. WaitOne(2000) timeout: 2013.037874 ms (precision: +/-0.647671569839459%); min: 2009.4018 ms; max:
        ///             2017.2096 ms
        ///         </item>
        ///     </list>
        ///     Because of this uncertainty, timing constraints are not checked within this test.
        /// </remarks>
        [TestMethod]
        public void RequestTransactionTestWithTimeoutAndTransactionInProgress()
        {
            var timeout = 1000;

            var requestorGuid = Guid.NewGuid();
            var transactionGuid = _target.RequestTransaction(requestorGuid, 0, TransactionType.Write);
            var otherTransactionGuid = Guid.Empty;

            _transactionCoordinator.Setup(
                    m => m.RequestTransaction(It.IsAny<ITransactionRequestor>(), TransactionType.Write, false))
                .Returns(otherTransactionGuid);

            var otherRequestorThread = new Thread(
                () =>
                {
                    var otherRequestorGuid = Guid.NewGuid();
                    otherTransactionGuid = _target.RequestTransaction(
                        otherRequestorGuid,
                        timeout,
                        TransactionType.Write);
                });

            otherRequestorThread.Start();
            otherRequestorThread.Join();

            Assert.AreEqual(Guid.Empty, otherTransactionGuid);
        }

        /// <summary>
        ///     Test to make sure you can still retrieve the state of the
        ///     TransactionCoordinator even if it has been disposed of
        /// </summary>
        [TestMethod]
        public void GetRunnableStateWhenDisposed()
        {
            var coordinator = new TransactionCoordinator();
            coordinator.Initialize();
            coordinator.Dispose();

            Assert.AreEqual(RunnableState.Stopped, coordinator.RunState);
        }

        [TestMethod]
        public void DoTransactionRequestWhenRequestorsAreQueuedTest()
        {
            var queuedRequestors = new Queue<KeyValuePair<ITransactionRequestor, TransactionType>>();

            queuedRequestors.Enqueue(
                new KeyValuePair<ITransactionRequestor, TransactionType>(
                    new TestTransactionRequestor(),
                    TransactionType.Write));

            _accessor._queuedRequestors = queuedRequestors;
            _accessor._currentTransactionId = Guid.Empty;

            var actual = (Guid)_accessor.DoTransactionRequest(Guid.NewGuid(), TransactionType.Write);

            Assert.AreEqual(Guid.Empty, actual);
        }

        [TestMethod]
        public void DoTransactionRequestWhenRequestorIsEmptyGuidTest()
        {
            var actual = (Guid)_accessor.DoTransactionRequest(Guid.Empty, TransactionType.Write);

            Assert.AreEqual(Guid.Empty, actual);
        }

        [TestMethod]
        public void DoTransactionRequestWithSnapshotTest()
        {
            _accessor._currentTransactionId = Guid.Empty;
            _storageTransaction.Setup(m => m.Commit()).Verifiable();
            _storageTransaction.Setup(m => m.Dispose()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionRequestorId"] = It.IsAny<Guid>()).Verifiable();
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionStartedEvent>())).Verifiable();

            var actual = (Guid)_accessor.DoTransactionRequest(Guid.NewGuid(), TransactionType.Write);

            Assert.AreNotEqual(Guid.Empty, actual);
            _eventBus.Verify();
            _storageTransaction.Verify();
            _block.Verify();
        }

        [TestMethod]
        public void AbandonTransactionWithQueuedTransactionTest()
        {
            _storageTransaction.Setup(m => m.Commit()).Verifiable();
            _storageTransaction.Setup(m => m.Dispose()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentRequestorId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentRequestId"] = It.IsAny<Guid>()).Verifiable();
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionCompletedEvent>())).Verifiable();

            _target.AbandonTransactions(_accessor._currentRequestorId);

            _eventBus.Verify();
            _storageTransaction.Verify();
            _block.Verify();
        }

        [TestMethod]
        public void DisposeTwiceTest()
        {
            _target.Dispose();
            _target.Dispose();
        }

        [TestMethod]
        public void RunTest()
        {
            _storageTransaction.Setup(m => m.Commit()).Verifiable();
            _storageTransaction.Setup(m => m.Dispose()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentTransactionId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentRequestorId"] = It.IsAny<Guid>()).Verifiable();
            _storageTransaction.SetupSet(m => m["CurrentRequestId"] = It.IsAny<Guid>()).Verifiable();
            _block.Setup(m => m.StartTransaction()).Returns(_storageTransaction.Object);

            // callback to stop running once we reach the end of the while loop
            _eventBus.Setup(m => m.Publish(It.IsAny<TransactionStartedEvent>())).Callback(() => _target.Stop())
                .Verifiable();

            // add at least one queued requestor
            _target.RequestTransaction(new TestTransactionRequestor(), TransactionType.Write);

            _accessor._currentTransactionId = Guid.Empty;
            _accessor._currentRequestId = Guid.Empty;

            // make a thread for Run and start it up
            var runThread = new Thread(_target.Run);
            runThread.Start();

            Assert.IsTrue(runThread.Join(5000));

            _eventBus.Verify();
            _block.Verify();
            _storageTransaction.Verify();
        }
    }
}
