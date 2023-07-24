namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System;

    /// <summary>
    ///     The reel status received event arguments.
    /// </summary>
    public class ReelSpinningStatusEventArgs : EventArgs
    {
        /// <summary>Gets or sets the reel id for the status</summary>
        public int ReelId { get; set; }

        /// <summary> A reel has started slow spinning </summary>
        public bool SlowSpinning { get; set; }

        /// <summary> A reel has started spinning </summary>
        public bool Spinning { get; set; }

        /// <summary> A reel is accelerating </summary>
        public bool Accelerating { get; set; }

        /// <summary> A reel is slowing down </summary>
        public bool Decelerating { get; set; }

        /// <summary> A reel is idle at a known stop </summary>
        public bool IdleAtStep { get; set; }

        /// <summary> The step position of a reel if at a known stop </summary>
        public int Step { get; set; }

        /// <summary>
        ///     Creates an instance of <see cref="ReelSpinningStatusEventArgs"/>
        /// </summary>
        /// <param name="reelId">The reel Id for this event</param>
        public ReelSpinningStatusEventArgs(int reelId)
        {
            ReelId = reelId;
        }
    }
}
