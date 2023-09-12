namespace Aristocrat.Monaco.G2S.UI.Models
{
    using System;
    using Aristocrat.G2S.Client;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using CommunityToolkit.Mvvm.ComponentModel;
    using Localization.Properties;

    /// <summary>
    ///     Defines a G2S Host that can be required to play.
    /// </summary>
    public class Host : ObservableObject, IHost
    {
        private bool _requiredForPlay;
        private bool _isProgressiveHost;
        private bool _registered;

        /// <inheritdoc />
        public int Index { get; set; }

        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public Uri Address { get; set; }

        /// <inheritdoc />
        public bool Registered
        {
            get => _registered;
            set
            {
                if (SetProperty(ref _registered, value))
                {
                    OnPropertyChanged(nameof(RegisteredDisplayText));
                }
            }
        }

        public string RegisteredDisplayText => GetBooleanDisplayText(Registered);

        /// <inheritdoc />
        public bool RequiredForPlay
        {
            get => _requiredForPlay;
            set
            {
                if (SetProperty(ref _requiredForPlay, value))
                {
                    OnPropertyChanged(nameof(RequiredForPlayDisplayText));
                }
            }
        }

        public string RequiredForPlayDisplayText => GetBooleanDisplayText(RequiredForPlay);

        /// <inheritdoc />
        public bool IsProgressiveHost
        {
            get => _isProgressiveHost;
            set
            {
                if (SetProperty(ref _isProgressiveHost, value))
                {
                    OnPropertyChanged(nameof(IsProgressiveHostDisplayText));
                }
            }
        }

        public string IsProgressiveHostDisplayText => GetBooleanDisplayText(IsProgressiveHost);

        /// <inheritdoc />
        public TimeSpan ProgressiveHostOfflineTimerInterval { get; set; }

        private static string GetBooleanDisplayText(bool value)
        {
            return value
                ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TrueText)
                : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.FalseText);
        }
    }
}