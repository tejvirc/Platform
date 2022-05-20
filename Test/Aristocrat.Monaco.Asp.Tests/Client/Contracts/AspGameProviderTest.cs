using Aristocrat.Monaco.Asp.Client.Contracts;
using Aristocrat.Monaco.Gaming.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aristocrat.Monaco.Asp.Tests.Client.Contracts
{
    [TestClass]
    public class AspGameProviderTests
    {
        private Mock<IGameProvider> _gameProvider;
        private Mock<IDenomination> _mockDenomActiveTrue;
        private Mock<IDenomination> _mockDenomActiveFalse;
        private List<IDenomination> _denominationList;
        private AspGameProvider _target;

        private int _activeGameId = 1;
        private int _activeGameValue = 10;
        private int _nonActiveGameId = 2;
        private int _nonActiveGameValue = 20;

        [TestInitialize]
        public void Initialize()
        {
            _gameProvider = new Mock<IGameProvider>();
            SetupGameProvider();
        }

        [DataRow(true, DisplayName = "Null GameProvider")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorTest(
            bool nullGameProvider)
        {
            _target = CreateAspGameProvider(nullGameProvider);
        }

        private AspGameProvider CreateAspGameProvider(bool nullGameProvider = false)
        {
            return new AspGameProvider(
                nullGameProvider ? null : _gameProvider.Object);
        }

        [TestMethod]
        public void GetEnabledGamesTest()
        {
            _target = new AspGameProvider(_gameProvider.Object);
            var result = _target.GetEnabledGames();
            Assert.AreEqual(result.Count, 1); // Only 1 Denomination is Active
            (IGameDetail game, IDenomination denom) = result.First();
            Assert.AreEqual(game.Denominations.Count<IDenomination>(), _denominationList.Count);
            Assert.AreEqual(denom.Id, _activeGameId);
            Assert.AreEqual(denom.Value, _activeGameValue);
        }

        private void SetupGameProvider()
        {
            var gameDetail = new Mock<IGameDetail>();
            SetupMockDenominations();
            _denominationList = new List<IDenomination> { _mockDenomActiveTrue.Object, _mockDenomActiveFalse.Object };
            gameDetail.Setup(x => x.Denominations).Returns(_denominationList);
            List<IGameDetail> gameDetailList = new List<IGameDetail>() { gameDetail.Object };
            _gameProvider.Setup(x => x.GetAllGames()).Returns(gameDetailList);
        }

        private void SetupMockDenominations()
        {
            _mockDenomActiveTrue = new Mock<IDenomination>(); //new Denomination(1, 10, true)
            _mockDenomActiveTrue.SetupGet(x => x.Id).Returns(_activeGameId);
            _mockDenomActiveTrue.SetupGet(x => x.Value).Returns(_activeGameValue);
            _mockDenomActiveTrue.SetupGet(x => x.Active).Returns(true);
            _mockDenomActiveFalse = new Mock<IDenomination>(); //new Denomination(2, 10, false)
            _mockDenomActiveFalse.SetupGet(x => x.Id).Returns(_nonActiveGameId);
            _mockDenomActiveFalse.SetupGet(x => x.Value).Returns(_nonActiveGameValue);
            _mockDenomActiveFalse.SetupGet(x => x.Active).Returns(false);
        }
    }
}
