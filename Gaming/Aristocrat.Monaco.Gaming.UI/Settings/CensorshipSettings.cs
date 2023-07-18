namespace Aristocrat.Monaco.Gaming.UI.Settings
{
    using CommunityToolkit.Mvvm.ComponentModel;

    /// <summary>
    ///     Censorship settings.
    /// </summary>
    internal class CensorshipSettings : ObservableObject
    {
        private bool _censorshipEnforced;
        private bool _censorshipEditable;
        private bool _censorViolentContent;
        private bool _censorDrugUseContent;
        private bool _censorSexualContent;
        private bool _censorOffensiveContent;

        /// <summary>
        ///     Gets or sets the a value that indicates whether censorship is enforced.
        /// </summary>
        public bool CensorshipEnforced
        {
            get => _censorshipEnforced;

            set => SetProperty(ref _censorshipEnforced, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether censorship is editable.
        /// </summary>
        public bool CensorshipEditable
        {
            get => _censorshipEditable;

            set => SetProperty(ref _censorshipEditable, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether violent content is censored.
        /// </summary>
        public bool CensorViolentContent
        {
            get => _censorViolentContent;

            set => SetProperty(ref _censorViolentContent, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether drug use content is censored.
        /// </summary>
        public bool CensorDrugUseContent
        {
            get => _censorDrugUseContent;

            set => SetProperty(ref _censorDrugUseContent, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether sexual content is censored.
        /// </summary>
        public bool CensorSexualContent
        {
            get => _censorSexualContent;

            set => SetProperty(ref _censorSexualContent, value);
        }

        /// <summary>
        ///     Gets or sets the a value that indicates whether offensive content is censored.
        /// </summary>
        public bool CensorOffensiveContent
        {
            get => _censorOffensiveContent;

            set => SetProperty(ref _censorOffensiveContent, value);
        }
    }
}
