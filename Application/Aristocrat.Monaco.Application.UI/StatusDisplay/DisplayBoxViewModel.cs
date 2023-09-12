namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using System;
    using System.Collections.ObjectModel;
    using Application.Contracts.Localization;
    using Monaco.Localization.Properties;
    using Kernel;
    using CommunityToolkit.Mvvm.ComponentModel;

    [CLSCompliant(false)]
    public class DisplayBoxViewModel : ObservableObject
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
                    OnPropertyChanged(nameof(Messages));
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

        public void AddDisplayableMessage(DisplayableMessage message)
        {
            DisplayableMessages.Insert(0, message);
            Messages.Insert(0, ResolveDisplayableMessageText(message));
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
                Messages.Add(ResolveDisplayableMessageText(displayableMessage));
            }
        }

        public string ResolveDisplayableMessageText(DisplayableMessage displayableMessage)
        {
            if (displayableMessage.ResourceKeyOnly)
            {
                try
                {
                    return Localizer.For(CultureFor.Operator).GetString(displayableMessage.Message);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Localization providers may not be ready yet at this point
                    return Resources.ResourceManager.GetString(displayableMessage.Message);
                }
            }
            else
            {
                return displayableMessage.Message;
            }
        }
    }
}
