namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    /// <summary>
    ///     TODO:set
    /// </summary>
    public interface IGpuDetailService
    {
        /// <summary>
        ///     Gets the active graphics card description
        /// </summary>
        string ActiveGpuName { get; }

        /// <summary>
        ///     TODO:set
        /// </summary>
        GpuInfo GraphicsCardInfo { get; }

        /// <summary>
        ///     TODO:set
        /// </summary>
        string GpuTemp { get; }

        /// <summary>
        /// TODO:set
        /// </summary>
        bool IsIGpuInUse { get; }
    }
}