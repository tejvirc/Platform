namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    /// <summary>
    ///     The Spin Data interface
    /// </summary>
    public interface ISpinData
    {
        /// <summary>
        ///     Gets the reel id for this data
        /// </summary>
        int ReelId { get; }

        /// <summary>
        ///     Gets the spin direction to use
        /// </summary>
        SpinDirection Direction { get; }

        /// <summary>
        ///     Gets the RPMs to spin the reels at or -1 if using the default on the controller
        /// </summary>
        int Rpm { get; }

        /// <summary>
        ///     Gets the step for the reel if using the controller timing or -1 if you are spinning until told to stop
        /// </summary>
        int Step { get; }
    }
}
