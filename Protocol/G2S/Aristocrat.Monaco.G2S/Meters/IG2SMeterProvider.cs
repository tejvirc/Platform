namespace Aristocrat.Monaco.G2S.Meters
{
    using Application.Contracts;

    /// <summary>
    ///     Definition of the IG2SMeterProvider interface.
    /// </summary>
    public interface IG2SMeterProvider : IMeterProvider
    {
        /// <summary>
        ///     Starts meter provider.
        /// </summary>
        void Start();
    }
}