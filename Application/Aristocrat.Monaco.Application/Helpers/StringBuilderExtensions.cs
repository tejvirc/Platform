namespace Aristocrat.Monaco.Application.Helpers
{
    using System.Text;

    /// <summary>
    ///     Extension methods for <see cref="StringBuilder"/>
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        ///     Appends the string to the end if it is not null or just whitespace.
        ///     It will also add a space in front of the newly added string
        ///     if there is already data in the string builder
        /// </summary>
        /// <param name="this">The <see cref="StringBuilder"/> to append the data to</param>
        /// <param name="value">The string to append</param>
        /// <returns>The string builder</returns>
        public static StringBuilder AppendWithSpace(this StringBuilder @this, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return @this;
            }

            return @this.Length > 0 ? @this.Append(" ").Append(value) : @this.Append(value);
        }
    }
}