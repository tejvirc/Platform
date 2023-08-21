namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.TiltLogger;
    using Kernel;

    public partial class EventLogFilterViewModel
    {
        public class EventFilterInfo : INotifyPropertyChanged
        {
            private bool _isSelected;
            private string _displayText;

            public string EventType { get; set; }

            public string DisplayText
            {
                get => _displayText;
                set
                {
                    if (value == _displayText)
                    {
                        return;
                    }

                    _displayText = value;
                    OnPropertyChanged(nameof(DisplayText));
                }
            }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (value == _isSelected)
                    {
                        return;
                    }

                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<EventFilterInfo> CreateFilters()
        {
            var logFilters = new ObservableCollection<EventFilterInfo>();
            var logsToDisplay = (string[])_propertiesManager.GetProperty(
                ApplicationConstants.LogTypesAllowedForDisplayKey,
                null);
            if (logsToDisplay != null)
            {
                foreach (var logType in logsToDisplay)
                {
                    if (logType.Equals("GPU"))
                    {
                        // Temporarily hide GPU Filter
                        continue;
                    }
                    var displayText = Localizer.For(CultureFor.Operator).GetString(logType);
                    logFilters.Add(CreateEventLogFilter(logType, displayText, true));
                }
            }

            return logFilters;
        }

        private EventFilterInfo CreateEventLogFilter(
            string eventLogType,
            string displayText = "",
            bool isSelected = false)
        {
            var eventFilter = new EventFilterInfo
            {
                EventType = eventLogType,
                IsSelected = isSelected,
                DisplayText = string.IsNullOrEmpty(displayText) ? eventLogType : displayText
            };
            eventFilter.PropertyChanged += HandlePropertyChanged;
            return eventFilter;
        }

        private void ClearPropertyChangedDelegate()
        {
            foreach (var filter in EventFilterCollection)
            {
                filter.PropertyChanged -= HandlePropertyChanged;
            }
        }

        private static IReadOnlyCollection<IEventLogAdapter> GetLogAdapters()
        {
            return ServiceManager.GetInstance().GetService<ILogAdaptersService>().GetLogAdapters().ToList();
        }
    }
}