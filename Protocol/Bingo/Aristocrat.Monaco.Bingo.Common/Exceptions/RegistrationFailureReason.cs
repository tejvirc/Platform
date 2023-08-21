namespace Aristocrat.Monaco.Bingo.Common.Exceptions
{
    /// <summary>
    ///     Failure reasons for registration
    /// </summary>
    public enum RegistrationFailureReason
    {
        /// <summary>
        ///     Happens when the machine is rejected for registration
        /// </summary>
        Rejected = 0,

        /// <summary>
        ///     Happens when the token provided is invalid
        /// </summary>
        InvalidToken,

        /// <summary>
        ///     Happens when we receive no response when attempting to register
        /// </summary>
        NoResponse
    }
}