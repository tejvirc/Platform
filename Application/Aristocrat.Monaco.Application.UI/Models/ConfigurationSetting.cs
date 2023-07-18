namespace Aristocrat.Monaco.Application.UI.Models
{
    using System;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;

    /// <summary>
    ///     Configuration setting.
    /// </summary>
    [CLSCompliant(false)]
    public class ConfigurationSetting : BaseObservableObject
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
