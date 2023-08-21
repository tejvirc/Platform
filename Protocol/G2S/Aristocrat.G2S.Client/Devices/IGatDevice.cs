namespace Aristocrat.G2S.Client.Devices
{
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with and control a Gat device.
    /// </summary>
    public interface IGatDevice : IDevice, ITransactionLogProvider
    {
        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     Gets device identifier of the idReader device associated with the local gat device.
        /// </summary>
        int IdReaderId { get; }

        /// <summary>
        ///     Gets the minimum number of components that the EGM MUST be able to queue for verification.
        /// </summary>
        int MinQueuedComps { get; }

        /// <summary>
        ///     Gets a value indicating whether commands related to special functions should be processed by the EGM for the
        ///     device.
        /// </summary>
        t_g2sBoolean SpecialFunctions { get; }

        /// <summary>
        ///     This command is used by the EGM to report the result of verifying a component.Although a
        ///     doVerification command can contain multiple components to verify, there is no requirement that the EGM
        ///     must wait until all the verifications are done before sending back a result. The EGM may send back the results
        ///     as they are completed, which is indicated by the verifyState attribute being updated to G2S_complete or
        ///     G2S_error.The EGM MUST continue to retry verificationResult command at the frequency set in the
        ///     timeToLive attribute of the gatProfile command until the results have been acknowledged by
        ///     verificationResultAck command or the verification request has been flushed from the gat class log.
        /// </summary>
        /// <param name="command">Package Status command</param>
        /// <returns>Verification result ack message from the host</returns>
        verificationResultAck SendVerificationResult(c_baseCommand command);
    }
}