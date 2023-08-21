namespace Aristocrat.Monaco.Sas.Tests.AftTransferProvider
{
    using System;
    using Accounting.Contracts;
    using Application.Contracts;
    using Contracts.Client;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.AftTransferProvider;
    using Test.Common;
    using IPrinter = Hardware.Contracts.Printer.IPrinter;

    /// <summary>
    ///     Contains the tests for the NullAftOnTransferProvider class
    /// </summary>
    [TestClass]
    public class NullAftOnTransferProviderTest
    {
        private NullAftOnTransferProvider _target;
        private readonly Mock<IAftLockHandler> _aftLock = new Mock<IAftLockHandler>(MockBehavior.Default);
        private readonly Mock<ISasHost> _sasHost = new Mock<ISasHost>(MockBehavior.Strict);
        private readonly Mock<ITransactionCoordinator> _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Strict);
        private readonly Mock<ITime> _timeService = new Mock<ITime>(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly Mock<IAutoPlayStatusProvider> _autoPlayStatusProvider = new Mock<IAutoPlayStatusProvider>(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IPrinter>(MockBehavior.Default);
            _target = new NullAftOnTransferProvider(
                _sasHost.Object,
                _aftLock.Object,
                _timeService.Object,
                _propertiesManager.Object,
                _transactionCoordinator.Object,
                _autoPlayStatusProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            _target.Dispose();
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void PropertiesTest()
        {
            Assert.IsFalse(_target.IsAftOnAvailable);
            Assert.IsFalse(_target.CanTransfer);
            Assert.IsFalse(_target.IsAftPending);
            Assert.AreEqual(Guid.Empty, _target.RequestorGuid);
        }

        [TestMethod]
        public void MethodsTest()
        {
            Assert.IsFalse(_target.InitiateAftOn());
            Assert.IsFalse(_target.AftOnRequest(null, true));
        }

        [TestMethod]
        public void TaskReturnMethodsTestAsync()
        {
            Assert.IsFalse(_target.InitiateTransfer(null).Result);
        }
    }
}
