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
        ///     Creates an instance of <see cref="ReelSpinningEventArgs"/>
        /// </summary>
        /// <param name="reelId">The reel Id for this event</param>
        /// <param name="spinVelocity"></param>
        public ReelSpinningEventArgs(int reelId, SpinVelocity spinVelocity)
        {
            ReelId = reelId;
            SpinVelocity = spinVelocity;
        }

        /// <summary>
        ///     Gets the reel Id for the event
        /// </summary>
        public int ReelId { get; }

        /// <summary>
        ///     Gets the reel step of the reel if known or -1 if unknown
        /// </summary>
        public int Step { get; }

        /// <summary> A reel is idle at a known stop </summary>
        public bool IdleAtStep { get; private set; }

        /// <summary> A reel has started slow spinning </summary>
        public bool SlowSpinning { get; private set; }

        /// <summary> The reel spin velocity of the reel </summary>
        public SpinVelocity SpinVelocity { get; private set; }

        /// <summary>
        ///     Create the event args for idle at step 
        /// </summary>
        /// <param name="reelId">reel index</param>
        /// <param name="step">the step</param>
        /// <returns>new instance of ReelSpinningEventArgs</returns>
        public static ReelSpinningEventArgs CreateForIdleAtStep(int reelId, int step)
        {
            var eventArgs = new ReelSpinningEventArgs(reelId, step) { IdleAtStep = true };
            return eventArgs;
        }

        /// <summary>
        ///     Create the event args for slow spinning
        /// </summary>
        /// <param name="reelId">reel index</param>
        /// <returns>new instance of ReelSpinningEventArgs</returns>
        public static ReelSpinningEventArgs CreateForSlowSpinning(int reelId)
        {
            var eventArgs = new ReelSpinningEventArgs(reelId) { SlowSpinning = true };
            return eventArgs;
        }
    }
}