namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.Localization;

    /// <summary>
    ///     Volume Option that relates a <see cref="byte"/> to a localized display string
    /// </summary>
    [CLSCompliant(false)]
    public class VolumeOption : ObservableObject
    {
        public VolumeOption(byte level,string description)
        {
            Level = level;
            Description = description;
        }

        public byte Level { get; }
        public string Description { get; }

        public string DisplayString => Localizer.For(CultureFor.Operator).GetString(Description);

        public void UpdateDisplay()
        {
            OnPropertyChanged(nameof(DisplayString));
        }
    }
}
