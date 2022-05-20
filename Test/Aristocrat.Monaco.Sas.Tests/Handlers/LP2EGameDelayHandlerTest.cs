namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Sas.Contracts.SASProperties;
    using Aristocrat.Monaco.Sas.Handlers;
    using Aristocrat.Monaco.Sas.Storage.Models;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Tests for the LP2EGameDelayHandler class
    /// </summary>
    [TestClass]
    public class LP2EGameDelayHandlerTest
    {
        private LP2EGameDelayHandler _target;
        private Mock<IBonusHandler> _bonusHandler;
        private Mock<IPropertiesManager> _propertiesManager;

        /// <summary>
        ///     In <see cref="LP2EGameDelayHandler"/> we multiply the incoming data value by 100.
        /// </summary>
        private const int DelayTimePadding = 100;
        private const uint Delay = 20;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _bonusHandler = new Mock<IBonusHandler>(MockBehavior.Default);

            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { LegacyBonusAllowed = true });

            _target = new LP2EGameDelayHandler(
                _bonusHandler.Object,
                _propertiesManager.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.DelayGame));
        }

        [TestMethod]
        public void TestSetDelay()
        {
            var actualTime = TimeSpan.Zero;

            _bonusHandler
                .Setup(x => x.SetGameEndDelay(
                    TimeSpan.FromMilliseconds(Delay * DelayTimePadding),
                    TimeSpan.MaxValue,
                    0,
                    true))
                .Callback((TimeSpan delay, TimeSpan duration, int numberOfGames, bool both) => actualTime = delay);

            _target.Handle(new LongPollSingleValueData<uint>(Delay));

            Assert.AreEqual(
                Delay * DelayTimePadding,
                actualTime.TotalMilliseconds);
        }

        [TestMethod]
        public void TestClearDelay()
        {
            var actualTime = TimeSpan.Zero;

            _bonusHandler
                .Setup(x => x.SetGameEndDelay(
                    TimeSpan.FromMilliseconds(Delay * DelayTimePadding),
                    TimeSpan.MaxValue,
                    0,
                    true))
                .Callback((TimeSpan delay, TimeSpan duration, int numberOfGames, bool both) => actualTime = delay);

            _target.Handle(new LongPollSingleValueData<uint>(Delay));

            Assert.AreEqual(
                Delay * DelayTimePadding,
                actualTime.TotalMilliseconds);
        }

        [TestMethod]
        public void HandleLegacyBonusingNotEnabledTest()
        {
            var actualTime = TimeSpan.Zero;

            _propertiesManager.Setup(m => m.GetProperty(SasProperties.SasFeatureSettings, It.IsAny<SasFeatures>()))
                .Returns(new SasFeatures { LegacyBonusAllowed = false });

            _bonusHandler
                .Setup(x => x.SetGameEndDelay(
                    It.IsAny<TimeSpan>(),
                    TimeSpan.MaxValue,
                    0,
                    true))
                .Callback((TimeSpan delay, TimeSpan duration, int numberOfGames, bool both) => actualTime = delay);

            _target.Handle(new LongPollSingleValueData<uint>(Delay));

            Assert.AreEqual(0, actualTime.TotalMilliseconds);
        }
    }
}