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

    }
}
