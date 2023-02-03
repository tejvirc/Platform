﻿namespace Aristocrat.Monaco.Application.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using CommunityToolkit.Mvvm.Input;
    using ConfigWizard;
    using Contracts;
    using Contracts.OperatorMenu;
    using Contracts.Protocol;
    using Kernel;
    using Monaco.Common;

    /// <summary>
    ///     The view model for time and time zone configuration
    /// </summary>
    [CLSCompliant(false)]
    public class TimeConfigPageViewModel : ConfigWizardViewModelBase
    {
        private readonly ITime _time;
        private bool _timeZoneChanged;

        private int _hour;
        private int _minute;
        private int _second;
        private int _previousDay;
        private int _previousMonth;
        private int _previousYear;
        private int _previousHour;
        private int _previousMinute;
        private int _previousSecond;

        private DateTime _pickerDate;

        private string _timeZoneId;
        private string _timeZoneOffset = string.Empty;

        public TimeConfigPageViewModel(bool isWizardPage) : base(isWizardPage)
        {
            _time = ServiceManager.GetInstance().GetService<ITime>();

            TimeZones = TimeZoneInfo.GetSystemTimeZones();

            InputStatusText = string.Empty;

            Hours = Enumerable.Range(0, 24).ToList();
            Minutes = Enumerable.Range(0, 60).ToList();
            Seconds = Enumerable.Range(0, 60).ToList();

            _timeZoneChanged = false;
            ApplyCommand = new RelayCommand<object>(Apply, _ => CanApply);
        }

        public RelayCommand<object> ApplyCommand { get; }

        public ReadOnlyCollection<TimeZoneInfo> TimeZones { get; }

        public bool ApplyButtonIsVisible => WizardNavigator == null;

        public bool OffsetIsVisible => WizardNavigator == null &&
                                       ServiceManager.GetInstance().GetService<IMultiProtocolConfigurationProvider>()
                                           .MultiProtocolConfiguration
                                           .Any(x => x.Protocol == CommsProtocol.G2S);
        public bool CanApply => InputEnabled && TimeZoneId != _time.TimeZoneInformation.Id || _previousDay != _pickerDate.Day ||
                                _previousMonth != _pickerDate.Month || _previousYear != _pickerDate.Year ||
                                Hour != _previousHour || Minute != _previousMinute || Second != _previousSecond;

        private void Apply(object obj)
        {
            SaveChanges();
        }

        public string TimeZoneId
        {
            get => _timeZoneId;

            set
            {
                _timeZoneId = value;
                if (!string.IsNullOrEmpty(value))
                {
                    OnTimeZoneChanged(value);
                    OnPropertyChanged(nameof(TimeZoneId));
                    ApplyCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string TimeZoneOffset
        {
            get => _timeZoneOffset;

            set
            {
                if (_timeZoneOffset != value)
                {
                    _timeZoneOffset = value;
                    OnPropertyChanged(nameof(TimeZoneOffset));
                }
            }
        }

        public int Hour
        {
            get => _hour;

            set
            {
                if (_hour != value)
                {
                    _hour = value;
                    OnPropertyChanged(nameof(Hour));
                    ApplyCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public int Minute
        {
            get => _minute;

            set
            {
                if (_minute != value)
                {
                    _minute = value;
                    OnPropertyChanged(nameof(Minute));
                    ApplyCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public int Second
        {
            get => _second;

            set
            {
                if (_second != value)
                {
                    _second = value;
                    OnPropertyChanged(nameof(Second));
                    ApplyCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public DateTime PickerDate
        {
            get => _pickerDate;

            set
            {
                if (_pickerDate != value)
                {
                    _pickerDate = value;
                    OnPropertyChanged(nameof(PickerDate));
                    ApplyCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public List<int> Hours { get; }

        public List<int> Minutes { get; }

        public List<int> Seconds { get; }

        protected override void SaveChanges()
        {
            if (_timeZoneChanged)
                EventBus.Publish(new TimeZoneUpdatedEvent());  // VLT-4526

            if (string.IsNullOrEmpty(TimeZoneId))
            {
                TimeZoneId = _time.TimeZoneInformation?.Id;
            }

            PropertiesManager.SetProperty(ApplicationConstants.TimeZoneKey, TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId));

            // Time display to the user is in the local time of the time zone that is selected. We need to convert this time
            // to the UTC time to update system time.  Internally we stored and referenced only UTC Time.  For display purposes
            // we display in local time for the user to see and select.
            var dtUserSelected = new DateTime(
                _pickerDate.Year,
                _pickerDate.Month,
                _pickerDate.Day,
                _hour,
                _minute,
                _second,
                DateTimeKind.Unspecified);
            var dtUtcSelected = TimeZoneInfo.ConvertTimeToUtc(dtUserSelected, _time.TimeZoneInformation);
            _time.Update(dtUtcSelected);

            EventBus.Publish(new OperatorMenuSettingsChangedEvent());

            _previousHour = Hour;
            _previousMinute = Minute;
            _previousSecond = Second;
            _previousDay = _pickerDate.Day;
            _previousMonth = _pickerDate.Month;
            _previousYear = _pickerDate.Year;
            ApplyCommand.NotifyCanExecuteChanged();

            _timeZoneChanged = false;
        }

        // VLT-9804 : save moving of the page in config sequence (we still allow Apply button in AuditMenu)
        protected override void OnUnloaded()
        {
            if (WizardNavigator != null)
            {
                base.OnUnloaded();
            }
        }

        protected override void Loaded()
        {
            EventBus.Subscribe<TimeZoneOffsetUpdatedEvent>(this, OnOffsetUpdated);
            TimeZoneId = _time.TimeZoneInformation?.Id;
            UpdateTimeZoneOffset();

            _previousHour = Hour;
            _previousMinute = Minute;
            _previousSecond = Second;
            _previousDay = _pickerDate.Day;
            _previousMonth = _pickerDate.Month;
            _previousYear = _pickerDate.Year;

            ApplyCommand.NotifyCanExecuteChanged();
        }

        protected override void OnInputEnabledChanged()
        {
            ApplyCommand.NotifyCanExecuteChanged();
        }

        private void OnOffsetUpdated(TimeZoneOffsetUpdatedEvent evt)
        {
            UpdateTimeZoneOffset();
        }

        private void OnTimeZoneChanged(string id)
        {
            OnTimeZoneChanged(TimeZoneInfo.FindSystemTimeZoneById(id));
            _timeZoneChanged = true;

        }

        private void OnTimeZoneChanged(TimeZoneInfo timeZone)
        {
            if (timeZone == null)
            {
                return;
            }

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            PickerDate = now;
            Hour = now.Hour;
            Minute = now.Minute;
            Second = now.Second;

            Logger.Debug($"Time Zone Selected: {timeZone.Id} Local Time Zone: {TimeZoneInfo.Local.Id}");

            UpdateTimeZoneOffset();
            ApplyCommand.NotifyCanExecuteChanged();
        }

        private void UpdateTimeZoneOffset()
        {
            TimeZoneOffset = PropertiesManager.GetValue(ApplicationConstants.TimeZoneOffsetKey, TimeSpan.Zero)
                .GetFormattedOffset();
        }
    }
}
