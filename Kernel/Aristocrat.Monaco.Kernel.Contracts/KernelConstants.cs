namespace Aristocrat.Monaco.Kernel.Contracts
{
    /// <summary>
    ///     Kernel Constants
    /// </summary>
    public static class KernelConstants
    {
        /// <summary>
        ///     Property key for the version string
        /// </summary>
        public const string SystemVersion = "System.Version";

        /// <summary>
        /// Path to the public key used to sign system components
        /// </summary>
        public const string SystemKey = "SystemKey";

        /// <summary>
        /// Path to the public key used to sign game components
        /// </summary>
        public const string GameKey = "GameKey";

        /// <summary>
        /// Path to the public key used to sign system components
        /// </summary>
        public const string SmartCardKey = "SmartCardKey";
    }
}
