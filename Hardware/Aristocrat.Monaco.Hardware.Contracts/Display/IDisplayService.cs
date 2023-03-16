namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    /// <summary>
    ///     Provides a mechanism to interact with the displays
    /// </summary>
    public interface IDisplayService
    {
        /// <summary>
        ///     Gets a value indicating whether or not the display service is in a faulted state
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        ///     Gets the number of connected displays
        /// </summary>
        int ConnectedCount { get; }

        /// <summary>
        ///     Gets the number of expected displays
        /// </summary>
        int ExpectedCount { get; }

        /// <summary>
        ///     Gets the active graphics card description
        /// </summary>
        string GraphicsCard { get; }

        /// <summary>
        /// TODO:set 
        /// </summary>
        public GpuInfo GraphicsCardInfo { get; }

        /// <summary>
        ///     Gets the maximum frame rate for the active graphics card.  If uncapped -1 will be returned.
        /// </summary>
        int MaximumFrameRate { get; }
    }
}