namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using System.Collections.ObjectModel;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    ///     Game settings.
    /// </summary>
    internal class GameSettings : BaseObservableObject
    {
        private string _themeId;
        private string _paytableId;

        /// <summary>
        ///     Gets or sets the theme identifier.
        /// </summary>
        public string ThemeId
        {
            get => _themeId;

            set => SetProperty(ref _themeId, value);
        }

        /// <summary>
        ///     Gets or sets the pay table identifier.
        /// </summary>
        public string PaytableId
        {
            get => _paytableId;

            set => SetProperty(ref _paytableId, value);
        }

        /// <summary>
        ///     Gets or sets a list of denominations
        /// </summary>
        public ObservableCollection<DenominationSettings> Denominations { get; set; }
    }
}
