namespace Aristocrat.Monaco.Mgam.Services.Devices
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Mgam.Client;
    using Attributes;
    using Common.Events;
    using Hardware.Contracts.NoteAcceptor;
    using Hardware.Contracts.SharedDevice;
    using Kernel;

    public class NoteAcceptorService : IService
    {
        private readonly IEventBus _eventBus;
        private readonly IAttributeManager _attributes;
        private readonly INoteAcceptor _noteAcceptor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoteAcceptorService"/> class
        ///     that allows MGAM host to interact with hardware devices
        /// </summary>
        public NoteAcceptorService(
            IEventBus eventBus,
            IAttributeManager attributes)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            _noteAcceptor = ServiceManager.GetInstance().TryGetService<INoteAcceptor>();

            SubscribeToEvents();
        }

        /// <inheritdoc />
        public string Name => typeof(NoteAcceptorService).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(NoteAcceptorService) };

        /// <inheritdoc />
        public void Initialize()
        {
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<AttributeChangedEvent>(
                this,
                _ => EnableNoteAcceptor(),
                e => e.AttributeName == AttributeNames.BillAcceptorEnabled);
            _eventBus.Subscribe<AttributesUpdatedEvent>(this, _ => EnableNoteAcceptor());
        }

        private void EnableNoteAcceptor()
        {
            // This property has changed in the host, so disable or enable the note acceptor accordingly
            var enabled = _attributes.Get(AttributeNames.BillAcceptorEnabled, false);
            if (enabled)
            {
                _noteAcceptor?.Enable(EnabledReasons.Backend);
            }
            else
            {
                _noteAcceptor?.Disable(DisabledReasons.Backend);
            }
        }
    }
}
