namespace Aristocrat.Monaco.G2S.Common
{
    using System;

    /// <summary>
    ///     Extensions for string
    /// </summary>
    public static class StringExtensions
    {
        private const string UriDelimiter1 = "//";
        private const string UriDelimiter2 = "@";

        /// <summary>
        ///     Returns the location URI without the user name and password.
        /// </summary>
        /// <param name="uri">Location URI string.</param>
        /// <returns>URI without user name and password.</returns>
        public static string LocationUri(this string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                var startIndex = uri.IndexOf(UriDelimiter1, StringComparison.InvariantCultureIgnoreCase);
                var endIndex = uri.IndexOf(UriDelimiter2, StringComparison.InvariantCultureIgnoreCase);

                if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
                {
                    return uri.Remove(startIndex + UriDelimiter1.Length, endIndex - startIndex - UriDelimiter2.Length);
                }
            }

            return uri;
        }
    }
}