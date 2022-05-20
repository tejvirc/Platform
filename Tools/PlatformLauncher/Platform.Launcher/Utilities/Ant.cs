namespace Platform.Launcher.Utilities
{
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Represents a class which matches paths using ant-style path matching.
    /// </summary>
    public class Ant : IAnt
    {
        private readonly string _originalPattern;
        private readonly Regex _regex;

        /// <summary>
        ///     Initializes a new <see cref="Ant" />.
        /// </summary>
        /// <param name="pattern">Ant-style pattern.</param>
        public Ant(string pattern)
        {
            _originalPattern = pattern ?? string.Empty;
            _regex = new Regex(EscapeAndReplace(_originalPattern), RegexOptions.Singleline);
        }

        /// <inheritdoc />
        public bool IsMatch(string input)
        {
            input = input ?? string.Empty;
            return _regex.IsMatch(GetUnixPath(input));
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _originalPattern;
        }

        private static string EscapeAndReplace(string pattern)
        {
            var unix = GetUnixPath(pattern);

            if (unix.EndsWith("/"))
            {
                unix += "**";
            }

            pattern = Regex.Escape(unix)
                .Replace(@"/\*\*/", "(.*[/])")
                .Replace(@"\*\*/", "(.*)")
                .Replace(@"/\*\*", "(.*)")
                .Replace(@"\*", "([^/]*)")
                .Replace(@"\?", "(.)")
                .Replace(@"}", ")")
                .Replace(@"\{", "(")
                .Replace(@"0-999", @"([0-9]|[1-9][0-9]|[1-9][0-9][0-9])")
                .Replace(@"1-999", @"([1-9]|[1-9][0-9]|[1-9][0-9][0-9])")
                .Replace(@"0-", @"([0-9][0-9]*)")
                .Replace(@"1-", @"([1-9][0-9]*)")
                .Replace(@",", "|");

            return $"^{pattern}$";
        }

        private static string GetUnixPath(string txt)
        {
            return txt.Replace(@"\", "/").TrimStart('/');
        }
    }
}