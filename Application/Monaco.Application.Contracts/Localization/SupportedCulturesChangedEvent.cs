namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    ///     Notification that culture(s) have been removed.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class SupportedCulturesChangedEvent : LocalizationEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SupportedCulturesChangedEvent"/> class.
        /// </summary>
        /// <param name="cultures"></param>
        protected SupportedCulturesChangedEvent(IEnumerable<CultureInfo> cultures)
        {
            Cultures = cultures.ToArray();
        }

        /// <summary>
        ///     Gets the cultures that were added.
        /// </summary>
        public IEnumerable<CultureInfo> Cultures { get; }
    }
}
