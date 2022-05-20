namespace Aristocrat.Monaco.Hhr.Client.Data
{
    using Messages;

    /// <summary>
    ///     Client properties which are populated as part of some command requests.
    /// </summary>
    public class ClientProperties
    {
        /// <summary>
        ///     EGM device ID received as part of response of CMD_PARAMETER
        /// </summary>
        public static uint ParameterDeviceId = 0;

        /// <summary>
        ///     HHR protocol command versions. Currently this is always zero.
        /// </summary>
        public static uint CommandVersion = 0;

        /// <summary>
        ///     The multicast IP that we expect to use. This comes from response of CMD_PARAMETER.
        /// </summary>
        public static string ProgressiveUdpIp = "224.1.1.1";

        /// <summary>
        ///     TimeOut for ManualHandicap
        /// </summary>
        public static int ManualHandicapTimeOut = 0;

        /// <summary>
        ///     TimeOut for Race Stat Page
        /// </summary>
        public static int RaceStatTimeOut = 0;

        /// <summary>
        ///     Manual handicap mode setting
        /// </summary>
        public static string ManualHandicapMode = HhrConstants.DetectPickMode;
    }
}
