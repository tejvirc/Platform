﻿namespace Aristocrat.Monaco.Application.UI.StatusDisplay
{
    using System;
    using System.Collections.ObjectModel;
    using MVVM.ViewModel;

    [CLSCompliant(false)]
    public class DisplayBoxViewModel : BaseViewModel
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
                    RaisePropertyChanged(nameof(Messages));
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