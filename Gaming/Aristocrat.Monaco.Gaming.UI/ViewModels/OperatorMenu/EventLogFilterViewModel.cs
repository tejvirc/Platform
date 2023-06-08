namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.Contracts.Tickets;
    using Application.Contracts.TiltLogger;
    using Application.UI.Events;
    using Application.UI.Models;
    using Application.UI.OperatorMenu;
    using Application.UI.Views;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using MVVM;
    using MVVM.Command;

    [CLSCompliant(false)]
    public partial class EventLogFilterViewModel : OperatorMenuPageViewModelBase
    {
        private readonly object _eventLogEventHandlerLock = new object();
        private readonly int _loggerConfigurationCount;
        private readonly int _loggerSubscriptions;
        private readonly List<EventLog> _appendedEvents = new List<EventLog>();
        private readonly object _appendedLock = new object();
        private string _eventCount;
        private EventLog _selectedItem;
        private string _subscriptionStatus;
        private bool _subscriptionTextVisible = true;
        private TimeSpan _offset;
        private readonly ITiltLogger _tiltLogger;
        private readonly IPropertiesManager _propertiesManager;
        private ObservableCollection<EventLog> _eventLogCollection = new ObservableCollection<EventLog>();
        private const int DefaultLogMaxEntries = 500;

        private ObservableCollection<EventFilterInfo> _eventFilterCollection;
        private readonly IReadOnlyCollection<IEventLogAdapter> _eventLogAdapters;
        private bool _filterMenuEnabled;

        private bool _isAllFiltersSelected;
        private bool _settingFilterSelections;
        public IActionCommand AllFiltersSelectedCommand { get; }
        public IActionCommand FilterSelectedCommand { get; }

        public EventLogFilterViewModel()
            : base(true)
        {
            _tiltLogger = ServiceManager.GetInstance().GetService<ITiltLogger>();
            _propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            _loggerSubscriptions = _tiltLogger.GetEventsSubscribed(string.Empty);
            _loggerConfigurationCount = _tiltLogger.GetEventsToSubscribe(string.Empty);

            ShowAdditionalInfoCommand = new ActionCommand<object>(ShowAdditionalInfo);

            IsAllFiltersSelected = true;
            AllFiltersSelectedCommand = new ActionCommand<object>(_ => AllFiltersSelected());
            FilterSelectedCommand = new ActionCommand<object>(_ => FilterSelected());

            FilterMenuEnabled = false;
            _eventLogAdapters = GetLogAdapters();
            EventFilterCollection = CreateFilters();
        }

        public ICommand ShowAdditionalInfoCommand { get; }

        public ObservableCollection<EventLog> EventLogCollection
        {
            get => _eventLogCollection;
            set
            {
                _eventLogCollection = value;
                RaisePropertyChanged(nameof(EventLogCollection));
                RaisePropertyChanged(nameof(FilteredLogCollection));
                RaisePropertyChanged(nameof(PrintSelectedButtonEnabled));
                RaisePropertyChanged(nameof(DataEmpty));
            }
        }

        public int MaxEntries { get; set; }

        public TimeSpan Offset
        {
            get => _offset;
            private set
            {
                if (_offset != value)
                {
                    _offset = value;
                    RaisePropertyChanged(nameof(Offset));
                }
            }
        }

        public string SubscriptionStatus
        {
            get => _subscriptionStatus;

            set
            {
                _subscriptionStatus = value;
                RaisePropertyChanged(nameof(SubscriptionStatus));
            }
        }

        public string EventCount
        {
            get => _eventCount;

            set
            {
                _eventCount = value;
                RaisePropertyChanged(nameof(EventCount));
            }
        }

        public EventLog SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                RaisePropertyChanged(nameof(PrintSelectedButtonEnabled));
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [Subscription Text visible].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [Subscription Text visible]; otherwise, <c>false</c>.
        /// </value>
        public bool SubscriptionTextVisible
        {
            get => _subscriptionTextVisible;

            set
            {
                _subscriptionTextVisible = value;
                RaisePropertyChanged(nameof(SubscriptionTextVisible));
            }
        }

        public override bool MainPrintButtonEnabled =>
            base.MainPrintButtonEnabled && FilteredLogCollection.Any();

        /// <summary>
        ///     Gets or sets a value indicating whether [print selected button enabled].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [print selected button enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool PrintSelectedButtonEnabled =>
            SelectedItem != null && PrinterButtonsEnabled && FilteredLogCollection.Any();

        public override bool DataEmpty => EventLogCollection == null || !EventLogCollection.Any();

        public ObservableCollection<EventFilterInfo> EventFilterCollection
        {
            get => _eventFilterCollection;
            set
            {
                _eventFilterCollection = value;
                RaisePropertyChanged(nameof(EventFilterCollection));
                RaisePropertyChanged(nameof(FilteredLogCollection));
                RaisePropertyChanged(nameof(PrintSelectedButtonEnabled));
                RaisePropertyChanged(nameof(MainPrintButtonEnabled));
            }
        }

        public bool FilterMenuEnabled
        {
            get => _filterMenuEnabled;
            set
            {
                if (value == _filterMenuEnabled)
                {
                    return;
                }

                _filterMenuEnabled = value;
                RaisePropertyChanged(nameof(FilterMenuEnabled));
            }
        }

        public IEnumerable<EventLog> FilteredLogCollection => EventFilterCollection.Where(filter => filter.IsSelected)
            .SelectMany(
                filter => EventLogCollection.Where(
                    log => log.Description.Type.Equals(filter.EventType)))
            .OrderByDescending(evt => evt.Description.TransactionId)
            .Take(MaxEntries);

        private void EventLogCollection_OnCollectionChanged(object o, NotifyCollectionChangedEventArgs args)
        {
            UpdateEventCount();
            RaisePropertyChanged(nameof(FilteredLogCollection));
        }

        private void UpdateEventCount()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    var count = FilteredLogCollection?.Count() ?? 0;
                    EventCount = string.Format(
                        CultureInfo.CurrentCulture,
                        "  " + (count == 1
                            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EventInLog)
                            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EventsInLog)),
                        count);
                    RaisePropertyChanged(nameof(DataEmpty), nameof(FilteredLogCollection));
                });
        }

        private void UpdateMaxEntries()
        {
            var loggerType = IsAllFiltersSelected
                ? string.Empty
                : string.Join("|", EventFilterCollection.Where(f => f.IsSelected).Select(f => f.EventType));

            MaxEntries = GetMaxEntries(loggerType);
        }

        private int GetMaxEntries(string loggerType)
        {
            var max = _tiltLogger.GetMax(loggerType);
            Logger.Debug($"Max entries for type {loggerType} = {max}");
            return max == 0 ? DefaultLogMaxEntries : max;
        }

        private List<Ticket> GenerateLogTickets(OperatorMenuPrintData dataType)
        {
            var logs = GetItemsToPrint(FilteredLogCollection.ToList(), dataType).ToList();

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IEventLogTicketCreator>();
            if (ticketCreator == null)
            {
                Logger.Info("Couldn't find ticket creator");
                return null;
            }

            var tickets = new List<Ticket>();
            var printNumberOfPages = (int)Math.Ceiling((double)logs.Count / ticketCreator.EventsPerPage);
            for (var page = 0; page < printNumberOfPages; page++)
            {
                var singlePageLogs = new Collection<EventDescription>(
                    logs.Skip(page * ticketCreator.EventsPerPage).Take(ticketCreator.EventsPerPage).Select(e => e.Description).ToList());
                var ticket = ticketCreator.Create(page + 1, singlePageLogs);
                tickets.Add(ticket);
            }

            return tickets;
        }

        protected override void DisposeInternal()
        {
            SetupTiltLogAppendedTilt(false);
            ClearEventLogCollection();
            base.DisposeInternal();
        }

        private void LoadEventHistory()
        {
            var events = new List<EventDescription>(_tiltLogger.GetEvents(string.Empty));
            events.AddRange(GetTransactionHistoryEvents());
            events = events.Distinct().ToList();

            UpdateMaxEntries();

            var eventLogs = new List<EventLog>(events.Select(e => new EventLog(e)));

            EventLogCollection.CollectionChanged -= EventLogCollection_OnCollectionChanged;
            EventLogCollection =
                new ObservableCollection<EventLog>(eventLogs.OrderByDescending(e => e.Description.TransactionId));
            EventLogCollection.CollectionChanged += EventLogCollection_OnCollectionChanged;
            UpdateEventCount();

            // Add any events that have occurred during initial load.
            lock (_appendedLock)
            {
                foreach (var e in _appendedEvents)
                {
                    InsertEvent(e);
                }

                _appendedEvents.Clear();
            }
        }

        private void ReloadEventHistory()
        {
            FilterMenuEnabled = false;

            MvvmHelper.ExecuteOnUI(() => { IsLoadingData = true; });

            Task.Run(InitializeData);
        }

        private void EventLogAppended(object sender, TiltLogAppendedEventArgs e)
        {
            if (e?.Message != null)
            {
                if (EventLogCollection == null)
                {
                    lock (_appendedLock)
                    {
                        _appendedEvents.Add(new EventLog(e.Message));
                    }

                    return;
                }

                InsertEvent(new EventLog(e.Message), true);
            }
        }

        private void InsertEvent(EventLog message, bool deleteOldMessages = false)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (message != null)
                    {
                        EventLogCollection.Insert(0, message);

                        if (deleteOldMessages)
                        {
                            // No need to store more than the max number of current log type (combined) entries in the collection
                            var maxEntriesCurrentType = GetMaxEntries(message.Description.Type);
                            var combinedList = _tiltLogger.GetCombinedTypes(message.Description.Type);
                            var logCollection = combinedList != null
                                ? EventLogCollection.Where(x => combinedList.Contains(x.Description.Type))
                                : EventLogCollection.Where(x => x.Description.Type == message.Description.Type);
                            var logsToExclude = logCollection.Skip(maxEntriesCurrentType).ToList();
                            logsToExclude.ForEach(x => EventLogCollection.Remove(x));
                            var typeString = combinedList == null
                                ? message.Description.Type
                                : string.Join("-", combinedList);
                            Logger.Debug($"Removed {logsToExclude.Count} logs from combined types {typeString} with max {maxEntriesCurrentType}");
                        }
                    }
                });
        }

        private IEnumerable<EventDescription> GetTransactionHistoryEvents()
        {
            var events = new List<EventDescription>();

            foreach (var filter in EventFilterCollection)
            {
                var logAdapter = _eventLogAdapters.FirstOrDefault(l => l.LogType == filter.EventType);
                if (logAdapter != null)
                {
                    events.AddRange(logAdapter.GetEventLogs());
                }
            }

            return events;
        }

        protected override void InitializeData()
        {
            UpdateEventCount();
            LoadEventHistory();

            SubscriptionStatus = string.Format(
                CultureInfo.CurrentCulture,
                _loggerConfigurationCount == 1
                    ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WatchingEvent)
                    : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WatchingEvents),
                _loggerConfigurationCount);

            if (_loggerConfigurationCount > _loggerSubscriptions)
            {
                var improperConfig = _loggerConfigurationCount - _loggerSubscriptions;
                SubscriptionStatus += string.Format(
                    CultureInfo.CurrentCulture,
                    "  " + (improperConfig == 1
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImproperConfig)
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ImproperConfigs)),
                    improperConfig);
            }

            FilterMenuEnabled = true;
            IsLoadingData = false;
        }

        protected override void OnLoaded()
        {
            UpdateEventCount();

            Offset = PropertiesManager.GetValue(ApplicationConstants.TimeZoneOffsetKey, TimeSpan.Zero);

            ReloadEventHistory();

            SetupTiltLogAppendedTilt(true);

            EventBus.Subscribe<TimeZoneUpdatedEvent>(this, HandleTimeZoneChangedEvent);
            SubscriptionTextVisible = GetConfigSetting(OperatorMenuSetting.ShowSubscriptionText, true);
            SelectedItem = null;
            RaisePropertyChanged(nameof(PrintCurrentPageButtonVisible));
            RaisePropertyChanged(nameof(PrintSelectedButtonVisible));
            RaisePropertyChanged(nameof(PrintLast15ButtonVisible));
            EventBus.Subscribe<OperatorMenuPrintJobStartedEvent>(this, o => FilterMenuEnabled = false);
            EventBus.Subscribe<OperatorMenuPrintJobCompletedEvent>(this, o => FilterMenuEnabled = true);
        }

        protected override void OnUnloaded()
        {
            SetupTiltLogAppendedTilt(false);
        }

        protected override void UpdatePrinterButtons()
        {
            RaisePropertyChanged(nameof(PrintSelectedButtonEnabled));
            RaisePropertyChanged(nameof(MainPrintButtonEnabled));
        }

        private void SetupTiltLogAppendedTilt(bool add)
        {
            lock (_eventLogEventHandlerLock)
            {
                if (add)
                {
                    _tiltLogger.TiltLogAppendedTilt += EventLogAppended;

                    foreach (var logAdapter in _eventLogAdapters.Where(e => e is ISubscribableEventLogAdapter))
                    {
                        ((ISubscribableEventLogAdapter)logAdapter).Appended += EventLogAppended;
                    }
                }
                else
                {
                    _tiltLogger.TiltLogAppendedTilt -= EventLogAppended;

                    foreach (var logAdapter in _eventLogAdapters.Where(e => e is ISubscribableEventLogAdapter))
                    {
                        ((ISubscribableEventLogAdapter)logAdapter).Appended -= EventLogAppended;
                    }
                }
            }
        }

        private Ticket GenerateSelectedEventTicket()
        {
            if (_selectedItem == null)
            {
                return null;
            }

            var selectedEvent = new Collection<EventDescription> { _selectedItem.Description };
            var ticketCreator = ServiceManager.GetInstance().TryGetService<IEventLogTicketCreator>();

            return ticketCreator?.Create(1, selectedEvent);
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                case OperatorMenuPrintData.CurrentPage:
                case OperatorMenuPrintData.Last15:
                    var selectedFilters = EventFilterCollection.Where(
                        e => e.IsSelected).ToList();
                    // If single filter is selected and we have a special print functionality for it.
                    if (selectedFilters.Count == 1 && _eventLogAdapters.SingleOrDefault(
                            e => e.LogType == selectedFilters.First().EventType) is ILogTicketPrintable ticketPrintable
                    ) // If single filter selected, check for special log/print handlers
                    {
                        var logsToBePrinted = GetItemsToPrint(FilteredLogCollection.ToList(), dataType);
                        var transactionIDs = logsToBePrinted.Select(e => e.Description.TransactionId).ToList();
                        tickets = ticketPrintable.GenerateLogTickets(transactionIDs);
                    }
                    else
                    {
                        tickets = GenerateLogTickets(dataType);
                    }

                    break;

                case OperatorMenuPrintData.SelectedItem:
                    if (_selectedItem != null)
                    {
                        var eventBaseTypeFromEvenType =
                            EventFilterCollection.SingleOrDefault(e => e.EventType == _selectedItem.Description.Type)
                                ?.EventType;
                        var logExtractor = _eventLogAdapters.SingleOrDefault(
                            e => e.LogType == eventBaseTypeFromEvenType);
                        // If selected item has a special print functionality
                        if (logExtractor is ILogTicketPrintable printable)
                        {
                            tickets = TicketToList(printable.GetSelectedTicket(_selectedItem.Description));
                        }
                        else
                        {
                            tickets = TicketToList(GenerateSelectedEventTicket());
                        }
                    }

                    break;
            }

            return tickets;
        }

        private void ClearEventLogCollection()
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    EventLogCollection.CollectionChanged -= EventLogCollection_OnCollectionChanged;
                    EventLogCollection.Clear();
                });
        }

        private void HandleTimeZoneChangedEvent(TimeZoneUpdatedEvent evt)
        {
            SetupTiltLogAppendedTilt(false);
            ReloadEventHistory();
            SetupTiltLogAppendedTilt(true);
        }

        private void ShowAdditionalInfo(object obj)
        {
            // Show a popup window with additional info
            var dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            var eventBaseTypeFromEventType =
                EventFilterCollection.FirstOrDefault(e => e.EventType == SelectedItem.Description.Type)?.EventType;
            var logAdapter = _eventLogAdapters.SingleOrDefault(
                e => e.LogType == eventBaseTypeFromEventType);
            var viewModel = new LogDetailsViewModel(SelectedItem, logAdapter);

            var name = SelectedItem.Description.Name.Split(
                new[] { EventLogUtilities.EventDescriptionNameDelimiter },
                StringSplitOptions.None);

            dialogService.ShowInfoDialog<LogDetailView>(
                this,
                viewModel,
                $"{name[0]} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.LogDetails)}");
        }

        private void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_settingFilterSelections)
            {
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            UpdateMaxEntries();
            RaisePropertyChanged(nameof(FilteredLogCollection));
            UpdateEventCount();
            UpdatePrinterButtons();
        }

        public bool IsAllFiltersSelected
        {
            get => _isAllFiltersSelected;
            set
            {
                if (_isAllFiltersSelected != value)
                {
                    _isAllFiltersSelected = value;
                    RaisePropertyChanged(nameof(IsAllFiltersSelected));
                }
            }
        }

        private void AllFiltersSelected()
        {
            // Wait until all filter selections have changed and then update
            _settingFilterSelections = true;
            foreach (var filter in EventFilterCollection)
            {
                filter.IsSelected = IsAllFiltersSelected;
            }
            _settingFilterSelections = false;

            UpdateUI();
        }

        private void FilterSelected()
        {
            IsAllFiltersSelected = EventFilterCollection.All(filter => filter.IsSelected);
        }
    }
}
