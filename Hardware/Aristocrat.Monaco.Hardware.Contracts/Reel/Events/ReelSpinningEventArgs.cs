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
            IsAccelerating = IsDecelerating = false;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ReelSpinningEventArgs"/>
        /// </summary>
        /// <param name="reelId">The reel Id for this event</param>
        /// <param name="accelerating"></param>
        /// <param name="decelerating"></param>
        public ReelSpinningEventArgs(int reelId, bool accelerating, bool decelerating)
        {
            ReelId = reelId;
            Step = -1;
            IsAccelerating = accelerating;
            IsDecelerating = decelerating;
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
            IsAccelerating = IsDecelerating = false;
        }

        /// <summary>
        ///     Gets the reel Id for the event
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the reel step of the reel if known or -1 if unknown
        /// </summary>
        public int Step { get; }

        /// <summary>
        ///     Is the reel accelerating?
        /// </summary>
        public bool IsAccelerating { get; }

        /// <summary>
        ///     Is the reel decelerating?
        /// </summary>
        public bool IsDecelerating { get; }
    }
}