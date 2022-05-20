namespace Aristocrat.Monaco.Gaming.UI.Tests.PlayerInfoDisplay
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Contracts;
    using Contracts.Events;
    using Contracts.PlayerInfoDisplay;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Runtime;
    using Runtime.Client;
    using UI.PlayerInfoDisplay;

    [TestClass]
    public class DefaultPlayerInfoDisplayManagerTests
    {
        private DefaultPlayerInfoDisplayManager _underTest;
        private Mock<IEventBus> _eventBus;
        private Mock<IRuntime> _runtime;
        private Action<HandpayStartedEvent> _handlerHandpayStartedEvent;
        private Action<VoucherOutStartedEvent> _handlerVoucherOutStartedEvent;
        private Action<CurrencyInStartedEvent> _handlerCurrencyInStartedEvent;
        private Action<VoucherRedemptionRequestedEvent> _handlerVoucherRedemptionRequestedEvent;
        private Action<PlayerInfoButtonPressedEvent> _handlerPlayerInfoButtonPressedEvent;
        private Action<PlayerInfoDisplayExitRequestEvent> _handlerPlayerInfoDisplayExitedEvent;
        private Action<GameSelectedEvent> _handlerGameSelectedEvent;
        private Mock<IPlayerInfoDisplayViewModel> _mainPage;
        private Mock<IGameResourcesModelProvider> _gameResourcesModelProvider;

        public DefaultPlayerInfoDisplayManagerTests()
        {
            if (System.Windows.Application.Current == null)
            {
                Activator.CreateInstance(typeof(System.Windows.Application));
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _eventBus = new Mock<IEventBus>();
            _runtime = new Mock<IRuntime>();
            _gameResourcesModelProvider = new Mock<IGameResourcesModelProvider>();

            _eventBus.Setup(x => x.Subscribe(It.IsAny<IPlayerInfoDisplayManager>(), It.IsAny<Action<HandpayStartedEvent>>()))
                .Callback((object x, Action<HandpayStartedEvent> y) => _handlerHandpayStartedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<IPlayerInfoDisplayManager>(), It.IsAny<Action<VoucherOutStartedEvent>>()))
                .Callback((object x, Action<VoucherOutStartedEvent> y) => _handlerVoucherOutStartedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<IPlayerInfoDisplayManager>(), It.IsAny<Action<CurrencyInStartedEvent>>()))
                .Callback((object x, Action<CurrencyInStartedEvent> y) => _handlerCurrencyInStartedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<IPlayerInfoDisplayManager>(), It.IsAny<Action<VoucherRedemptionRequestedEvent>>()))
                .Callback((object x, Action<VoucherRedemptionRequestedEvent> y) => _handlerVoucherRedemptionRequestedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<IPlayerInfoDisplayManager>(), It.IsAny<Action<PlayerInfoButtonPressedEvent>>()))
                .Callback((object x, Action<PlayerInfoButtonPressedEvent> y) => _handlerPlayerInfoButtonPressedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<IPlayerInfoDisplayManager>(), It.IsAny<Action<PlayerInfoDisplayExitRequestEvent>>()))
                .Callback((object x, Action<PlayerInfoDisplayExitRequestEvent> y) => _handlerPlayerInfoDisplayExitedEvent = y);

            _eventBus.Setup(x => x.Subscribe(It.IsAny<IPlayerInfoDisplayManager>(), It.IsAny<Action<GameSelectedEvent>>()))
                .Callback((object x, Action<GameSelectedEvent> y) => _handlerGameSelectedEvent = y);

            _mainPage = new Mock<IPlayerInfoDisplayViewModel>();
            _mainPage.Setup(x => x.PageType).Returns(PageType.Menu);

            _underTest = new DefaultPlayerInfoDisplayManager(
                _eventBus.Object,
                _runtime.Object,
                _gameResourcesModelProvider.Object);
        }

        [TestCleanup]
        public void TearDown()
        {
        }

        [TestMethod]
        public void NullConstructorTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultPlayerInfoDisplayManager(null, _runtime.Object, _gameResourcesModelProvider.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultPlayerInfoDisplayManager(_eventBus.Object, null, _gameResourcesModelProvider.Object));
            Assert.ThrowsException<ArgumentNullException>(() => new DefaultPlayerInfoDisplayManager(_eventBus.Object, _runtime.Object, null));
        }

        [TestMethod]
        public void GivenPagesWhenAddPagesThenOldPagesRemoved()
        {
            var page1 = new Mock<IPlayerInfoDisplayViewModel>();
            _underTest.AddPages(new [] { page1.Object });
            page1.Verify(x => x.Hide(), Times.Never);

            var pageDifferent1 = new Mock<IPlayerInfoDisplayViewModel>();

            _underTest.AddPages(new [] { pageDifferent1.Object });
            pageDifferent1.Verify(x => x.Hide(), Times.Never);
            page1.Verify(x => x.Hide(), Times.Once);
        }

        [TestMethod]
        public void GivenPagesWhenDisposeAllPagesHidden()
        {
            var page1 = new Mock<IPlayerInfoDisplayViewModel>();
            _underTest.AddPages(new [] { page1.Object});
            _underTest.Dispose();
            page1.Verify(x => x.Hide(), Times.Once);
        }

        [TestMethod]
        public void GivenPagesWhenDisposeThenUnsubscribeAll()
        {
            var page1 = new Mock<IPlayerInfoDisplayViewModel>();
            var page2 = new Mock<IPlayerInfoDisplayViewModel>();
            _underTest.AddPages(new [] { page1.Object, page2.Object });
            _underTest.Dispose();
            _eventBus.Verify(x => x.UnsubscribeAll(It.Is<IPlayerInfoDisplayManager>(p => p == _underTest)), Times.Once);
        }

        [TestMethod]
        public void GivenPlayerInfoButtonPressedEventWhenPublishThenMenuShow()
        {
            _underTest.AddPages(new [] { _mainPage.Object });

            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            _mainPage.Verify(x => x.Show(), Times.Once);
        }

        [TestMethod]
        public void GivenMainPageWhenButtonClickedExitThenHidePage()
        {
            _underTest.AddPages(new [] { _mainPage.Object });
            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());
            _mainPage.Verify(x => x.Show());

            _mainPage.Raise(e => e.ButtonClicked += null, new CommandArgs(CommandType.Exit));

            _mainPage.Verify(x => x.Hide());
        }

        [TestMethod]
        public void GivenMainPageWhenButtonClickedExitThenRunTimeNotified()
        {
            _runtime.Setup(x => x.Connected).Returns(true);
            _underTest.AddPages(new [] { _mainPage.Object });
            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());
            _mainPage.Verify(x => x.Show());

            _mainPage.Raise(e => e.ButtonClicked += null, new CommandArgs(CommandType.Exit));

            _runtime.Verify(x => x.UpdateFlag(RuntimeCondition.InPlayerInfoDisplayMenu, false));
            _eventBus.Verify(x => x.Publish(It.IsAny<PlayerInfoDisplayExitedEvent>()));
        }


        [TestMethod]
        public void GivenPlayerInfoButtonPressedEventWhenPublishNoMainPageThenNoMenu()
        {
            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            _mainPage.Verify(x => x.Show(), Times.Never);
        }

        [TestMethod]
        public void GivenPlayerInfoDisplayExitedEventWhenPublishThenNotifyRuntime()
        {
            _underTest.AddPages(new [] { _mainPage.Object });

            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            Assert.IsTrue(_underTest.IsActive());

            _runtime.Setup(x => x.Connected).Returns(true);
            _handlerPlayerInfoDisplayExitedEvent.Invoke(new PlayerInfoDisplayExitRequestEvent());

            _runtime.Verify(x => x.UpdateFlag(RuntimeCondition.InPlayerInfoDisplayMenu, false));
        }

        [TestMethod]
        public void GivenPlayerInfoButtonPressedEventWhenPublishThenRunTimeNotified()
        {
            _underTest.AddPages(new [] { _mainPage.Object });

            _runtime.Setup(x => x.Connected).Returns(true);
            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            _runtime.Verify(x => x.UpdateFlag(RuntimeCondition.InPlayerInfoDisplayMenu, true));
        }

        [TestMethod]
        public void GivenPlayerInfoButtonPressedEventWhenPublishRunTimeNotConnectedThenNoRunTimeNotified()
        {
            _underTest.AddPages(new [] { _mainPage.Object });

            _runtime.Setup(x => x.Connected).Returns(false);
            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            _runtime.Verify(x => x.UpdateFlag(It.IsAny<RuntimeCondition>(), It.IsAny<bool>()), Times.Never);
        }

        [TestMethod]
        public void GivenPlayerInfoButtonPressedEventWhenPublishThenActive()
        {
            _underTest.AddPages(new [] { _mainPage.Object });

            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            Assert.IsTrue(_underTest.IsActive());
        }

        [TestMethod]
        public void GivenPlayerInfoButtonPressedEventWhenPublishThenEnteredEvent()
        {
            _underTest.AddPages(new [] { _mainPage.Object });

            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            _eventBus.Verify(x => x.Publish(It.IsAny<PlayerInfoDisplayEnteredEvent>()));

        }

        [TestMethod]
        public void GivenGameSelectedEventWhenPublishThenResourcePassedToViewModel()
        {
            var id = 2;
            var denomination = 3L;

            var model = Mock.Of<IPlayInfoDisplayResourcesModel>();
            _gameResourcesModelProvider.Setup(x => x.Find(id))
                .Returns(model)
                .Verifiable();

            var pages = new[]
            {
                new Mock<IPlayerInfoDisplayViewModel>()
            };
            _underTest.AddPages(pages.Select(x => x.Object).ToArray());

            _handlerGameSelectedEvent.Invoke(new GameSelectedEvent(
                id
                ,denomination
                ,Guid.NewGuid().ToString()
                ,false
                ,IntPtr.Zero
                ,IntPtr.Zero
                ,IntPtr.Zero
                ,IntPtr.Zero
                ));


            foreach (var p in pages)
            {
                p.Verify(x => x.SetupResources(model), Times.Once);
            }

            _gameResourcesModelProvider.Verify();
        }

        [TestMethod]
        [DataRow(typeof(HandpayStartedEvent))]
        [DataRow(typeof(VoucherOutStartedEvent))]
        [DataRow(typeof(CurrencyInStartedEvent))]
        [DataRow(typeof(VoucherRedemptionRequestedEvent))]
        public void GivenManagerActiveWhenTransferOutStartedEventThenExit(Type type)
        {
            _underTest.AddPages(new [] { _mainPage.Object });

            _handlerPlayerInfoButtonPressedEvent.Invoke(new PlayerInfoButtonPressedEvent());

            Assert.IsTrue(_underTest.IsActive());

            if (typeof(HandpayStartedEvent) == type)
            {
                _handlerHandpayStartedEvent.Invoke(new HandpayStartedEvent(HandpayType.BonusPay, 1, 2, 3, true));
            }
            else if (typeof(VoucherOutStartedEvent) == type)
            {
                _handlerVoucherOutStartedEvent.Invoke(new VoucherOutStartedEvent(1));
            }
            else if (typeof(VoucherRedemptionRequestedEvent) == type)
            {
                _handlerVoucherRedemptionRequestedEvent.Invoke(new VoucherRedemptionRequestedEvent(new VoucherInTransaction()));
            }
            else if (typeof(CurrencyInStartedEvent) == type)
            {
                _handlerCurrencyInStartedEvent.Invoke(new CurrencyInStartedEvent(Mock.Of<INote>()));
            }
            else
            {
                throw new Exception($@"Unexpected type {type}");
            }


            Assert.IsFalse(_underTest.IsActive());

            _mainPage.Verify(x => x.Hide(), Times.Once);

        }
    }
}