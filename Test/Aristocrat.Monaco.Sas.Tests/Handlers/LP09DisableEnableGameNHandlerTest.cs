namespace Aristocrat.Monaco.Sas.Tests.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Accounting.Contracts;
    using Application.Contracts.OperatorMenu;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Gaming.Contracts;
    using Gaming.Contracts.Configuration;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Handlers;

    [TestClass]
    public class LP09DisableEnableGameNHandlerTest
    {
        private const int TimeoutWait = 1000;  // one second
        private LP09DisableEnableGameNHandler _target;
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Strict);
        private readonly Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Strict);
        private readonly Mock<IGamePlayState> _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Strict);
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Strict);
        private readonly Mock<IConfigurationProvider> _gameConfigurationProvider = new Mock<IConfigurationProvider>();
        private AutoResetEvent _waiter;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var profile1 = new TestGameProfile { Id = 1, ActiveDenominations = new List<long> { 1000, 5000 }, Denominations = new List<IDenomination> { new MockDenomination(1000, 1), new MockDenomination(5000, 3) } };
            var profile2 = new TestGameProfile { Id = 2, ActiveDenominations = new List<long> { 5000 }, Denominations = new List<IDenomination> { new MockDenomination(5000, 2) } };
            _gameProvider.Setup(m => m.GetGame(1)).Returns(profile1);
            _gameProvider.Setup(m => m.GetGame(2)).Returns(profile2);
            _gameProvider.Setup(m => m.GetAllGames()).Returns(new List<IGameDetail> { profile1, profile2 });
            _gameProvider.Setup(m => m.GetGame(5)).Returns((IGameDetail)null);
            _gameProvider.Setup(m => m.ValidateConfiguration(It.IsAny<IGameDetail>(), It.IsAny<IEnumerable<long>>())).Returns(true);
            _gameProvider.Setup(m => m.ValidateConfiguration(It.IsAny<IGameDetail>())).Returns(true);
            _waiter = new AutoResetEvent(false);
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuEnteredEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuExitedEvent>>())).Verifiable();

            _bank.Setup(m => m.QueryBalance()).Returns(0);
            _gamePlayState.Setup(m => m.InGameRound).Returns(false);

            _target = new LP09DisableEnableGameNHandler(_gameProvider.Object, _bank.Object, _gamePlayState.Object, _eventBus.Object, _gameConfigurationProvider.Object);
        }


        [DataRow(true, false, false, false, false)]
        [DataRow(false, true, false, false, false)]
        [DataRow(false, false, true, false, false)]
        [DataRow(false, false, false, true, false)]
        [DataRow(false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullTest(bool nullProvider, bool nullBank, bool nullState, bool nullBus, bool nullConfig)
        {
            _target = new LP09DisableEnableGameNHandler(nullProvider ? null : _gameProvider.Object,
                                                        nullBank ? null : _bank.Object,
                                                        nullState ? null :_gamePlayState.Object,
                                                        nullBus ? null : _eventBus.Object,
                                                        nullConfig ? null : _gameConfigurationProvider.Object);
        }

        [TestMethod]
        public void CommandsTest()
        {
            Assert.AreEqual(1, _target.Commands.Count);
            Assert.IsTrue(_target.Commands.Contains(LongPoll.EnableDisableGameN));
        }

        [TestMethod]
        public void HandleDisableSingleGamePassTest()
        {
            const int targetGameId = 1;
            var data = new EnableDisableData { Enable = false, Id = targetGameId };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false };
            _gameProvider.Setup(m => m.SetActiveDenominations(targetGameId, new List<long> { 5000 })).Callback(() => _waiter.Set()).Verifiable();

            var actual = _target.Handle(data);

            // wait for the async call to finish
            Assert.IsTrue(_waiter.WaitOne(TimeoutWait)); 

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleDisableFailGameIdInvalidTest()
        {
            // indicate game id not in our list so we fail
            var data = new EnableDisableData { Enable = false, Id = 5 };
            var expected = new EnableDisableResponse { Succeeded = false, Busy = false};

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
        }

        [TestMethod]
        public void HandleEnableAllGamesFailsTest()
        {
            // indicate enabling all games so we fail
            var data = new EnableDisableData { Enable = true, Id = 0 };
            var expected = new EnableDisableResponse { Succeeded = false, Busy = false};
            _gameProvider.Setup(m => m.GetGame(0)).Returns((IGameDetail)null);

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
        }

        [TestMethod]
        public void HandleEnableTest()
        {
            const int gameId = 1;
            var profile1 = new List<IGameDetail>
            {
                new TestGameProfile
                {
                    Id = gameId, Denominations = new List<IDenomination> { new MockDenomination(1000, gameId) }, ActiveDenominations = new List<long>()
                }
            };

            var data = new EnableDisableData { Enable = true, Id = gameId };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false };
            _gameProvider.Setup(m => m.SetActiveDenominations(gameId, new List<long> { 1000 } )).Callback(() => _waiter.Set()).Verifiable();
            _gameProvider.Setup(m => m.GetAllGames()).Returns(profile1);

            var actual = _target.Handle(data);

            // wait for the async call to finish
            Assert.IsTrue(_waiter.WaitOne(TimeoutWait));

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleEnableWithMoneyOnMachineBusyTest()
        {
            var data = new EnableDisableData { Enable = true, Id = 1 };
            var expected = new EnableDisableResponse { Succeeded = false, Busy = true };
            _bank.Setup(m => m.QueryBalance()).Returns(10);

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
        }

        [TestMethod]
        public void HandleDisableGamePlayingBusyTest()
        {
            var data = new EnableDisableData { Enable = false, Id = 1 };
            var expected = new EnableDisableResponse { Succeeded = false, Busy = true };
            _gamePlayState.Setup(m => m.InGameRound).Returns(true);

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
        }

        [TestMethod]
        public void HandleInOperatorMenuBusyTest()
        {
            var data = new EnableDisableData { Enable = false, Id = 1 };
            var expected = new EnableDisableResponse { Succeeded = false, Busy = true };

            _target.InOperatorMenu = true;
            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
        }

        [TestMethod]
        public void HandleDisableGame0Test()
        {
            const int gameId = 1;
            var profile1 = new List<IGameDetail>
            {
                new TestGameProfile
                {
                    Id = gameId, Denominations = new List<IDenomination> { new MockDenomination(1000, gameId) }
                }
            };

            // indicate game 0
            var data = new EnableDisableData { Enable = false, Id = 0 };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false };
            _gameProvider.Setup(m => m.GetAllGames()).Returns(profile1);
            _gameProvider.Setup(m => m.SetActiveDenominations(gameId, new List<long>())).Callback(() => _waiter.Set()).Verifiable();
            _gameProvider.Setup(m => m.GetGame(0)).Returns((IGameDetail)null);

            var actual = _target.Handle(data);

            // wait for the async call to finish
            Assert.IsTrue(_waiter.WaitOne(TimeoutWait));

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleDisableDenomTest()
        {
            const int targetGame = 1;
            _gameProvider.Setup(m => m.SetActiveDenominations(targetGame, new List<long> { 5000 })).Callback(() => _waiter.Set()).Verifiable();

            var data = new EnableDisableData { Enable = false, Id = targetGame, TargetDenomination = 1, MultiDenomPoll = true };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false, ErrorCode = MultiDenomAwareErrorCode.NoError };

            var actual = _target.Handle(data);

            // wait for the async call to finish
            _waiter.WaitOne(TimeoutWait);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            Assert.AreEqual(expected.ErrorCode, actual.ErrorCode);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleEnableDenomTest()
        {
            const int targetGame = 2;
            var profile1 = new TestGameProfile { Id = 1, ActiveDenominations = new List<long>(), Denominations = new List<IDenomination> { new MockDenomination(1000, 1), new MockDenomination(5000, 3) } };
            var profile2 = new TestGameProfile { Id = 2, ActiveDenominations = new List<long>(), Denominations = new List<IDenomination> { new MockDenomination(5000, 2) } };
            _gameProvider.Setup(m => m.GetGame(1)).Returns(profile1);
            _gameProvider.Setup(m => m.GetGame(2)).Returns(profile2);
            _gameProvider.Setup(m => m.GetAllGames()).Returns(new List<IGameDetail> { profile1, profile2 });
            _gameProvider.Setup(m => m.SetActiveDenominations(targetGame, new List<long> { 5000 })).Verifiable();

            var data = new EnableDisableData { Enable = true, Id = targetGame, TargetDenomination = 5, MultiDenomPoll = true };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false, ErrorCode = MultiDenomAwareErrorCode.NoError };

            var actual = _target.Handle(data);

            // wait for the async call to finish
            _waiter.WaitOne(TimeoutWait);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            Assert.AreEqual(expected.ErrorCode, actual.ErrorCode);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleDisableDenomNotInGameTest()
        {
            const int targetGame = 2;

            var data = new EnableDisableData { Enable = false, Id = targetGame, TargetDenomination = 1, MultiDenomPoll = true };
            var expected = new EnableDisableResponse { Succeeded = false, Busy = false, ErrorCode = MultiDenomAwareErrorCode.SpecificDenomNotSupported };

            var actual = _target.Handle(data);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            Assert.AreEqual(expected.ErrorCode, actual.ErrorCode);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleDisableGame0WithDenomTest()
        {
            _gameProvider.Setup(m => m.GetGame(0)).Returns((IGameDetail)null);
            _gameProvider.Setup(m => m.SetActiveDenominations(1, new List<long> { 1000 })).Verifiable();
            _gameProvider.Setup(m => m.SetActiveDenominations(2, new List<long>())).Verifiable();

            var data = new EnableDisableData { Enable = false, Id = 0, TargetDenomination = 5, MultiDenomPoll = true };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false, ErrorCode = MultiDenomAwareErrorCode.NoError };

            var actual = _target.Handle(data);

            _waiter.WaitOne(TimeoutWait);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            Assert.AreEqual(expected.ErrorCode, actual.ErrorCode);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleDisableGame0WithNonSharedDenomTest()
        {
            _gameProvider.Setup(m => m.GetGame(0)).Returns((IGameDetail)null);
            _gameProvider.Setup(m => m.SetActiveDenominations(1, new List<long> { 5000 })).Verifiable();

            var data = new EnableDisableData { Enable = false, Id = 0, TargetDenomination = 1, MultiDenomPoll = true };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false, ErrorCode = MultiDenomAwareErrorCode.NoError };

            var actual = _target.Handle(data);

            // no real chance for a callback so this test will just take awhile
            _waiter.WaitOne(TimeoutWait);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            Assert.AreEqual(expected.ErrorCode, actual.ErrorCode);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void HandleDisableGame0WithZeroDenomTest()
        {
            _gameProvider.Setup(m => m.GetGame(0)).Returns((IGameDetail)null);
            _gameProvider.Setup(m => m.SetActiveDenominations(1, new List<long>())).Verifiable();
            _gameProvider.Setup(m => m.SetActiveDenominations(2, new List<long>())).Verifiable();

            var data = new EnableDisableData { Enable = false, Id = 0, TargetDenomination = 0, MultiDenomPoll = true };
            var expected = new EnableDisableResponse { Succeeded = true, Busy = false, ErrorCode = MultiDenomAwareErrorCode.NoError };

            var actual = _target.Handle(data);

            _waiter.WaitOne(TimeoutWait);

            Assert.AreEqual(expected.Succeeded, actual.Succeeded);
            Assert.AreEqual(expected.Busy, actual.Busy);
            Assert.AreEqual(expected.ErrorCode, actual.ErrorCode);
            _gameProvider.Verify();
        }

        [TestMethod]
        public void DisposeTest()
        {
            _eventBus.Setup(m => m.UnsubscribeAll(It.IsAny<LP09DisableEnableGameNHandler>())).Verifiable();

            _target.Dispose();

            // call again to test already disposed path
            _target.Dispose();

            _eventBus.Verify();
        }

        [TestMethod]
        public void EventBusSubscribeOperatorMenuEnteredEventTest()
        {
            Action<OperatorMenuEnteredEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuEnteredEvent>>()))
                .Callback(
                    (object subscriber, Action<OperatorMenuEnteredEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target = new LP09DisableEnableGameNHandler(
                    _gameProvider.Object,
                    _bank.Object,
                    _gamePlayState.Object,
                    _eventBus.Object,
                    _gameConfigurationProvider.Object)
                { InOperatorMenu = false };

            callback.Invoke(new OperatorMenuEnteredEvent());
            Assert.IsTrue(_target.InOperatorMenu);
        }

        [TestMethod]
        public void EventBusSubscribeOperatorMenuExitedEventTest()
        {
            Action<OperatorMenuExitedEvent> callback = null;
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<OperatorMenuExitedEvent>>()))
                .Callback(
                    (object subscriber, Action<OperatorMenuExitedEvent> eventCallback) =>
                    {
                        callback = eventCallback;
                    });

            _target = new LP09DisableEnableGameNHandler(
                    _gameProvider.Object,
                    _bank.Object,
                    _gamePlayState.Object,
                    _eventBus.Object,
                    _gameConfigurationProvider.Object)
                { InOperatorMenu = true };

            callback.Invoke(new OperatorMenuExitedEvent());
            Assert.IsFalse(_target.InOperatorMenu);
        }
    }
}
