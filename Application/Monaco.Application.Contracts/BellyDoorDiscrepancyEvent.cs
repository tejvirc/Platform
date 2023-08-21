namespace Aristocrat.Monaco.Application.Contracts
{
    using Localization;
    using Kernel;
    using Aristocrat.Monaco.Localization.Properties;

    /// <summary>
    /// An event when belly door discrepancy occurs
    /// </summary>
    public class BellyDoorDiscrepancyEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.BellyDoorDiscrepancy);
        }
    }
}
