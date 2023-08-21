namespace Aristocrat.G2S.Client
{
    /// <summary>
    ///     Defines a G2S Egm
    /// </summary>
    public interface IG2SEgm :
        IClientControl,
        IEgm
    {
        /// <summary>
        ///     Gets a value indicating whether this <see cref="IG2SEgm" /> is running.
        /// </summary>
        /// <value>
        ///     <c>true</c> if running; otherwise, <c>false</c>.
        /// </value>
        bool Running { get; }
    }
}