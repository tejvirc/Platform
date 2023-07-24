namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     Event args for all reel events
    /// </summary>
    public class ReelSpinningEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReelSpinningEventArgs"/>
        /// </summary>
        /// <param name="reelId">The reel Id for this event</param>
        public ReelSpinningEventArgs(int reelId)
        {
            ReelId = reelId;
            Step = -1;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ReelSpinningEventArgs"/>
        /// </summary>
        /// <param name="reelId">The reel Id for this event</param>
        /// <param name="step">The step that the reel has stopped on if known, or -1 if unknown</param>
        public ReelSpinningEventArgs(int reelId, int step)
        {
            ReelId = reelId;
            Step = step;
        }

        /// <summary>
        ///     Gets the reel Id for the event
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the reel step of the reel if known or -1 if unknown
        /// </summary>
        public int Step { get; }
    }
}