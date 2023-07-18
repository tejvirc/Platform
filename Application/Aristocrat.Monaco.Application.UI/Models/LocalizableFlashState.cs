namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Hardware.Contracts.TowerLight;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    [CLSCompliant(false)]
    public class LocalizableFlashState : BaseObservableObject
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
