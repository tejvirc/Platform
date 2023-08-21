namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using System.Linq;

    /// <summary>
    ///     A set of extension methods for the <see cref="IIdReaderProvider" />
    /// </summary>
    public static class ProviderExtensions
    {
        /// <summary>
        ///     Gets the current identity if present
        /// </summary>
        /// <param name="this">A <see cref="IIdReaderProvider" /> instance</param>
        /// <returns>The current <see cref="Identity" />, if present</returns>
        public static Identity GetCurrentIdentity(this IIdReaderProvider @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var idReader = @this.Adapters.FirstOrDefault(x => x.Identity != null);

            return idReader?.Identity;
        }
    }
}
