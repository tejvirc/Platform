namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class EnableDisableBillAcceptorHandlerTest
    {
        private const int TimeoutWait = 1000;  // one second
        private EnableDisableBillAcceptorHandler _target;
        private Mock<ISasNoteAcceptorProvider> _noteAcceptor;
        private AutoResetEvent _waiter;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _noteAcceptor = new Mock<ISasNoteAcceptorProvider>(MockBehavior.Default);
            _waiter = new AutoResetEvent(false);
            _target = new EnableDisableBillAcceptorHandler(_noteAcceptor.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(2, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EnableBillAcceptor));
            Assert.IsTrue(_target.Commands.Contains(LongPoll.DisableBillAcceptor));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNoteAcceptorProviderTest()
        {
            _target = new EnableDisableBillAcceptorHandler(null);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        public void HandleDisableSucceedTest()
        {
            var data = new EnableDisableData { Enable = false };
            var expected = new EnableDisableResponse { Succeeded = true };

            _noteAcceptor.Setup(m => m.DisableBillAcceptor()).Returns(Task.CompletedTask).Callback(() => _waiter.Set());

            var actual = _target.Handle(data);

            // wait for the async call to finish
            Assert.IsTrue(_waiter.WaitOne(TimeoutWait));

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
        }

        [TestMethod]
        public void HandleEnableSucceedTest()
        {
            var data = new EnableDisableData { Enable = true };
            var expected = new EnableDisableResponse { Succeeded = true };

            _noteAcceptor.Setup(m => m.EnableBillAcceptor()).Returns(Task.CompletedTask).Callback(() => _waiter.Set());

            var actual = _target.Handle(data);

            // wait for the async call to finish
            Assert.IsTrue(_waiter.WaitOne(TimeoutWait));

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
        }
    }
}
