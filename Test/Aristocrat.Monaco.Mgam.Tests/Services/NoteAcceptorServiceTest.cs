namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Monaco.Mgam.Common.Events;
    using Mgam.Services.Attributes;
    using Mgam.Services.Devices;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class NoteAcceptorServiceTest
    {
        // Target and mocked items
        private NoteAcceptorService _target;
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>();
        private readonly Mock<IAttributeManager> _attributes = new Mock<IAttributeManager>();
        private readonly Mock<INoteAcceptor> _noteAcceptor = new Mock<INoteAcceptor>();

        private Action<AttributeChangedEvent> _subscriptionToAttributeChanged;

        [TestInitialize]
        public void Setup()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            MoqServiceManager.AddService(_noteAcceptor);

            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<AttributeChangedEvent>>(), It.IsAny<Predicate<AttributeChangedEvent>>()))
                .Callback<object, Action<AttributeChangedEvent>, Predicate<AttributeChangedEvent>>((_, callback, pred) => _subscriptionToAttributeChanged = callback);

            // Initialize the target
            _target = new NoteAcceptorService(_eventBus.Object, _attributes.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(false, true, DisplayName = "Null Event Bus Object")]
        [DataRow(true, false, DisplayName = "Null Properties Manager Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool eventBus,
            bool properties)
        {
            _target = new NoteAcceptorService(
                eventBus ? _eventBus.Object : null,
                properties ? _attributes.Object : null);
        }

        [TestMethod]
        public void WhenHostEnablesNoteAcceptorExpectEnabled()
        {
            _attributes.Setup(p => p.Get(AttributeNames.BillAcceptorEnabled, false)).Returns(true);
            _noteAcceptor.Setup(n => n.Enable(EnabledReasons.Backend)).Verifiable();
            _subscriptionToAttributeChanged(new AttributeChangedEvent(AttributeNames.BillAcceptorEnabled));
            _noteAcceptor.Verify();
        }

        [TestMethod]
        public void WhenHostDisablesNoteAcceptorExpectDisabled()
        {
            _attributes.Setup(p => p.Get(AttributeNames.BillAcceptorEnabled, false)).Returns(false);
            _noteAcceptor.Setup(n => n.Disable(DisabledReasons.Backend)).Verifiable();
            _subscriptionToAttributeChanged(new AttributeChangedEvent(AttributeNames.BillAcceptorEnabled));
            _noteAcceptor.Verify();
        }
    }
}
