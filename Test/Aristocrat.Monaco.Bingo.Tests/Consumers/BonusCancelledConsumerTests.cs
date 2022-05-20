﻿namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BonusCancelledConsumerTests
    {
        private BonusCancelledConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new BonusCancelledConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _bingoEventQueue.Object);
        }

        [DataRow(true, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, DisplayName = "Event Reporting Service Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool eventBusNull, bool queueNull)
        {
            _target = new BonusCancelledConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                queueNull ? null : _bingoEventQueue.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.BonusWinRefused)).Verifiable();

            _target.Consume(new BonusCancelledEvent(new BonusTransaction()));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusWinRefused), Times.Once());
        }

        [TestMethod]
        public void ConsumesGameWinTest()
        {
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.BonusWinRefused)).Verifiable();

            _target.Consume(new BonusCancelledEvent(new BonusTransaction { Mode = BonusMode.GameWin }));

            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.BonusWinRefused), Times.Never());
        }
    }
}