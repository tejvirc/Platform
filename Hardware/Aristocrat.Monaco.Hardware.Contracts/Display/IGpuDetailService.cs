namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    /// <summary>
    ///     Service to gather the GPU or iGPU details
    /// </summary>
    public interface IGpuDetailService
    {
        /// <summary>
        ///     Gets the active graphics card name.
        /// </summary>
        string ActiveGpuName { get; }

        /// <summary>
        ///     Returns the GPU details.
        /// </summary>
        GpuInfo GraphicsCardInfo { get; }

        /// <summary>
        ///     Returns the current GPU temperature if available
        /// </summary>
        string GpuTemp { get; }

        /// <summary>
        ///  returns whether only an iGPU is available.
        /// </summary>
        bool OnlyIGpuAvailable { get; }
    }
}