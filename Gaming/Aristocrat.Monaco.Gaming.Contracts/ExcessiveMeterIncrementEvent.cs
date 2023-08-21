namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Application.Contracts.Localization;
    using Kernel;
    using Localization.Properties;

    /// <summary>
    ///     An event when a excessive meter increment lockup happened
    /// </summary>
    public class ExcessiveMeterIncrementEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ExcessiveMeterIncrementError);
        }
    }
}