namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Contracts.Client;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;

    /// <summary>
    ///     Contains the unit tests for CardsHeldConsumer
    /// </summary>
    [TestClass]
    public class CardsHeldConsumerTest
    {
        private CardsHeldConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);

        private readonly Mock<IRteStatusProvider> _rteProvider =
            new Mock<IRteStatusProvider>(MockBehavior.Strict);
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Strict);


        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);

            _eventBus.Setup(m => m.Subscribe(
                It.IsAny<object>(),
                It.IsAny<Action<CardsHeldEvent>>(),
                It.IsAny<Predicate<CardsHeldEvent>>())).Verifiable();
            _target = new CardsHeldConsumer(_rteProvider.Object, _exceptionHandler.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullRteProviderTest()
        {
            _target = new CardsHeldConsumer(null, _exceptionHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullExceptionHandlerTest()
        {
            _target = new CardsHeldConsumer(_rteProvider.Object, null);
        }

        [TestMethod]
        public void ConsumeHoldStatusTest()
        {
            // one card from the deal is held, 4 other cards drawn
            var test = new List<HoldStatus>
            {
                HoldStatus.Held, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld
            };

            var @event = new CardsHeldEvent(test);

            _rteProvider.Setup(m => m.Client1RteEnabled).Returns(true);
            _rteProvider.Setup(m => m.Client2RteEnabled).Returns(false);
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<CardHeldExceptionBuilder>(), 0)).Verifiable();

            _target.Consume(@event);

            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<CardHeldExceptionBuilder>(), 0), Times.Exactly(5));
        }

        [TestMethod]
        public void ConsumeHoldDrawMoreThan5CardsTest()
        {
            // one card from the deal is held, 5 other cards drawn
            var test = new List<HoldStatus>
            {
                HoldStatus.Held, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld, HoldStatus.NotHeld
            };

            var @event = new CardsHeldEvent(test);

            _rteProvider.Setup(m => m.Client1RteEnabled).Returns(false);
            _rteProvider.Setup(m => m.Client2RteEnabled).Returns(true);
            _exceptionHandler.Setup(m => m.ReportException(It.IsAny<CardHeldExceptionBuilder>(), 1)).Verifiable();

            _target.Consume(@event);

            // even though we have 6 cards in the hand, only 5 are reported
            _exceptionHandler.Verify(m => m.ReportException(It.IsAny<CardHeldExceptionBuilder>(), 1), Times.Exactly(5));
        }

    }
}
