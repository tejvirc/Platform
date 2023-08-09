namespace Aristocrat.Monaco.Application.UI.Settings
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using Contracts.Extensions;
    using Hardware.Contracts.NoteAcceptor;
    using Newtonsoft.Json;

    /// <summary>
    ///     Contains the settings for disabled notes.
    /// </summary>
    internal class DisabledNotesSetting : ObservableObject
    {
        private int _denom;
        private string _isoCode;

        /// <summary>
        ///     Gets or sets the denom.
        /// </summary>
        public int Denom
        {
            get => _denom;

            set
            {
                SetProperty(ref _denom, value);
                OnPropertyChanged(nameof(DenomDisplay));
            }
        }

        /// <summary>
        ///     Gets the host 0 denom to display.
        /// </summary>
        [JsonIgnore]
        public string DenomDisplay => ((decimal)_denom).FormattedCurrencyString();

        /// <summary>
        ///     Gets or sets the ISO code.
        /// </summary>
        public string IsoCode
        {
            get => _isoCode;

            set => SetProperty(ref _isoCode, value);
        }

        /// <summary>
        ///     Performs conversion from <see cref="DisabledNotes"/> to <see cref="DisabledNotesSetting"/>.
        /// </summary>
        /// <param name="notes">The <see cref="DisabledNotes"/> object.</param>
        public static explicit operator DisabledNotesSetting(DisabledNotes notes) => new DisabledNotesSetting
        {
            Denom = notes.Denom,
            IsoCode = notes.IsoCode
        };

        /// <summary>
        ///     Performs conversion from <see cref="DisabledNotesSetting"/> to <see cref="DisabledNotes"/>.
        /// </summary>
        /// <param name="setting">The <see cref="DisabledNotesSetting"/> setting.</param>
        public static explicit operator DisabledNotes(DisabledNotesSetting setting) => new DisabledNotes
        {
            Denom = setting.Denom,
            IsoCode = setting.IsoCode
        };
    }
}
