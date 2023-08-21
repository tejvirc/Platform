namespace Aristocrat.Monaco.Application.Contracts.Localization
{
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Notification that operator culture(s) have been added.
    /// </summary>
    public class OperatorCultureAdded : SupportedCulturesChangedEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperatorCultureAdded"/> class.
        /// </summary>
        /// <param name="cultures"></param>
        public OperatorCultureAdded(IEnumerable<CultureInfo> cultures)
            : base(cultures)
        {
        }
    }
}
