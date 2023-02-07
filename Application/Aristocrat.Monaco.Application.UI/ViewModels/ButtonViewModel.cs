namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.ConfigWizard;
    using Hardware.Contracts.Button;
    using Kernel;

    [CLSCompliant(false)]
    public class ButtonViewModel : ObservableObject
    {
        private readonly IButtonService _button;
        private readonly object _context = new object();
        private readonly IEventBus _eventBus;
        private readonly IInspectionService _reporter;
        private ButtonAction _action;
        private string _name;
        private ButtonState _state;

        public ButtonViewModel(int id, IInspectionService reporter)
            : this(
                ServiceManager.GetInstance().GetService<IEventBus>(),
                ServiceManager.GetInstance().GetService<IButtonService>(),
                id,
                reporter
            )
        {
        }

        public ButtonViewModel(IEventBus eventBus, IButtonService button, int id, IInspectionService reporter)
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

        public ButtonAction Action
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

        public ButtonState State
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

        public bool IsPressed => Action == ButtonAction.Down;

        public void OnLoaded()
        {
            Update();

            _eventBus.Subscribe<UpEvent>(_context, HandleEvent);
            _eventBus.Subscribe<DownEvent>(_context, HandleEvent);
        }

        public void OnUnloaded()
        {
            _eventBus.UnsubscribeAll(_context);
        }

        private void HandleEvent(UpEvent evt)
        {
            if (evt.LogicalId != Id)
            {
                return;
            }

            Update();
            _reporter?.SetTestName($"{Name} {Action}");
        }

        private void HandleEvent(DownEvent evt)
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
            Name = _button.GetLocalizedButtonName(Id);
            Action = _button.GetButtonAction(Id);
            State = _button.GetButtonState(Id);
        }
    }
}