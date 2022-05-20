namespace Aristocrat.Monaco.Application.Localization
{
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json.Linq;

    /// <summary>
    ///     Used to persist the localization state.
    /// </summary>
    public class LocalizationState
    {
        /// <summary>
        ///     Gets or sets the default culture.
        /// </summary>
        public CultureInfo DefaultCulture { get; set; }

        /// <summary>
        ///     Gets or sets the current culture.
        /// </summary>
        public CultureInfo CurrentCulture { get; set; }

        /// <summary>
        ///     Gets or sets the providers.
        /// </summary>
        public Dictionary<string, JObject> Providers { get; set; }
    }
}
