namespace Aristocrat.Monaco.Asp.Tests.Client.DataSources
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Asp.Events;
    using Aristocrat.Monaco.Kernel;
    using Asp.Client.DataSources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HandpayValidatorTests
    {
        private HandpayValidator _subject;
        private Mock<IEventBus> _eventBus;
        private Action<LinkStatusChangedEvent> _linkStatusChangedCallback;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(m => m.Subscribe(It.IsAny<HandpayValidator>(), It.IsAny<Action<LinkStatusChangedEvent>>()))
                  .Callback<object, Action<LinkStatusChangedEvent>>((subscriber, callback) => _linkStatusChangedCallback = callback);

            _subject = new HandpayValidator(_eventBus.Object);
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new HandpayValidator(null));
        }

        [TestMethod]
        public void HandpayValidatorNameTest()
        {
            Assert.AreEqual("HandpayValidator", _subject.Name);
        }

        [TestMethod]
        public void AllowLocalHandpayTest()
        {
            Assert.IsTrue(_subject.AllowLocalHandpay);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expected = new[] { typeof(IHandpayValidator) };

            CollectionAssert.AreEqual(expected, (ICollection)_subject.ServiceTypes);
        }

        [TestMethod]
        public void LogTransactionRequiredTest()
        {
            Assert.IsTrue(_subject.LogTransactionRequired());
        }

        [TestMethod]
        public void RequestHandpayTest()
        {
            var transaction = new HandpayTransaction();

            var actualTask = _subject.RequestHandpay(transaction);
            Assert.AreEqual(Task.CompletedTask, actualTask);
        }

        [TestMethod]
        public void LinkStatusChangedEventTest()
        {
            Assert.AreEqual(false, _subject.HostOnline);

            _linkStatusChangedCallback(new LinkStatusChangedEvent(true));
            Assert.AreEqual(true, _subject.HostOnline);
        }

        [TestMethod]
        public void ValidateHandpayTest()
        {
            Assert.IsTrue(_subject.ValidateHandpay(1, 1, 1, HandpayType.GameWin));
        }
    }
}