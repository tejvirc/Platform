namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Test.Common;

    [TestClass]
    public class LP08ConfigureBillDenominationsHandlerTest
    {
        private const int TimeoutWait = 3000;  // three seconds
        private LP08ConfigureBillDenominationsHandler _target;
        private Mock<ISasNoteAcceptorProvider> _noteAcceptorProvider;
        private AutoResetEvent _waiter;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _noteAcceptorProvider = new Mock<ISasNoteAcceptorProvider>(MockBehavior.Default);
            _waiter = new AutoResetEvent(false);

            _target = new LP08ConfigureBillDenominationsHandler(_noteAcceptorProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.ConfigureBillDenominations));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullNoteAcceptorProviderTest()
        {
            _target = new LP08ConfigureBillDenominationsHandler(null);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        public void HandleTest()
        {
            // pass in $1 and $10,000 bills
            var data = new LongPollBillDenominationsData();
            data.Denominations.Clear();
            data.Denominations.Add(1);
            data.Denominations.Add(10_000);
            data.DisableAfterAccept = false;

            _noteAcceptorProvider.Setup(m => m.ConfigureBillDenominations(It.IsAny<IEnumerable<ulong>>()))
                .Callback(() => _waiter.Set());
            _noteAcceptorProvider
                .Setup(m => m.BillDisableAfterAccept)
                .Callback(() => _waiter.Set());

            var actual = _target.Handle(data);

            // wait for the async call to finish, need 2 waits since we have 2 mocks we're checking
            _waiter.WaitOne(TimeoutWait);
            _waiter.WaitOne(TimeoutWait);

            Assert.AreEqual(typeof(LongPollResponse), actual.GetType());
         }
    }
}
