namespace Aristocrat.Monaco.Sas.Aft
{
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Interface for top level classes that process Aft requests
    /// </summary>
    public interface IAftRequestProcessorTransferCode
    {
        /// <summary>
        ///     Process the request
        /// </summary>
        /// <param name="data">The data for the request</param>
        /// <returns>The response</returns>
        AftResponseData Process(AftResponseData data);
    }
}