namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;
    using Central;

    /// <summary>
    ///     Defines a request for a game outcome (Central, Bingo, HHR, etc.)
    /// </summary>
    public interface IOutcomeRequest
    {
        /// <summary>
        ///     Gets the requested outcome count
        /// </summary>
        int Quantity { get; }

        /// <summary>
        ///     Additional game play requests
        /// </summary>
        IEnumerable<IAdditionalGamePlayInfo> AdditionalInfo { get; }
    }

    /// <summary>
    ///     Defines a request for a game outcome that is template based
    /// </summary>
    public interface ITemplateRequest
    {
        /// <summary>
        ///     Gets the wager category
        /// </summary>
        /// <remarks>
        ///     Within the context of a central determinant game, the finite deal (wagered amount, won amount, play count and
        ///     payback percentage) is assigned to a wager category within a paytable
        /// </remarks>
        int TemplateId { get; }
    }
}