namespace Aristocrat.Monaco.Gaming.Tests.Commands.RuntimeEvents
{
    using System;
    using Contracts;
    using Contracts.Events;
    using Gaming.Commands;
    using Gaming.Commands.RuntimeEvents;
    using Gaming.Runtime.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using PlayMode = Gaming.Runtime.Client.PlayMode;

    [TestClass]
    public class AllowCashInEventHandlerTests
    {
        private readonly Mock<IPlayerBank> _bank = new(MockBehavior.Default);
        private readonly Mock<IEventBus> _bus = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Default);
        private AllowCashInEventHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTest(
            bool nullBank,
            bool nullBus,
            bool nulProperties)
        {
            Assert.ThrowsException<ArgumentNullException>(() => _ = CreateTarget(nullBank, nullBus, nulProperties));
        }

        [DataRow(GameRoundEventAction.Begin, false, true, false)]
        [DataRow(GameRoundEventAction.Begin, true, false, false)]
        [DataRow(GameRoundEventAction.Completed, false, false, true)]
        [DataRow(GameRoundEventAction.Completed, true, false, false)]
        [DataTestMethod]
        public void HandleTest(
            GameRoundEventAction eventAction,
            bool allowCashInDuringGamePlay,
            bool allowMoney,
            bool preventMoney)
        {
            _properties.Setup(x => x.GetProperty(GamingConstants.AllowCashInDuringPlay, It.IsAny<bool>()))
                .Returns(allowCashInDuringGamePlay);

            var gameRoundEvent = new GameRoundEvent(
                GameRoundEventState.AllowCashInDuringPlay,
                eventAction,
                PlayMode.Normal,
                null,
                0,
                0,
                0,
                null);
            _target.HandleEvent(gameRoundEvent);

            _bus.Verify(x => x.Publish(It.IsAny<AllowMoneyInEvent>()), allowMoney ? Times.Once() : Times.Never());
            _bus.Verify(x => x.Publish(It.IsAny<ProhibitMoneyInEvent>()), preventMoney ? Times.Once() : Times.Never());
            _bank.Verify(x => x.Lock(), preventMoney ? Times.Once() : Times.Never());
        }

        private AllowCashInEventHandler CreateTarget(
            bool nullBank = false,
            bool nullBus = false,
            bool nulProperties = false)
        {
            return new AllowCashInEventHandler(
                nullBank ? null : _bank.Object,
                nullBus ? null : _bus.Object,
                nulProperties ? null : _properties.Object);
        }
    }
}