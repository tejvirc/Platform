namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP51SendNumberOfGamesImplementedHandlerTest
    {
        private LP51SendNumberOfGamesImplementedHandler _target;
        private Mock<IGameProvider> _gameProvider;
        private const int NumberOfGamesImplemented = 320;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);

            var games = new List<IGameDetail>();
            games.AddRange(
                Enumerable.Repeat<IGameDetail>(
                    new TestGameProfile
                    {
                        Denominations = new List<IDenomination> { new MockDenomination(1000), new MockDenomination(20000) }
                    },
                    NumberOfGamesImplemented / 2));

            _gameProvider.Setup(m => m.GetAllGames()).Returns(games);
            _target = new LP51SendNumberOfGamesImplementedHandler(_gameProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendNumberOfGames));
        }

        [TestMethod]
        public void HandleReadTest()
        {
            var data = new LongPollData();
            var expected = new LongPollReadSingleValueResponse<int>(NumberOfGamesImplemented);
            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Data, actual.Data);
        }
    }
}
