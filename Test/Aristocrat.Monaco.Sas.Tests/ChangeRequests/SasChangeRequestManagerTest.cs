namespace Aristocrat.Monaco.Sas.Tests.ChangeRequests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.Client;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.ChangeRequests;
    using Test.Common;

    /// <summary>
    ///     Contains tests for SasChangeRequestManager
    /// </summary>
    [TestClass]
    public class SasChangeRequestManagerTest
    {
        private int TestGameId = 1;
        private GameStatus ExpectedGameStatus = GameStatus.DisabledByBackend;
        private const double ExpectedTimeout = 30_000;
        private SasChangeRequestManager _target;
        private Mock<ISasMeterChangeHandler> _sasMeterChangeHandler;
        private Mock<IGameProvider> _gameProvider;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _gameProvider = MoqServiceManager.CreateAndAddService<IGameProvider>(MockBehavior.Strict);

            _sasMeterChangeHandler = new Mock<ISasMeterChangeHandler>(MockBehavior.Strict);
            _target = new SasChangeRequestManager(_sasMeterChangeHandler.Object);
            _target.Initialize();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            _target.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullMeterChangeHandlerTest()
        {
            _target = new SasChangeRequestManager(null);
        }

        [TestMethod]
        public void NameTest()
        {
            var expectedName = typeof(SasChangeRequestManager).ToString();
            Assert.AreEqual(expectedName, _target.Name);
        }

        [TestMethod]
        public void ServiceTypesTest()
        {
            var expectedServiceType = new List<Type> { typeof(SasChangeRequestManager) };
            var actualServiceType = _target.ServiceTypes.ToList();
            CollectionAssert.AreEqual(expectedServiceType, actualServiceType);
        }

        [TestMethod]
        public void AddRequestTest()
        {
            var actualMeterCollectStatus = MeterCollectStatus.NotInPendingChange;
            var actualTimeout = 0.0;

            _sasMeterChangeHandler.Setup(s => s.StartPendingChange(It.IsAny<MeterCollectStatus>(), It.IsAny<double>()))
                .Callback((MeterCollectStatus mcs, double t) =>
                 {
                     actualMeterCollectStatus = mcs;
                     actualTimeout = t;
                 });

            _target.AddRequest(new GameEnableRequest(TestGameId, true));

            Assert.AreEqual(MeterCollectStatus.GameDenomPaytableChange, actualMeterCollectStatus);
            Assert.AreEqual(ExpectedTimeout, actualTimeout);
        }

        [DataTestMethod]
        [DataRow(true, DisplayName = "Test to commit enable game id")]
        [DataRow(false, DisplayName = "Test to commit disable game id")]
        public void CommitTest(bool enable)
        {
            var actualGameId = 0;
            var actualGameStatus = GameStatus.None;

            _sasMeterChangeHandler.Setup(s => s.StartPendingChange(It.IsAny<MeterCollectStatus>(), It.IsAny<double>()));
            _target.AddRequest(new GameEnableRequest(TestGameId, enable));

            if (enable)
            {
                _gameProvider.Setup(g => g.EnableGame(It.IsAny<int>(), It.IsAny<GameStatus>()))
                    .Callback((int id, GameStatus gameStatus) =>
                        {
                            actualGameId = id;
                            actualGameStatus = gameStatus;
                        });
            }
            else
            {
                _gameProvider.Setup(g => g.DisableGame(It.IsAny<int>(), It.IsAny<GameStatus>()))
                    .Callback((int id, GameStatus gameStatus) =>
                    {
                        actualGameId = id;
                        actualGameStatus = gameStatus;
                    });
            }

            _sasMeterChangeHandler.Raise(s => s.OnChangeCommit += null, new EventArgs());

            Assert.AreEqual(TestGameId, actualGameId);
            Assert.AreEqual(ExpectedGameStatus, actualGameStatus);
        }

        [TestMethod]
        public void CancelTest()
        {
            Mock<ISasChangeRequest> mockRequest = new Mock<ISasChangeRequest>(MockBehavior.Strict);
            mockRequest.Setup(m => m.Type).Returns(ChangeType.Meters);
            mockRequest.Setup(m => m.Cancel()).Verifiable();

            _sasMeterChangeHandler.Setup(s => s.StartPendingChange(It.IsAny<MeterCollectStatus>(), It.IsAny<double>()));
            _target.AddRequest(mockRequest.Object);
            _target.AddRequest(new GameEnableRequest(TestGameId, true));
            _sasMeterChangeHandler.Raise(s => s.OnChangeCancel += null, new EventArgs());

            mockRequest.Verify(m => m.Cancel(), Times.Once);
            mockRequest.Verify(m => m.Commit(), Times.Never);
        }
    }
}
