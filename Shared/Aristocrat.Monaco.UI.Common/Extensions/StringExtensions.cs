namespace Aristocrat.Monaco.UI.Common.Extensions
{
    /// <summary>
    ///     Extensions for string
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        ///     Returns null if the string is Empty, otherwise just returns the unchanged string
        /// </summary>
        /// <param name="text">text to check</param>
        /// <returns>Null if the string is Empty, otherwise just returns the unchanged string</returns>
        public static string NullIfEmpty(this string text)
        {
            return text == string.Empty ? null : text;
        }

        /// <summary>
        ///     returns true if string is null or empty
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
    }
}