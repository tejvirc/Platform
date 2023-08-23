namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.ConfigWizard;
    using Hardware.Contracts.KeySwitch;
    using Kernel;

    [CLSCompliant(false)]
    public class KeyViewModel : ObservableObject
    {
        private readonly IKeySwitch _button;
        private readonly object _context = new object();
        private readonly IEventBus _eventBus;
        private readonly IInspectionService _reporter;
        private KeySwitchAction _action;
        private string _name;
        private KeySwitchState _state;

        public KeyViewModel(int id, IInspectionService reporter)
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IKeySwitch>(),
                id,
                reporter
            )
        {
        }

        public KeyViewModel(IEventBus eventBus, IKeySwitch button, int id, IInspectionService reporter)
        {
            _eventBus = eventBus;
            _button = button;
            Id = id;
            _reporter = reporter;
        }

        public int Id { get; }

        public string Name
        {
            get => _name;

            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public KeySwitchAction Action
        {
            get => _action;

            set
            {
                if (_action == value)
                {
                    return;
                }

                _action = value;
                OnPropertyChanged(nameof(Action));
                OnPropertyChanged(nameof(IsPressed));
            }
        }

        public KeySwitchState State
        {
            get => _state;

            private set
            {
                if (_state == value)
                {
                    return;
                }

                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        public bool IsPressed => Action == KeySwitchAction.On;

        public void OnLoaded()
        {
            Update();

            _eventBus.Subscribe<OffEvent>(_context, HandleEvent);
            _eventBus.Subscribe<OnEvent>(_context, HandleEvent);
        }

        public void OnUnloaded()
        {
            _eventBus.UnsubscribeAll(_context);
        }

        public void UpdateProps()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Action));
        }

        private void HandleEvent(OffEvent evt)
        {
            if (evt.LogicalId != Id)
            {
                return;
            }

            Update();
            _reporter?.SetTestName($"{Name} {Action}");
        }

        private void HandleEvent(OnEvent evt)
        {
            if (evt.LogicalId != Id)
            {
                return;
            }

            Update();
            _reporter?.SetTestName($"{Name} {Action}");
        }

        private void Update()
        {
            Name = _button.GetLocalizedKeySwitchName(Id);
            Action = _button.GetKeySwitchAction(Id);
            State = _button.GetKeySwitchState(Id);
        }
    }
}
