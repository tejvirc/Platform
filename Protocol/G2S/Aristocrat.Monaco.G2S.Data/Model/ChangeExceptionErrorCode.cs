namespace Aristocrat.Monaco.G2S.Data.Model
{
    /// <summary>
    ///     Enumerates available error codes for CommConfig and OptionConfig.
    /// </summary>
    public enum ChangeExceptionErrorCode
    {
        /// <summary>
        ///     Command successful
        /// </summary>
        Successful = 0,

        /// <summary>
        ///     Apply window expired
        /// </summary>
        Expired = 1,

        /// <summary>
        ///     Error applying changes.
        /// </summary>
        ErrorApplyingChanges = 2,

        /// <summary>
        ///     EGM must be disabled
        /// </summary>
        EgmMustBeDisabled = 3,

        /// <summary>
        ///     EGM must have zero credits
        /// </summary>
        EgmMustZeroCredits = 4,

        /// <summary>
        ///     EGM must be idle
        /// </summary>
        EgmMustBeIdle = 5,

        /// <summary>
        ///     Host authorization timeout
        /// </summary>
        Timeout = 6
    }
}