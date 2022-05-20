namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    /// <summary>
    ///     Data class to hold the response of a sas version and serial number query info
    /// </summary>
    public class LongPollSendSasVersionResponse : LongPollResponse
    {
        /// <summary>
        ///     Creates an instance of the LongPollSendSASVersionResponse class
        /// </summary>
        public LongPollSendSasVersionResponse(string sasVersion, string serialNumber)
        {
            SasVersion = sasVersion;
            SerialNumber = serialNumber;
        }

        public string SasVersion { get; }

        public string SerialNumber { get; }
    }
}
