namespace Aristocrat.Monaco.Asp.Extensions
{
    using System.Text.RegularExpressions;

    public static class StringExtensions
    {
        /// <summary>
        ///     Split the string according to CamelCase convention, for instance: "ThisString" will return "This String".
        /// </summary>
        public static string SplitCamelCase(this string input) => Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
    }
}