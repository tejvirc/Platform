namespace Aristocrat.Monaco.Bingo.Common.Exceptions
{
    public enum ConfigurationFailureReason
    {
        /// <summary>
        ///     Happens when the machine is rejected for configuration
        /// </summary>
        Rejected = 0,

        /// <summary>
        ///     Happens when the game configuration is invalid for the EGM
        /// </summary>
        InvalidGameConfiguration,

        /// <summary>
        ///     Happens when the stored configuration does not match and requires Persistence to be cleared
        /// </summary>
        ConfigurationMismatch,

        /// <summary>
        ///     Happens when we receive no response when attempting to register
        /// </summary>
        NoResponse
    }
}