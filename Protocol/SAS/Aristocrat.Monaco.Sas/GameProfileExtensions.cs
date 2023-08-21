namespace Aristocrat.Monaco.Sas
{
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Gaming.Contracts;

    /// <summary>
    ///     Extension methods for the IGameProfile interface
    /// </summary>
    public static class GameProfileExtensions
    {
        /// <summary>
        ///     Returns a list of codes from the list of denominations according to the c-4 table in sas documentation
        /// </summary>
        /// <param name="this">The Game Profile to use</param>
        /// <returns>The list of codes associated with each denomination.</returns>
        public static IReadOnlyCollection<byte> GetCodesFromDenominations(this IGameDetail @this) =>
            @this.ActiveDenominations.Distinct().Select(denomination => DenominationCodes.GetCodeForDenomination((int)denomination.MillicentsToCents())).ToList();
    }
}
