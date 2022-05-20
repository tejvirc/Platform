namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     The AllowGameRoundChangedEvent is posted when the runtime updates the Allow Game Round Flag.
    /// </summary>
    [Serializable]
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
        public bool AllowGameRound { get; }
    }
}