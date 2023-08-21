// ReSharper disable once CheckNamespace
namespace Aristocrat.Monaco.Mgam.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using Gaming.Contracts;

    /// <summary>
    ///     Gaming method extensions.
    /// </summary>
    public static class GamingExtensions
    {
        /// <summary>
        ///     Converts amount from millicents to pennies.
        /// </summary>
        /// <param name="denom"></param>
        /// <returns>the amount in pennies.</returns>
        public static long ToPennies(this long denom)
        {
            return denom / 1000L;
        }

        /// <summary>
        ///     Gets a list of games configured for this VLT.
        /// </summary>
        /// <param name="provider"><see cref="IGameProvider"/>.</param>
        /// <returns>List of <see cref="IGameDetail"/>.</returns>
        public static IEnumerable<IGameDetail> GetConfiguredGames(this IGameProvider provider)
        {
            return provider.GetGames().Where(g => g.CentralAllowed && g.EgmEnabled);
        }
    }
}
