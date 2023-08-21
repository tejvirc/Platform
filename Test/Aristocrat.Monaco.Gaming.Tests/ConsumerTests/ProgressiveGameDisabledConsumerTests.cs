namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using System.Collections.Generic;
    using Consumers;
    using Contracts;
    using Contracts.Progressives;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class ProgressiveGameDisabledConsumerTests
    {
        private readonly Mock<IRuntime> _runtime = new Mock<IRuntime>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
        private readonly Mock<IEventBus> _bus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
        private ProgressiveGameDisabledConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.AddService(_bus);
            _target = new ProgressiveGameDisabledConsumer(_runtime.Object, _properties.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullRuntimeTest()
        {
            _target = new ProgressiveGameDisabledConsumer(null, _properties.Object, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullPropertiesTest()
        {
            _target = new ProgressiveGameDisabledConsumer(_runtime.Object, null, _gameProvider.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullGameProviderTest()
        {
            _target = new ProgressiveGameDisabledConsumer(_runtime.Object, _properties.Object, null);
        }

        [DataTestMethod]
        [DataRow(1, 1000L, "TestBet", 1, 1000L, "TestBet", true, true)]
        [DataRow(1, 1000L, "TestBet", 2, 1000L, "TestBet", true, false)]
        [DataRow(1, 1000L, "TestBet", 1, 2000L, "TestBet", true, false)]
        [DataRow(1, 1000L, "TestBet", 1, 1000L, "TestBet", false, false)]
        [DataRow(1, 1000L, "TestBet", 1, 1000L, null, true, true)]
        [DataRow(1, 1000L, "TestBet", 1, 1000L, "", true, true)]
        [DataRow(1, 1000L, "TestBet", 1, 1000L, "DifferentBet", true, false)]
        public void ConsumeTest(
            int selectedGameId,
            long selectedDenom,
            string selectedBetOption,
            int errorGameId,
            long errorDenom,
            string errorBetOption,
            bool isRuntimeConnected,
            bool flagUpdated)
        {
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(selectedGameId);
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>()))
                .Returns(selectedDenom);
            _runtime.Setup(x => x.Connected).Returns(isRuntimeConnected);
            var gameInfo = new MockGameInfo
            {
                Id = selectedGameId,
                Denominations = new List<IDenomination>
                {
                    new Denomination { Value = selectedDenom, Id = 1, BetOption = selectedBetOption }
                }
            };

            _gameProvider.Setup(x => x.GetGame(errorGameId)).Returns(gameInfo);

            _target.Consume(new ProgressiveGameDisabledEvent(errorGameId, errorDenom, errorBetOption));

            _runtime.Verify(
                x => x.UpdateFlag(RuntimeCondition.ProgressiveError, true),
                flagUpdated ? Times.Once() : Times.Never());
        }
    }
}