namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;

    public class HhrPageCommand : ObservableObject, IRelayCommand<object>
    {
        private readonly Action<object> _execute;

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
        public HhrPageCommand(Action<object> execute, bool visibility, Command command)
        {
            _execute = execute;
            _visibility = visibility;
            Command = command;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute?.Invoke(parameter);

        void IRelayCommand<object>.Execute(object parameter) => ((ICommand)this).Execute(parameter);

        bool IRelayCommand<object>.CanExecute(object parameter) => ((ICommand)this).CanExecute(parameter);

        void IRelayCommand.NotifyCanExecuteChanged() => OnCanExecuteChanged();

        private void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
