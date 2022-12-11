namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The AllowGameRoundChangedEvent is posted when the runtime updates the Allow Game Round Flag.
    /// </summary>
    [ProtoContract]
    public class AllowGameRoundChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AllowGameRoundChangedEvent" /> class.
        /// </summary>
        /// <param name="allowGameRound">Allow Game Round.</param>
        public AllowGameRoundChangedEvent(bool allowGameRound)
        {
            AllowGameRound = allowGameRound;
        }

        /// <summary>
        ///     Gets the AllowGameRound parameter
        /// </summary>
        [ProtoMember(1)]
        public bool AllowGameRound { get; }
    }
}