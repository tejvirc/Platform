namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Notification that the player culture was added.
    /// </summary>
    public class PlayerCultureAdded : SupportedCulturesChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureAdded"/> class.
        /// </summary>
        /// <param name="cultures"></param>
        public PlayerCultureAdded(IEnumerable<CultureInfo> cultures)
            : base(cultures)
        {
        }
    }
}
