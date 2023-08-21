namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;
    using Test.Common;

    [TestClass]
    public class LP56SendEnabledGameNumbersHandlerTests
    {
        private LP56SendEnabledGameNumbersHandler _target;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IPropertiesManager> _propertiesManager;
        private const int NumberOfActiveGames = 320;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _gameProvider = MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Default);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Default);
            _target = new LP56SendEnabledGameNumbersHandler(_gameProvider.Object, _propertiesManager.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIGameProviderTest()
        {
            _target = new LP56SendEnabledGameNumbersHandler(null, _propertiesManager.Object);
            Assert.Fail("Should have thrown an ArgumentNullException");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullIPropertiesManagerTest()
        {
            _target = new LP56SendEnabledGameNumbersHandler(_gameProvider.Object, null);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.SendEnabledGameNumbers));
        }

        [TestMethod]
        public void HandleTest()
        {
            var games = Enumerable.Range(0, NumberOfActiveGames)
                .Select(
                    id => new TestGameProfile
                    {
                        Id = id,
                        Enabled = true,
                        ActiveDenominations = new List<long> { 1000 },
                        Denominations = new List<IDenomination>
                        {
                            new MockDenomination(1000, id * 2 + 1), new MockDenomination(5000, id * 2 + 2)
                        }
                    }).Cast<IGameDetail>().ToList();

            _gameProvider.Setup(m => m.GetEnabledGames()).Returns(games);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<object>())).Returns((long)1000);
            var response = _target.Handle(new LongPollMultiDenomAwareData());
            CollectionAssert.AreEqual(
                games.SelectMany(game => game.Denominations.Where(x => x.Value == 1000).Select(x => x.Id)).ToList(),
                response.EnabledGameIds.ToList());
        }

        [TestMethod]
        public void HandleDenomTest()
        {
            var games = Enumerable.Range(0, NumberOfActiveGames)
                .Select(
                    id => new TestGameProfile
                    {
                        Id = id,
                        Enabled = true,
                        ActiveDenominations = id % 2 == 0 ? new List<long> { 1000, 5000 } : new List<long> { 1000 },
                        Denominations = id % 2 == 0
                            ? new List<IDenomination>
                            {
                                new MockDenomination(1000, id * 2 + 1), new MockDenomination(5000, id * 2 + 2)
                            }
                            : new List<IDenomination> { new MockDenomination(1000, id * 2 + 1) }
                    }).ToList();

            _gameProvider.Setup(m => m.GetAllGames()).Returns(games);
            _gameProvider.Setup(m => m.GetEnabledGames()).Returns(games);

            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<object>())).Returns((long)1);

            var expected = games.Where(game => game.Id % 2 == 0)
                .SelectMany(game => game.Denominations.Where(x => x.Value == 5000).Select(x => x.Id)).ToList();
            var actual = _target.Handle(new LongPollMultiDenomAwareData { TargetDenomination = 5, MultiDenomPoll = true }).EnabledGameIds.ToList();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}