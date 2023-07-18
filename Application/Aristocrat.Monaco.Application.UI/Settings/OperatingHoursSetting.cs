namespace Aristocrat.Monaco.Application.UI.Settings
{
    using System;
    using Contracts.Operations;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     Contains the settings for operating hours for a particular day.
    /// </summary>
    internal class OperatingHoursSetting : ObservableObject
    {
        private DayOfWeek _day;
        private int _time;
        private bool _enabled;

        /// <summary>
        ///     Gets or sets the day of the week.
        /// </summary>
        public DayOfWeek Day
        {
            get => _day;

            set => SetProperty(ref _day, value);
        }

        /// <summary>
        ///     Gets or sets the offset from midnight, in milliseconds.
        /// </summary>
        public int Time
        {
            get => _time;

            set => SetProperty(ref _time, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the operating hours state should be enabled.
        /// </summary>
        public bool Enabled
        {
            get => _enabled;

            set => SetProperty(ref _enabled, value);
        }

        public void RefreshAllSettings()
        {
            RaisePropertyChanged(nameof(Enabled));
            RaisePropertyChanged(nameof(Time));
            RaisePropertyChanged(nameof(Day));
        }

        /// <summary>
        ///     Performs conversion from <see cref="OperatingHours"/> to <see cref="OperatingHoursSetting"/>.
        /// </summary>
        /// <param name="hours">The <see cref="OperatingHours"/> object.</param>
        public static explicit operator OperatingHoursSetting(OperatingHours hours) => new OperatingHoursSetting
        {
            Day = hours.Day,
            Time = hours.Time,
            Enabled = hours.Enabled
        };

        /// <summary>
        ///     Performs conversion from <see cref="OperatingHoursSetting"/> to <see cref="OperatingHours"/>.
        /// </summary>
        /// <param name="setting">The <see cref="OperatingHoursSetting"/> setting.</param>
        public static explicit operator OperatingHours(OperatingHoursSetting setting) => new OperatingHours
        {
            Day = setting.Day,
            Time = setting.Time,
            Enabled = setting.Enabled
        };
    }
}
