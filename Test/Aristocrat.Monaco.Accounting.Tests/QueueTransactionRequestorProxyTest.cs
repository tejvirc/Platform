namespace Aristocrat.Monaco.Accounting.Tests
{
    using System;
    using System.IO;
    using System.Threading;
    using Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mono.Addins;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     This is a test class for QueueTransactionRequestorProxy and is intended
    ///     to contain all QueueTransactionRequestorProxy Unit Tests
    /// </summary>
    [TestClass]
    public class QueueTransactionRequestorProxyTest
    {
        private Mock<ITransactionCoordinator> _transactionCoordinator;

        /// <summary>
        ///     Setup the environment for each test
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            AddinManager.Initialize(Directory.GetCurrentDirectory());
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _transactionCoordinator =
                MoqServiceManager.CreateAndAddService<ITransactionCoordinator>(MockBehavior.Strict);
        }

        /// <summary>
        ///     Cleanup after each test
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            _transactionCoordinator = null;

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

        /// <summary>
        ///     A test for the constructor where no transaction is already
        ///     in progress. A transaction Guid should be available immediately
        /// </summary>
        [TestMethod]
        public void ConstructorWithNoTransactionInProgress()
        {
            var transactionType = TransactionType.Write;
            var requestorGuid = Guid.NewGuid();
            var transactionGuid = Guid.NewGuid();
            _transactionCoordinator.Setup(
                    mock => mock.RequestTransaction(It.IsAny<ITransactionRequestor>(), TransactionType.Write, false))
                .Returns(transactionGuid);

            using (var readyEvent = new AutoResetEvent(false))
            {
                var proxy = new QueueTransactionRequestorProxy(
                    requestorGuid,
                    transactionType,
                    readyEvent,
                    false);

                Assert.IsTrue(proxy.IsReady);
                Assert.AreEqual(transactionGuid, proxy.GetTransactionGuid());
                Assert.AreEqual(requestorGuid, proxy.RequestorGuid);
            }
        }

        /// <summary>
        ///     A test for the constructor where a transaction is already
        ///     in progress. A transaction Guid should be available immediately
        /// </summary>
        [TestMethod]
        public void ConstructorWithTransactionInProgress()
        {
            var transactionType = TransactionType.Write;
            var requestorGuid = Guid.NewGuid();
            _transactionCoordinator.Setup(
                    mock => mock.RequestTransaction(
                        It.IsAny<QueueTransactionRequestorProxy>(),
                        TransactionType.Write,
                        false))
                .Returns(Guid.Empty);

            using (var readyReset = new AutoResetEvent(false))
            {
                var proxy = new QueueTransactionRequestorProxy(
                    requestorGuid,
                    transactionType,
                    readyReset,
                    false);

                Assert.IsFalse(proxy.IsReady);
                Assert.AreEqual(Guid.Empty, proxy.GetTransactionGuid());
            }
        }

        /// <summary>
        ///     Testing that NotifyTransactionReady behaves correctly when called by
        ///     the TransactionCoordinator when a transaction is ready
        /// </summary>
        [TestMethod]
        public void NotifyTransactionReady()
        {
            var transactionType = TransactionType.Write;
            var transactionGuid = Guid.NewGuid();
            var requestorGuid = Guid.NewGuid();
            var requestId = Guid.NewGuid();
            _transactionCoordinator.Setup(
                    mock => mock.RequestTransaction(
                        It.IsAny<QueueTransactionRequestorProxy>(),
                        TransactionType.Write,
                        false))
                .Returns(Guid.Empty);

            using (var readyReset = new AutoResetEvent(false))
            {
                var proxy = new QueueTransactionRequestorProxy(
                    requestorGuid,
                    transactionType,
                    readyReset,
                    false);

                Assert.IsFalse(proxy.IsReady);
                Assert.AreEqual(Guid.Empty, proxy.GetTransactionGuid());

                _transactionCoordinator.Setup(
                    mock => mock.RetrieveTransaction(requestId)).Returns(transactionGuid);

                var notifyThread = new Thread(
                    theRequestId =>
                    {
                        Thread.Sleep(2000);
                        proxy.NotifyTransactionReady((Guid)theRequestId!);
                    });
                notifyThread.Start(requestId);

                readyReset.WaitOne(5000);

                Assert.IsTrue(proxy.IsReady);
                var actualGuid = proxy.GetTransactionGuid();
                Assert.AreEqual(transactionGuid, actualGuid);
            }
        }
    }
}
