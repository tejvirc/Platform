namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    /// </summary>
    public static class EventLogUtilities
    {
        /// <summary>
        /// </summary>
        public static readonly string EventDescriptionNameDelimiter =
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EventDescriptionNameDelimiter);

        /// <summary>
        /// </summary>
        public static readonly string EventStringDelimiter =
            Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EventStringDelimiter);
    }
}