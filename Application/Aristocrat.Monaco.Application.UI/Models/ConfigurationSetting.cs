namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using MVVM.Model;

    /// <summary>
    ///     Configuration setting.
    /// </summary>
    [CLSCompliant(false)]
    public class ConfigurationSetting : BaseNotify
    {
        private string _name;
        private object _settings;

        /// <summary>
        ///     Gets the group settings name.
        /// </summary>
        public string Name
        {
            get => _name;

            set => SetProperty(ref _name, value);
        }

        /// <summary>
        ///     Gets the settings object.
        /// </summary>
        public object Settings
        {
            get => _settings;

            set => SetProperty(ref _settings, value);
        }
    }
}
