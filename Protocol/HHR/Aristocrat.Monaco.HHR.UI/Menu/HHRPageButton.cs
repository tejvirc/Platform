namespace Aristocrat.Monaco.Hhr.UI.Menu
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using CommunityToolkit.Mvvm.Input;

    public class HhrPageCommand : RelayCommand<object>, INotifyPropertyChanged
    {
        private bool _visibility;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Visibility -  to set/reset the Visibility of HHR button 
        /// </summary>
        public bool Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility)
                {
                    return;
                }

                _visibility = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }

        public Command Command { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        public HhrPageCommand(Action<object> executeMethod, bool visibility, Command command) : base(executeMethod)
        {
            _visibility = visibility;
            Command = command;
        }

        /// <summary>
        ///     Invoked when the property is changed
        /// </summary>
        /// <param name="propertyName"> Property which changed</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
