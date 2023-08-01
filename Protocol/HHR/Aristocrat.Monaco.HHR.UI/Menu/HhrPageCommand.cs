namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public class HhrPageCommand : ObservableObject, IRelayCommand<object>
    {
        private readonly RelayCommand<object> _relayCommand;

        private bool _visibility;

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Visibility -  to set/reset the Visibility of HHR button
        /// </summary>
        public bool Visibility
        {
            get => _visibility;

            set => SetProperty(ref _visibility, value);
        }

        public Command Command { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public HhrPageCommand(Action<object> action, bool visibility, Command command)
        {
            _relayCommand = new RelayCommand<object>(action);
            _relayCommand.CanExecuteChanged += (sender, args) => CanExecuteChanged?.Invoke(sender, args);
            _visibility = visibility;
            Command = command;
        }

        public bool CanExecute(object parameter) => _relayCommand.CanExecute(parameter);

        public void Execute(object parameter) => _relayCommand.Execute(parameter);

        public void NotifyCanExecuteChanged() => _relayCommand.NotifyCanExecuteChanged();
    }
}