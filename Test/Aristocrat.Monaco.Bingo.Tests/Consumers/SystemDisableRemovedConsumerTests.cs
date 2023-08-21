namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Bingo.Consumers;
    using Kernel;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Commands;
    using Common;

    [TestClass]
    public class SystemDisableRemovedConsumerTests
    {
        private SystemDisableRemovedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new (MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, false, DisplayName = "EventBus Null")]
        [DataRow(false, false, true, DisplayName = "Command Handler Factory Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool nullReporting, bool nullEventBus, bool nullCommandFactory)
        {
            _ = CreateTarget(nullReporting, nullEventBus, nullCommandFactory);
        }

        [DataRow(false, DisplayName = "Is Still Disabled")]
        [DataRow(true, DisplayName = "Is Enabling")]
        [DataTestMethod]
        public async Task ConsumesTest(bool isEnabling)
        {
            var removedEvent = new SystemDisableRemovedEvent(
                SystemDisablePriority.Normal,
                Guid.Empty,
                "test",
                !isEnabling,
                false);
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.Enabled)).Verifiable();
            _commandHandlerFactory
                .Setup(x => x.Execute(It.IsAny<ReportEgmStatusCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            await _target.Consume(removedEvent, CancellationToken.None);

            _reportingService.Verify(
                m => m.AddNewEventToQueue(ReportableEvent.Enabled),
                isEnabling ? Times.Once() : Times.Never());
            _commandHandlerFactory.Verify();
        }

        private SystemDisableRemovedConsumer CreateTarget(
            bool nullReporting = false,
            bool nullEventBus = false,
            bool nullCommandFactory = false)
        {
            return new SystemDisableRemovedConsumer(
                nullEventBus ? null : _eventBus.Object,
                _sharedConsumer.Object,
                nullReporting ? null : _reportingService.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object);
        }
    }
}