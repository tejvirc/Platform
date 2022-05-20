namespace Aristocrat.Monaco.Hhr
{
    /// <summary>
    ///     Property keys names for storage/retrieval of properties for the HHR protocol.
    /// </summary>
    public static class HHRPropertyNames
    {
        /// <summary>The IP of the central server for requests.</summary>
        public const string ServerTcpIp = "HHR.ServerTcpIp";

        /// <summary>The port of the central server for requests.</summary>
        public const string ServerTcpPort = "HHR.ServerTcpPort";

        /// <summary>The port of the central server for broadcasts.</summary>
        public const string ServerUdpPort = "HHR.ServerUdpPort";

        /// <summary>The encryption key for encoding messages.</summary>
        public const string EncryptionKey = "HHR.EncryptionKey";

        /// <summary>Time interval in milliseconds between heartbeats.</summary>
        public const string HeartbeatInterval = "HHR.HeartbeatInterval";

        /// <summary>Last game played timestamp received from server.</summary>
        public const string LastGamePlayTime = "HHR.LastGamePlayTime";

        /// <summary>The sequence ID for messages being sent to the server.</summary>
        public const string SequenceId = "HHR.SequenceId";

        /// <summary>The timeout we allow before retrying sending a message.</summary>
        public const string FailedRequestRetryTimeout = "HHR.FailedRequestRetryTimeout";

        /// <summary>The amount of time the horses take to run the race on screen.</summary>
        public const string HorseResultsRunTime = "HHR.HorseResultsRunTime";

        /// <summary>Indicates whether we are doing Quick-Pick or Auto-Pick.</summary>
        public const string ManualHandicapMode = "HHR.ManualHandicapMode";
    }
}