namespace Aristocrat.Monaco.Mgam.Services.Meters
{
    /// <summary>
    ///     Define the <see cref="IMeterMonitor" /> interface.
    /// </summary>
    public interface IMeterMonitor
    {
        /// <summary>
        ///     Send all attribute messages for meters.
        /// </summary>
        void SendAllAttributes();
    }
}