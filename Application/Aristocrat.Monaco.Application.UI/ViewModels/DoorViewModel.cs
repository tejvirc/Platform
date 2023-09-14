namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
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
        private readonly string _testMessage;

        private bool _closed;
        private DateTime? _lastOpened;
        private string _name;

        public DoorViewModel(IInspectionService reporter, int id, bool ignored, bool isTestRequired = false)
        {
            _reporter = reporter;
            Id = id;
            Ignored = ignored;
            IsTestRequired = isTestRequired;

            if (IsTestRequired)
            {
                _testMessage = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DoorRequiresTest);
            }
        }

        public string Name
        {
            get => _name;
            private set => SetProperty(ref _name, value, nameof(Name));
        }

        public int Id { get; }

        public bool Ignored { get; }

        public bool IsTestRequired { get; }

        public bool IsTestPassed => !IsTestRequired || Closed && LastOpened != null && LastOpened > DateTime.MinValue;

        public string Message => IsTestPassed ? string.Empty : _testMessage;

        public bool Closed
        {
            get => _closed;
            private set
            {
                if (SetProperty(ref _closed, value, nameof(Closed)))
                {
                    OnPropertyChanged(nameof(Action));
                    OnPropertyChanged(nameof(Message));
                    OnPropertyChanged(nameof(IsTestPassed));
                }

            }
        }

        public string Action => Closed ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ClosedText) : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OpenText);

        public DateTime? LastOpened
        {
            get => _lastOpened;
            private set
            {
                if (SetProperty(ref _lastOpened, value, nameof(LastOpened)))
                {
                    OnPropertyChanged(nameof(Message));
                    OnPropertyChanged(nameof(IsTestPassed));
                }
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
