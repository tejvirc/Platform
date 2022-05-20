namespace Aristocrat.G2S.Client
{
    /// <summary>
    ///     Provides a mechanism to pass startup data to a device.
    /// </summary>
    public interface IStartupContext : ICommunicationContext
    {
        /// <summary>
        ///     Gets the host identifier associated with this context
        /// </summary>
        int HostId { get; }
    }
}