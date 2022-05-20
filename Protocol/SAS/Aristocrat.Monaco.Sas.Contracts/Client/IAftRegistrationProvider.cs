namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     Definition of the IAftRegistrationProvider interface.
    /// </summary>
    public interface IAftRegistrationProvider
    {
        /// <summary>
        ///     Gets whether SAS AFT is registered
        /// </summary>
        bool IsAftRegistered { get; }

        /// <summary>
        ///     Gets whether SAS AFT is registered
        /// </summary>
        bool IsAftDebitTransferEnabled { get; }

        /// <summary>
        ///     Gets the AFT Registration Status
        /// </summary>
        /// <remarks>
        ///     Forces transition state order.  Sends SAS exceptions when necessary.
        /// </remarks>
        AftRegistrationStatus AftRegistrationStatus { get; }

        /// <summary>
        ///     Gets the AFT Registration Key
        /// </summary>
        byte[] AftRegistrationKey { get; }

        /// <summary>
        ///     Gets the AFT Registration Point Of Sale Identification
        /// </summary>
        uint PosId { get; }

        /// <summary>
        ///     Gets a registration key with all bytes set to 0.
        /// </summary>
        /// <remarks>When the registration key is all 0, AFT is not registered.</remarks>
        byte[] ZeroRegistrationKey { get; }

        /// <summary>
        ///     Process a registration code
        /// </summary>
        /// <param name="registrationCode">An AftRegistrationCode</param>
        /// <param name="assetNumber">An asset number</param>
        /// <param name="registrationKey">A registration key</param>
        /// <param name="posId">A point of sale identification</param>
        void ProcessAftRegistration(
            AftRegistrationCode registrationCode,
            uint assetNumber,
            byte[] registrationKey,
            uint posId);

        /// <summary>
        ///     Cancels the AFT registration cycle if it has not yet completed.
        /// </summary>
        void AftRegistrationCycleInterrupted();

        /// <summary>
        ///     EGM forces aft to unregister
        /// </summary>
        /// <remarks>
        ///     Examples from SAS 6.03:
        ///         MAY be done if: Memory error detected, or an operator changing the asset number or
        ///             AFT setup parameters or unregistering the gaming machine through a setup option
        ///             provided for that purpose.
        ///         MUST NOT be done for: Communication with the host has been lost, or a funds transfer
        ///             operation has failed.
        /// </remarks>
        void ForceAftNotRegistered();

        /// <summary>
        ///     Checks whether the input registration key matches the current registration key
        /// </summary>
        /// <param name="checkRegistrationKey">The input registration key to check</param>
        /// <returns>True if the input registration key matches the current registration key</returns>
        /// <remarks>This is a simple comparison.  There are no checks for AFT registration status in this method.</remarks>
        bool RegistrationKeyMatches(byte[] checkRegistrationKey);

        /// <summary>
        ///     Checks whether the input registration keys match
        /// </summary>
        /// <param name="registrationKeyLeft">The first registration key</param>
        /// <param name="registrationKeyRight">The second registration key</param>
        /// <returns>True if both registration keys match</returns>
        bool RegistrationKeyMatches(byte[] registrationKeyLeft, byte[] registrationKeyRight);
    }
}
