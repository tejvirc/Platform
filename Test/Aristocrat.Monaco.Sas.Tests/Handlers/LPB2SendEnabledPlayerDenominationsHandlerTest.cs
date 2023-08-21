namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Gaming.Contracts;
    using Sas.Handlers;
    using Aristocrat.Sas.Client;
    using Contracts.SASProperties;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    ///     Tests for the LPB2SendEnabledPlayerDenominationsHandler class
    /// </summary>
    [TestClass]
    public class LPB2SendEnabledPlayerDenominationsHandlerTest
    {
        private LPB2SendEnabledPlayerDenominationsHandler _target;
        private readonly Mock<IPropertiesManager> _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Strict);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Strict);
        private readonly List<long> DenominationsGame1 = new List<long> { 1000 };
        private readonly List<long> DenominationsGame2 = new List<long> { 25000 };

        [TestInitialize]
        public void MyTestInitialize()
        {
            var game1 = new Mock<IGameDetail>(MockBehavior.Strict);
            game1.SetupGet(c => c.ActiveDenominations).Returns(DenominationsGame1);

            var game2 = new Mock<IGameDetail>(MockBehavior.Strict);
            game2.SetupGet(c => c.ActiveDenominations).Returns(DenominationsGame2);

            var games = new List<IGameDetail> { game1.Object, game2.Object };

            _gameProvider.Setup(m => m.GetEnabledGames()).Returns(games);
            _target = new LPB2SendEnabledPlayerDenominationsHandler(_propertiesManager.Object, _gameProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendEnabledPlayerDenominations));
        }

        [TestMethod]
        public void GoodDataTest()
        {
            IReadOnlyCollection<byte> expectDenominationsCodes = DenominationsGame1.Union(DenominationsGame2).Distinct().Select(
                denomination => DenominationCodes.GetCodeForDenomination((int)denomination.MillicentsToCents())).ToList();

            // test good variables
            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(SasProperties.MultipleDenominationSupportedKey)),
                It.IsAny<bool>())).Returns(true);

            var expected = _target.Handle(new LongPollData());
            Assert.IsNotNull(expected);
            CollectionAssert.AreEqual(expected.EnabledDenominations.ToList(), expectDenominationsCodes.ToList());
        }

        [TestMethod]
        public void MultiDenominationNotSupportedTest()
        {
            // test multi not supported
            _propertiesManager.Setup(pm => pm.GetProperty(It.Is<string>(p => p.Equals(SasProperties.MultipleDenominationSupportedKey)),
                It.IsAny<bool>())).Returns(false);

            var expected = _target.Handle(new LongPollData());
            Assert.IsNull(expected);
        }
    }
}

