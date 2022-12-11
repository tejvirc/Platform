namespace Aristocrat.Monaco.Gaming.Contracts
{
    using ProtoBuf;
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to get the game details
    /// </summary>
    [ProtoContract]
    [ProtoInclude(4, typeof(GameDenomChangedEvent))]
    public interface IGameDetail :
        IGameProfile,
        IGameAttributes
    {
        /// <summary>
        ///     Gets the value of each credit wagered as part of the game
        /// </summary>
        IEnumerable<long> ActiveDenominations { get; }

        /// <summary>
        ///     Gets the list of all supported denominations for each credit wagered as part of the game
        /// </summary>
        IEnumerable<long> SupportedDenominations { get; }

        /// <summary>
        ///     Gets the list of all supported denominations for each credit wagered as part of the game
        /// </summary>
        IEnumerable<IDenomination> Denominations { get; }
    }
}
