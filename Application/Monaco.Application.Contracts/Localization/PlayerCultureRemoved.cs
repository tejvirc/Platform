namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Notification that the player culture was added.
    /// </summary>
    public class PlayerCultureRemoved : SupportedCulturesChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerCultureRemoved"/> class.
        /// </summary>
        /// <param name="cultures"></param>
        public PlayerCultureRemoved(IEnumerable<CultureInfo> cultures)
            : base(cultures)
        {
        }
    }
}
