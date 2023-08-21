namespace Aristocrat.Monaco.Gaming.Contracts.Session
{
    using Kernel;

    /// <summary>
    ///     Determines if id reader can start a session
    /// </summary>
    public interface IIdReaderValidator : IService
    {
        /// <summary>
        /// Returns true of a session can start
        /// </summary>
        /// <param name="idReaderId"></param>
        /// <returns></returns>
        bool CanAccept(int idReaderId);
    }
}
