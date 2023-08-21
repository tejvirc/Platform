namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Notification that operator culture(s) have been added.
    /// </summary>
    public class OperatorCultureRemoved : SupportedCulturesChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorCultureRemoved"/> class.
        /// </summary>
        /// <param name="cultures"></param>
        public OperatorCultureRemoved(IEnumerable<CultureInfo> cultures)
            : base(cultures)
        {
        }
    }
}
