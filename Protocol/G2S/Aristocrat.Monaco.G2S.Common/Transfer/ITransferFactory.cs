namespace Aristocrat.Monaco.G2S.Common.Transfer
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITransferFactory
    {
        /// <summary>
        /// returns the transfer service based on the string input
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        IProtocolTransferService GetTransferService(string location);
    }
}
