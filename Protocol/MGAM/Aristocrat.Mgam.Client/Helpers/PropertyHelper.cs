namespace Aristocrat.Mgam.Client.Helpers
{
    using System;

    /// <summary>
    ///     Properties helper functions and method extensions.
    /// </summary>
    public static class PropertyHelper
    {
        /// <summary>
        ///     Throws an exception that <paramref name="propertyName"/> not set.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <param name="validate"></param>
        public static void ThrowIfPropertyInvalid(this string propertyName, Func<bool> validate)
        {
            if (!validate())
            {
                throw new InvalidOperationException($"{propertyName} is not valid");
            }
        }
    }
}
