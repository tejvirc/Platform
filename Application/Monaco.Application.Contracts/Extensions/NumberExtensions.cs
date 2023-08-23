namespace Aristocrat.Monaco.Application.Contracts.Extensions
{
    using System.Globalization;
    using Localization;

    /// <summary>
    ///     NumberExtensions
    /// </summary>
    public static class NumberExtensions
    {
        private static CultureInfo DisplayCulture => Localizer.For(CultureFor.Operator).CurrentCulture ?? CultureInfo.CurrentCulture;

        /// <summary>
        /// Converts a double to a formatted number
        /// </summary>
        /// <remarks>Will format with separators based on culture</remarks>
        /// <param name="value">The value you wish to convert</param>
        /// <param name="cultureInfo">Optional param to override culture</param>
        /// <returns>A string containing all the separators for that culture</returns>
        public static string ToFormattedString(this double value, CultureInfo cultureInfo = null)
        {
            if (cultureInfo == null)
            {
                cultureInfo = DisplayCulture;
            }
            return value.ToString("F", cultureInfo);
        }

        /// <summary>
        /// Converts decimal to formatted percent
        /// </summary>
        /// <param name="value">decimal value to be passed in</param>
        /// <param name="cultureInfo">Optional param to override culture</param>
        /// <param name="format">The decimal formatting to be used, defaults to P3</param>
        /// <returns>A localized Percentage String</returns>
        public static string ToPercentageFormattedString(this decimal value, string format = "P3", CultureInfo cultureInfo = null)
        {
            if (cultureInfo == null)
            {
                cultureInfo = DisplayCulture;
            }
            return value.ToString(format, cultureInfo);
        }
    }
}
