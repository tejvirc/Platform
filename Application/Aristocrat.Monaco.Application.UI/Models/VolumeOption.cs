﻿namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Contracts.Localization;
    using Hardware.Contracts.Audio;
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     Volume Option that relates a <see cref="VolumeLevel"/> to a localized display string
    /// </summary>
    [CLSCompliant(false)]
    public class VolumeOption : ObservableObject
    {
        public VolumeOption(VolumeLevel level)
        {
            Level = level;
        }

        public VolumeLevel Level { get; }

        public string DisplayString => Localizer.For(CultureFor.Operator).GetString(Level.ToString());

        public void UpdateDisplay()
        {
            RaisePropertyChanged(nameof(DisplayString));
        }
    }
}
