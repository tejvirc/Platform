namespace Aristocrat.Monaco.G2S.Common.PackageManager
{
    using System.Collections.Generic;

    /// <summary>
    ///     Constants class
    /// </summary>
    public static class Constants
    {
        /// <summary>
        ///     Namespaces list
        /// </summary>
        public static readonly IDictionary<string, string> Namespaces = new Dictionary<string, string>
        {
            {
                "pkg",
                "http://www.gamingstandards.com/pkg/schemas/v1.0/"
            },
            {
                "ati",
                "http://www.aristocrat.com/pkg/schemas/v1.0"
            }
        };
    }
}