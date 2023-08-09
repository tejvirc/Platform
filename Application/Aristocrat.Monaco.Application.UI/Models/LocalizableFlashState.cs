namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Hardware.Contracts.TowerLight;

    [CLSCompliant(false)]
    public class LocalizableFlashState : ObservableObject
    {
        public LocalizableFlashState(FlashState state)
        {
            FlashState = state;
        }

        public FlashState FlashState { get; }

        public string FlashStateString => Localizer.For(CultureFor.Operator).GetString(FlashState.ToString());

        public void UpdateString()
        {
            OnPropertyChanged(nameof(FlashStateString));
        }
    }
}
