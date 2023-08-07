namespace Aristocrat.Monaco.Application.Tickets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using Contracts;
    using Contracts.Tickets;
    using Contracts.TiltLogger;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using log4net;

    /// <summary>
    ///     Definition of the EventLogTicketCreator class
    /// </summary>
    public class EventLogTicketCreator : IEventLogTicketCreator
    {
        private readonly int _maxEventsPerPage;
        private readonly int _maxLinesPerPage;
        private readonly EventLogTicket _eventLogTicket;

        /// <summary>
        ///     Create a logger for use in this class.
        /// </summary>
        protected static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     constructor
        /// </summary>
        public EventLogTicketCreator()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _maxEventsPerPage = properties.GetValue(ApplicationConstants.AuditTicketEventsPerPage, 6);

            _eventLogTicket = new EventLogTicket();
            _maxLinesPerPage = properties.GetValue(ApplicationConstants.AuditTicketLineLimit, 36) - _eventLogTicket.HeaderFooterLineCount;
        }

        public Ticket Create(int page, Collection<EventDescription> events)
        {
            _eventLogTicket.Initialize(events, EventsPerPage, page);
            return CreateTicket(events.Count, _eventLogTicket);
        }

        public Ticket CreateTicket(int eventCount, TextTicket ticket)
        {
            if (eventCount > EventsPerPage)
            {
                Logger.Warn($"Too many events; only the first {EventsPerPage} events of {eventCount} will be printed");
            }

            return ticket.CreateTextTicket();
        }

        /// <inheritdoc />
        public virtual string Name => "Event Log Ticket Creator";

        /// <inheritdoc />
        public virtual ICollection<Type> ServiceTypes => new[] { typeof(IEventLogTicketCreator) };

        /// <summary>
        ///     The number of events that will fit on one page based on max lines per page
        /// </summary>
        public int EventsPerPage => ItemLineLength > 0 ? _maxLinesPerPage / ItemLineLength : _maxEventsPerPage;

        /// <summary>
        ///     The number of lines one item requires to print (default is 4)
        /// </summary>
        public virtual int ItemLineLength { get; } = 4;

        public void Initialize()
        {
        }
    }
}