namespace Aristocrat.Bingo.Client.CompositionRoot
{
    /// <summary>
    ///     Constants for controlling GRPC settings
    /// </summary>
    public static class GrpcConstants
    {
        /// <summary>
        ///     An environment variable for controlling the GRPC DNS resolver
        /// </summary>
        public const string GrpcDnsResolver = "GRPC_DNS_RESOLVER";

        /// <summary>
        ///     The default value to set for the DNS resolver
        /// </summary>
        public const string GrpcDefaultDnsResolver= "native";

        /// <summary>
        ///     An environment variable for controlling the GRPC logging verbosity
        /// </summary>
        public const string GrpcVerbosity = "GRPC_VERBOSITY";

        /// <summary>
        ///     The logging level to set for GRPC
        /// </summary>
        public const string GrpcLogLevel = "DEBUG";

        /// <summary>
        ///     An environment variable for controlling the GRPC trace logging
        /// </summary>
        public const string GrpcTrace = "GRPC_TRACE";

        /// <summary>
        ///     The trace levels to set for GRPC logging
        /// </summary>
        public const string GrpcTraceLevel = "api,call_error,client_channel_routing,cares_resolver,cares_address_sorting,tcp,health_check_client,http_keepalive,http,handshaker,op_failure,subchannel,subchannel_pool";
    }
}