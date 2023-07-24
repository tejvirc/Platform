namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.AuditTicketMonitor;
    using Contracts.Localization;
    using Contracts.Tickets;
    using Hardware.Contracts.Door;
    using Hardware.Contracts.Persistence;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using log4net;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Definition of the AuditTicketMonitor class
    ///     This class is responsible for persisting audit ticket triggered from door closed events.
    /// </summary>
    public class AuditTicketMonitor : BaseRunnable
    {
        private const string BlockName = "Aristocrat.Monaco.Application.Monitors.AuditTicketMonitor";
        private const string TicketField = "Ticket";
        private const string Ticket1Field = "Ticket1";
        private const string Ticket2Field = "Ticket2";
        private const int PrintRetryInterval = 1000;
        private const string ConfigurationExtensionPath = "/AuditTicketMonitor/Configuration";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<int, bool> _doors = new Dictionary<int, bool>();
        private readonly IEventBus _eventBus;
        private readonly IDoorMonitor _doorMonitor;
        private readonly IDoorService _doorService;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IAuditTicketCreator _auditTicketCreator;
        private readonly IVerificationTicketCreator _verificationTicketCreator;

        private readonly ConcurrentDictionary<string, Ticket> _pendingTickets =
            new ConcurrentDictionary<string, Ticket>();

        private readonly bool _useAuditTicketCreator;

        private ConcurrentQueue<IEvent> _startupEvents = new ConcurrentQueue<IEvent>();

        private IPersistentStorageAccessor _block;
        private IPrinter _printer;
        private bool _printingInProgress;
        private bool _disposed;

        public AuditTicketMonitor()
            : this(
                ServiceManager.GetInstance().GetService<IDoorMonitor>(),
                ServiceManager.GetInstance().GetService<IDoorService>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>(),
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IAuditTicketCreator>(),
                ServiceManager.GetInstance().GetService<IVerificationTicketCreator>())
        {
        }

        public AuditTicketMonitor(
            IDoorMonitor doorMonitor,
            IDoorService doorService,
            IPropertiesManager propertiesManager,
            IEventBus eventBus,
            IAuditTicketCreator auditTicketCreator,
            IVerificationTicketCreator verificationTicketCreator)
        {
            _doorMonitor = doorMonitor ?? throw new ArgumentNullException(nameof(doorMonitor));
            _doorService = doorService ?? throw new ArgumentNullException(nameof(doorService));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _auditTicketCreator = auditTicketCreator ?? throw new ArgumentNullException(nameof(auditTicketCreator));
            _verificationTicketCreator = verificationTicketCreator ??
                                         throw new ArgumentNullException(nameof(verificationTicketCreator));

            _useAuditTicketCreator = !(bool)_propertiesManager
                .GetProperty(ApplicationConstants.DetailedAuditTickets, false);
        }

        private bool IsDoorOpen => _doors.ContainsValue(true);

        /// <inheritdoc />
        protected override void OnRun()
        {
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            Logger.Debug("Start the AuditTicketMonitor...");

            if (!ServiceManager.GetInstance().IsServiceAvailable<IPrinter>())
            {
                return; // Return if Printer is not configured
            }

            _printer = ServiceManager.GetInstance().GetService<IPrinter>();
            var persistentStorageManager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();

            _pendingTickets[TicketField] = null;
            _pendingTickets[Ticket1Field] = null;
            _pendingTickets[Ticket2Field] = null;

            // Obtain the log of door events from persistent storage and
            // check if there is an existing block with this name.
            if (persistentStorageManager.BlockExists(BlockName))
            {
                _block = persistentStorageManager.GetBlock(BlockName);
                var ticket = (string)_block[TicketField];
                var ticket1 = (string)_block[Ticket1Field];
                var ticket2 = (string)_block[Ticket2Field];

                _pendingTickets[TicketField] = Deserialize(ticket);
                _pendingTickets[Ticket1Field] = Deserialize(ticket1);
                _pendingTickets[Ticket2Field] = Deserialize(ticket2);

                PrintTickets();
            }
            else
            {
                _block = persistentStorageManager.CreateBlock(PersistenceLevel.Transient, BlockName, 1);
            }

            Configure();

            SubscribeToEvents();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private string Serialize(Ticket ticket)
        {
            if (ticket == null)
            {
                return string.Empty;
            }

            using (var ms = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(typeof(Ticket));
                ser.WriteObject(ms, ticket);
                var json = ms.ToArray();
                return Encoding.UTF8.GetString(json, 0, json.Length);
            }
        }

        private async void PrintTickets()
        {
            if (_pendingTickets.Count(a => a.Value != null) <= 0 || _printingInProgress)
            {
                return;
            }

            _printingInProgress = true;

            await Print(_pendingTickets[TicketField]);
            _pendingTickets[TicketField] = null;
            _block[TicketField] = string.Empty;

            await Print(_pendingTickets[Ticket1Field]);
            _pendingTickets[Ticket1Field] = null;
            _block[Ticket1Field] = string.Empty;

            await Print(_pendingTickets[Ticket2Field]);
            _pendingTickets[Ticket2Field] = null;
            _block[Ticket2Field] = string.Empty;

            _printingInProgress = false;

            async Task Print(Ticket ticket)
            {
                if (ticket == null)
                {
                    return;
                }

                while (!await _printer.Print(ticket))
                {
                    await Task.Delay(PrintRetryInterval);
                }
            }
        }

        private Ticket Deserialize(string toDeserialize)
        {
            if (!string.IsNullOrEmpty(toDeserialize))
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(toDeserialize)))
                {
                    Logger.Debug("Deserializing audit ticket");

                    var ser = new DataContractJsonSerializer(typeof(Ticket));
                    return ser.ReadObject(ms) as Ticket;
                }
            }

            return null;
        }

        private void Configure()
        {
            var logicDoors = _doorMonitor.GetLogicalDoors();
            var jurisdiction = _propertiesManager.GetValue(ApplicationConstants.JurisdictionKey, string.Empty);

            var triggers = MonoAddinsHelper.GetSelectedNodes<TriggersNode>(ConfigurationExtensionPath)
                .FirstOrDefault(node => node.Addin.Id.Contains(jurisdiction));

            if (triggers == null)
            {
                return;
            }

            foreach (var trigger in MonoAddinsHelper.GetChildNodes<DoorTriggerNode>(triggers))
            {
                if (_doorService.LogicalDoors.All(doorIdLogicalDoorPairs => doorIdLogicalDoorPairs.Value.Name != trigger.Name))
                {
                    continue;
                }

                var logicalId = _doorService.LogicalDoors
                    .Where(doorIdLogicalDoorPairs => doorIdLogicalDoorPairs.Value.Name == trigger.Name)
                    .Select(doorIdLogicalDoorPairs => doorIdLogicalDoorPairs.Key)
                    .First();

                if (logicDoors.TryGetValue(logicalId, out var status))
                {
                    _doors.Add(logicalId, status);
                }
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<DoorOpenMeteredEvent>(this, ReceiveEvent);
            _eventBus.Subscribe<DoorClosedMeteredEvent>(this, ReceiveEvent);
            _eventBus.Subscribe<AuditTicketCreatorInitializedEvent>(this, ReceiveEvent);
        }

        private void ReceiveEvent(DoorClosedMeteredEvent data)
        {
            if (_startupEvents != null)
            {
                _startupEvents?.Enqueue(data);
                return;
            }

            HandleEvent(data);
        }

        private void HandleEvent(DoorClosedMeteredEvent data)
        {
            if (!_doors.ContainsKey(data.LogicalId))
            {
                return;
            }

            _doors[data.LogicalId] = false;
        }

        private void ReceiveEvent(DoorOpenMeteredEvent data)
        {
            if (_startupEvents != null)
            {
                _startupEvents.Enqueue(data);
                return;
            }

            HandleEvent(data);
        }

        private void HandleEvent(DoorOpenMeteredEvent data)
        {
            if (!_doors.ContainsKey(data.LogicalId))
            {
                return;
            }

            if (IsDoorOpen || _pendingTickets.Count(a => a.Value != null) > 0)
            {
                return;
            }

            _doors[data.LogicalId] = true;

            PrintAuditTickets(data);
        }

        private void PrintAuditTickets(DoorOpenMeteredEvent data = null)
        {
            if (_useAuditTicketCreator && data != null && data.LogicalId != ((int)DoorLogicalId.Logic) )
            {
                _pendingTickets[TicketField] =
                    _auditTicketCreator.CreateAuditTicket(_doorMonitor.Doors[data.LogicalId]);
            }
            else
            {
                _pendingTickets[TicketField] = _verificationTicketCreator.Create(0);
                _pendingTickets[Ticket1Field] = _verificationTicketCreator.Create(1);
                _pendingTickets[Ticket2Field] = _verificationTicketCreator.Create(2);
            }

            _block[TicketField] = Serialize(_pendingTickets[TicketField]);
            _block[Ticket1Field] = Serialize(_pendingTickets[Ticket1Field]);
            _block[Ticket2Field] = Serialize(_pendingTickets[Ticket2Field]);

            Logger.Debug("Added pending audit ticket(s).");
            PrintTickets();
        }

        private void ReceiveEvent(AuditTicketCreatorInitializedEvent @event)
        {
            while (_startupEvents.TryDequeue(out var data))
            {
                switch (data)
                {
                    case DoorOpenMeteredEvent open:
                        HandleEvent(open);
                        break;
                    case DoorClosedMeteredEvent close:
                        HandleEvent(close);
                        break;
                }
            }

            _startupEvents = null;
        }
    }
}