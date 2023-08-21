namespace Aristocrat.Monaco.Asp.Client.Contracts
{
    /// <summary>
    ///     Class to hold protocol settings.
    /// </summary>
    public class ProtocolSettings
    {
        public static readonly string DacomProtocolVariation = "Dacom";
        public static readonly string Asp1000ProtocolVariation = "Asp1000";
        public static readonly string Asp2000ProtocolVariation = "Asp2000";

        /// <summary>
        ///     Protocol variation one of Dacom, Asp1000, Asp2000.
        /// </summary>
        public string ProtocolVariation { get; set; }

        /// <summary>
        ///     Protocol device definition file name/path.
        /// </summary>
        public string DeviceDefinitionFile { get; set; }
    }
}