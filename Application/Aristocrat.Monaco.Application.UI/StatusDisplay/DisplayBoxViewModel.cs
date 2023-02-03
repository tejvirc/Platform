namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using System;
    using System.Collections.ObjectModel;
    using CommunityToolkit.Mvvm.ComponentModel;

    [CLSCompliant(false)]
    public class DisplayBoxViewModel : ObservableObject
    {
        private ObservableCollection<string> _messages;

        public DisplayBoxViewModel()
        {
            _messages = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;

            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    OnPropertyChanged(nameof(Messages));
                }
            }
        }

        public void AddMessage(string message)
        {
            Messages.Insert(0, message);
        }

        public void RemoveMessage(string message)
        {
            Messages.Remove(message);
        }

        public void RemoveAll()
        {
            Messages.Clear();
        }
    }
}