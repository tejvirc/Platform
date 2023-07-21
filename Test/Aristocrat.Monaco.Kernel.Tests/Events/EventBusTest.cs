namespace Aristocrat.Monaco.Kernel.Tests.Events
{
    using System;
    using System.Threading;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;

    [TestClass]
    public class EventBusTest
    {
        private const int waitTimeout = 1500;
        [TestMethod]
        public void VerifyServiceImplemenation()
        {
            var bus = new EventBus();

            bus.Initialize();
            Assert.IsFalse(string.IsNullOrEmpty(bus.Name));
            Assert.IsTrue(bus.ServiceTypes.Contains(typeof(IEventBus)));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPublishNullEventExpectException()
        {
            var bus = new EventBus();

            TestEvent evt = null;

            bus.Publish(evt);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSubscribeWithNullContextExpectException()
        {
            var bus = new EventBus();

            bus.Subscribe<TestEvent>(null, _ => { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSubscribeWithNullHandlerExpectException()
        {
            var bus = new EventBus();

            Action<TestEvent> handler = null;

            bus.Subscribe(this, handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenSubscribeWithNullFuncExpectException()
        {
            var bus = new EventBus();

            bus.Subscribe<TestEvent>(this, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUnsubscribeWithNullContextExpectException()
        {
            var bus = new EventBus();

            bus.Unsubscribe<TestEvent>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenUnsubscribeAllWithNullContextExpectException()
        {
            var bus = new EventBus();

            bus.UnsubscribeAll(null);
        }

        [TestMethod]
        public void WhenSubscribeExpectHandled()
        {
            var wait = new AutoResetEvent(false);
            var bus = new EventBus();
            TestEvent handled = null;
            var testEvent = new TestEvent();

            var handler = new Action<TestEvent>(
                evt =>
                {
                    handled = evt;
                    wait.Set();
                });

            bus.Subscribe(this, handler);
            bus.Publish(testEvent);

            wait.WaitOne(waitTimeout);

            Assert.AreEqual(handled, testEvent);
        }

        [TestMethod]
        public void WhenSubscribeMulitpleContextsExpectAllHandled()
        {
            var wait = new AutoResetEvent(false);
            var waitOnThisToo = new AutoResetEvent(false);
            var bus = new EventBus();
            TestEvent handled = null;
            TestEvent handledToo = null;
            var testEvent = new TestEvent();

            var handler = new Action<TestEvent>(
                evt =>
                {
                    handled = evt;
                    wait.Set();
                });

            var anotherHandler = new Action<TestEvent>(
                evt =>
                {
                    handledToo = evt;
                    waitOnThisToo.Set();
                });

            bus.Subscribe(this, handler);
            bus.Subscribe(new object(), anotherHandler);
            bus.Publish(testEvent);

            wait.WaitOne(waitTimeout);
            waitOnThisToo.WaitOne(waitTimeout);

            Assert.AreEqual(handled, testEvent);
            Assert.AreEqual(handledToo, testEvent);
        }

        [TestMethod]
        public void WhenResubscribeExpectHandled()
        {
            var wait = new AutoResetEvent(false);
            var bus = new EventBus();
            TestEvent notHandled = null;
            TestEvent handled = null;
            var testEvent = new TestEvent();

            var handler = new Action<TestEvent>(
                evt =>
                {
                    notHandled = evt;
                    wait.Set();
                });

            var newHandler = new Action<TestEvent>(
                evt =>
                {
                    handled = evt;
                    wait.Set();
                });

            bus.Subscribe(this, handler);
            bus.Subscribe(this, newHandler);
            bus.Publish(testEvent);

            wait.WaitOne(waitTimeout);

            Assert.IsNull(notHandled);
            Assert.AreEqual(handled, testEvent);
        }

        [TestMethod]
        public void WhenUnSubscribeExpectNotHandled()
        {
            var wait = new AutoResetEvent(false);
            var bus = new EventBus();
            TestEvent handled = null;
            var testEvent = new TestEvent();

            var handler = new Action<TestEvent>(
                evt =>
                {
                    handled = evt;
                    wait.Set();
                });

            bus.Subscribe(this, handler);
            bus.Unsubscribe<TestEvent>(this);
            bus.Publish(testEvent);

            wait.WaitOne(waitTimeout);

            Assert.IsNull(handled);
        }

        [TestMethod]
        public void WhenUnSubscribeAllExpectNotHandled()
        {
            var wait = new AutoResetEvent(false);
            var bus = new EventBus();
            TestEvent handled = null;
            var testEvent = new TestEvent();

            var handler = new Action<TestEvent>(
                evt =>
                {
                    handled = evt;
                    wait.Set();
                });

            bus.Subscribe(this, handler);
            bus.UnsubscribeAll(this);
            bus.Publish(testEvent);

            wait.WaitOne(TimeSpan.FromMilliseconds(1));

            Assert.IsNull(handled);
        }

        [TestMethod]
        public void WhenDisposeExpectNotHandled()
        {
            var wait = new AutoResetEvent(false);
            var bus = new EventBus();
            TestEvent handled = null;
            var testEvent = new TestEvent();

            var handler = new Action<TestEvent>(
                evt =>
                {
                    handled = evt;
                    wait.Set();
                });

            bus.Subscribe(this, handler);
            bus.Dispose();
            bus.Publish(testEvent);

            wait.WaitOne(TimeSpan.FromMilliseconds(1));

            Assert.IsNull(handled);
        }
    }
}