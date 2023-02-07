namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using ConfigWizard;
    using Contracts;
    using Contracts.ConfigWizard;
    using Contracts.Localization;
    using Hardware.Contracts.Door;
    using Kernel;
    using Monaco.Localization.Properties;

    [CLSCompliant(false)]
    public class DoorViewModel : ObservableObject
    {
        private readonly object _context = new object();
        private readonly IInspectionService _reporter;

        private bool _closed;
        private DateTime? _lastOpened;
        private string _name;

        public DoorViewModel(IInspectionService reporter, int id, bool ignored = false)
        {
            _reporter = reporter;
            Id = id;
            Ignored = ignored;
        }

        public string Name
        {
            get => _name;

            private set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public int Id { get; }

        public bool Ignored { get; }

        public bool Closed
        {
            get => _closed;

            private set
            {
                if (_closed == value)
                {
                    return;
                }

                _closed = value;

                OnPropertyChanged(nameof(Action));
            }
        }

        public string Action => Closed ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosedText) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenText);

        public DateTime? LastOpened
        {
            get => _lastOpened;

            private set
            {
                if (_lastOpened == value)
                {
                    return;
                }

                _lastOpened = value;
                OnPropertyChanged(nameof(LastOpened));
            }
        }

        public void OnLoaded()
        {
            Update();

            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            eventBus.Subscribe<OpenEvent>(_context, HandleEvent);
            eventBus.Subscribe<ClosedEvent>(_context, HandleEvent);
        }

        public void OnUnloaded()
        {
            var eventBus = ServiceManager.GetInstance().GetService<IEventBus>();
            eventBus.UnsubscribeAll(_context);
        }

        private void HandleEvent(OpenEvent evt)
        {
            if (evt.LogicalId != Id)
            {
                return;
            }

            Update();

            _reporter?.SetTestName($"Open {Name}");
        }

        private void HandleEvent(ClosedEvent evt)
        {
            if (evt.LogicalId != Id)
            {
                return;
            }

            Update();

            _reporter?.SetTestName($"Closed {Name}");
        }

        private void Update()
        {
            var door = ServiceManager.GetInstance().GetService<IDoorService>();
            var monitor = ServiceManager.GetInstance().GetService<IDoorMonitor>();

            Name = monitor.GetLocalizedDoorName(Id);
            Closed = Ignored || door.GetDoorClosed(Id);
            LastOpened = Ignored ? DateTime.MinValue : LocalizeDate(door.GetDoorLastOpened(Id));
        }

        private static DateTime? LocalizeDate(DateTime lastOpened)
        {
            if (lastOpened == DateTime.MinValue)
            {
                return null;
            }

            return TimeZoneInfo.ConvertTime(lastOpened, TimeZoneInfo.Local);
        }
    }
}