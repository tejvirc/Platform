namespace Aristocrat.Monaco.G2S.Services
{
    /// <summary>
    ///     Provides a mechanism to check the internal status of the client/EGM
    /// </summary>
    public interface ISelfTest
    {
        /// <summary>
        ///     Checks the status of each internal device
        /// </summary>
        void Execute();
    }
}