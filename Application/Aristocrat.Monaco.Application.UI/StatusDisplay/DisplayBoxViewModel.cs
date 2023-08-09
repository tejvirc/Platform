namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using System;
    using System.Collections.ObjectModel;
    using Kernel;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class DisplayBoxViewModel : BaseViewModel
    {
        private ObservableCollection<string> _messages;
        private ObservableCollection<DisplayableMessage> _displayableMessages;

        public DisplayBoxViewModel()
        {
            _messages = new ObservableCollection<string>();
            _displayableMessages = new ObservableCollection<DisplayableMessage>();
        }

        public ObservableCollection<string> Messages
        {
            get => _messages;

            set
            {
                if (_messages != value)
                {
                    _messages = value;
                    RaisePropertyChanged(nameof(Messages));
                }
            }
        }

        public ObservableCollection<DisplayableMessage> DisplayableMessages
        {
            get => _displayableMessages;

            set
            {
                if (_displayableMessages != value)
                {
                    _displayableMessages = value;
                    RaisePropertyChanged(nameof(DisplayableMessages));  
                }
            }
        }

        public void AddMessage(string message)
        {
            Messages.Insert(0, message);
        }

        public void AddDisplayableMessage(DisplayableMessage message)
        {
            DisplayableMessages.Insert(0, message); 
        }

        public void RemoveMessage(DisplayableMessage message)
        {
            Messages.Remove(message.Message);
            DisplayableMessages.Remove(message);
        }

        public void RemoveAll()
        {
            Messages.Clear();
        }

        public void UpdateMessages()
        {
            RemoveAll();

            foreach (var displayableMessage in  _displayableMessages)
            {
                Messages.Add(displayableMessage.Message);
            }
        }
    }
}