namespace Aristocrat.Monaco.Hhr.Tests.Progressive
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Hhr.Services.Progressive;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ProgressiveHitHandlerTests
    {
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);

        private readonly Mock<IProtocolLinkedProgressiveAdapter> _linkedProgressiveAdapter =
            new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);

        private Action<LinkedProgressiveHitEvent> _linkedProgressiveHitEventHandler;
        private ProgressiveHitHandler _progressiveHitService;

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<object>())).Verifiable();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenNullExpectException(
            bool nullLinkedProgressiveProvider = false,
            bool nullEvent = false)
        {
            SetupProgressiveHitService(nullLinkedProgressiveProvider, nullEvent);
            Assert.IsNull(_progressiveHitService);
        }

        private void SetupProgressiveHitService(
            bool nullLinkedProgressiveProvider = false,
            bool nullEvent = false)
        {
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<ProgressiveHitHandler>(),
                        It.IsAny<Action<LinkedProgressiveHitEvent>>()))
                .Callback<object, Action<LinkedProgressiveHitEvent>
                >((y, x) => _linkedProgressiveHitEventHandler = x);

            _progressiveHitService = new ProgressiveHitHandler(
                nullLinkedProgressiveProvider ? null : _linkedProgressiveAdapter.Object,
                nullEvent ? null : _eventBus.Object);
        }

        [TestMethod]
        public void TestProgressiveHitEvent()
        {
            var linkedLevel = new LinkedProgressiveLevel
            {
                ProtocolName = ProtocolNames.HHR,
                ProgressiveGroupId = 1,
                LevelId = 1,
                Amount = 1000,
                Expiration = DateTime.UtcNow + TimeSpan.FromDays(365),
                CurrentErrorStatus = ProgressiveErrors.None,
                ClaimStatus = new LinkedProgressiveClaimStatus
                {
                    WinAmount = 1234
                }
            };

            var progressiveLevel = new ProgressiveLevel
            {
                LevelName = "TestLevelName",
                CurrentValue = 1234
            };

            SetupProgressiveHitService();
            _linkedProgressiveHitEventHandler?.Invoke(new LinkedProgressiveHitEvent(progressiveLevel,
                new List<IViewableLinkedProgressiveLevel> {linkedLevel}, new JackpotTransaction()));
        }
    }
}