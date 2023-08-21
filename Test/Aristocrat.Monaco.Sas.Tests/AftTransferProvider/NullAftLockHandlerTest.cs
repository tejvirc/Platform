namespace Aristocrat.Monaco.Sas.Tests.AftTransferProvider
{
    using System;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sas.AftTransferProvider;

    /// <summary>
    ///     Contains the tests for the NullAftLockHandler class
    /// </summary>
    [TestClass]
    public class NullAftLockHandlerTest
    {
        private readonly NullAftLockHandler _target = new NullAftLockHandler();

        [TestMethod]
        public void NameTest()
        {
            Assert.AreEqual("Aristocrat.Monaco.Sas.AftTransferProvider.NullAftLockHandler", _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            Assert.AreEqual(1, _target.ServiceTypes.Count);
            Assert.IsTrue(_target.ServiceTypes.Contains(typeof(IAftLockHandler)));
        }

        [TestMethod]
        public void PropertiesTest()
        {
            Assert.AreEqual(AftGameLockStatus.GameNotLocked, _target.LockStatus);
            Assert.AreEqual(Guid.Empty, _target.RequestorGuid);
        }

        [TestMethod]
        public void MethodsTest()
        {
            Assert.AreEqual(Guid.Empty, _target.RetrieveTransactionId());
        }
    }
}
