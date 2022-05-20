namespace Aristocrat.Monaco.Mgam.Tests.GamePlay
{
    using System;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Mgam.Common.Events;
    using Gaming.Contracts;
    using Kernel;
    using Mgam.Services.Attributes;
    using Mgam.Services.GamePlay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class AutoPlayTest
    {
        private Action<AttributeChangedEvent> _handler;
        private Mock<IAttributeManager> _attributes;
        private Mock<IEventBus> _eventBus;
        private Mock<IAutoPlayStatusProvider> _autoPlay;
        private Predicate<AttributeChangedEvent> _filter;
        private bool _autoPlayPropValue;

        [TestInitialize]
        public void Initialize()
        {
            _attributes = new Mock<IAttributeManager>(MockBehavior.Strict);

            _attributes.Setup(x => x.Get(AttributeNames.AutoPlay, It.IsAny<bool>()))
                .Returns(() => _autoPlayPropValue);

            _eventBus = new Mock<IEventBus>(MockBehavior.Strict);

            _eventBus.Setup(x => x.UnsubscribeAll(It.IsAny<object>()));
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GamePlayEnabledEvent>>()));
            _eventBus.Setup(
                    x => x.Subscribe(
                        It.IsAny<object>(),
                        It.IsAny<Action<AttributeChangedEvent>>(),
                        It.IsAny<Predicate<AttributeChangedEvent>>()))
                .Callback<object, Action<AttributeChangedEvent>, Predicate<AttributeChangedEvent>>(
                    (context, handler, filter) =>
                    {
                        _handler = handler;
                        _filter = filter;
                    });

            _autoPlay = new Mock<IAutoPlayStatusProvider>(MockBehavior.Strict);

            _autoPlay.Setup(x => x.StartSystemAutoPlay());

            _autoPlay.Setup(x => x.StopSystemAutoPlay());

            _autoPlay.Setup(x => x.EndAutoPlayIfActive())
                .Returns(true);

            new AutoPlay(_eventBus.Object, _attributes.Object, _autoPlay.Object);
        }

        [TestMethod]
        public void EnableAutoPlayTest()
        {
            _autoPlayPropValue = true;

            var e = new AttributeChangedEvent(AttributeNames.AutoPlay);

            if (_filter(e))
            {
                _handler.Invoke(e);
            }

            _attributes.Verify(x => x.Get(AttributeNames.AutoPlay, It.IsAny<bool>()), Times.AtLeastOnce);

            _autoPlay.Verify(x => x.StartSystemAutoPlay(), Times.Once);
        }

        [TestMethod]
        public void DisableAutoPlayTest()
        {
            _autoPlayPropValue = false;

            var e = new AttributeChangedEvent(AttributeNames.AutoPlay);

            if (_filter(e))
            {
                _handler.Invoke(e);
            }

            _attributes.Verify(x => x.Get(AttributeNames.AutoPlay, It.IsAny<bool>()), Times.AtLeastOnce);

            _autoPlay.Verify(x => x.EndAutoPlayIfActive(), Times.Once);
        }

        [TestMethod]
        public void NonAutoPlayPropertyChangedTest()
        {
            _autoPlayPropValue = true;

            var e = new AttributeChangedEvent(AttributeNames.LocationName);

            if (_filter(e))
            {
                _handler.Invoke(e);
            }

            _autoPlay.Verify(x => x.StartSystemAutoPlay(), Times.Never);
            _autoPlay.Verify(x => x.EndAutoPlayIfActive(), Times.Never);
        }
    }
}
