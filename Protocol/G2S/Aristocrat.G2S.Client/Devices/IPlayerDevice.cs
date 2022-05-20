namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with the player device
    /// </summary>
    public interface IPlayerDevice : IDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets a value indicating where the EGM meets the minimum display requirements
        /// </summary>
        bool DisplayPresent { get; }

        /// <summary>
        ///     Gets the message duration (in milliseconds)
        /// </summary>
        int MessageDuration { get; }

        /// <summary>
        ///     Gets a value indicating whether the EGM supports Meter Delta Logs
        /// </summary>
        bool MeterDeltaSupported { get; }

        /// <summary>
        ///     Gets a value indicating Indicates whether the EGM generates the playerSessionEndExt command at the end of player
        ///     sessions rather than playSessionEnd commands
        /// </summary>
        bool SendMeterDelta { get; }

        /// <summary>
        ///     Gets the id reader to use for player tracking
        /// </summary>
        int IdReader { get; }

        /// <summary>
        ///     Gets a value indicating whether idReader devices identified in the validationDeviceList command should be used to
        ///     initiate player sessions.
        /// </summary>
        bool UseMultipleIdDevices { get; }

        /// <summary>
        ///     Gets a value indicating whether idReader devices identified in the validationDeviceList command should be used to
        ///     initiate player sessions.
        /// </summary>
        IEnumerable<int> IdReaders { get; }

        /// <summary>
        ///     Gets the parameters of a host meter delta subscription.
        /// </summary>
        IEnumerable<meterDeltaHostSubscription> SubscribedMeters { get; }

        /// <summary>
        ///     Sets the meter subscription
        /// </summary>
        /// <param name="subscription">The subscription list</param>
        void SetMeterSubscription(IEnumerable<meterDeltaHostSubscription> subscription);

        /// <summary>
        ///     This method is used by an EGM to notify the host when a player session has started
        /// </summary>
        /// <param name="transactionId">Transaction identifier assigned by the EGM</param>
        /// <param name="idReaderType">The idReaderType as reported by the idReader class</param>
        /// <param name="idNumber">Identification number as reported by the idReader class</param>
        /// <param name="playerId">The host defined identifier for the idNumber/idReaderType pair as reported by the idReader class</param>
        /// <param name="startDateTime">Date and time that the session was started</param>
        /// <returns>A player session ack</returns>
        (bool timedOut, playerSessionStartAck response) StartSession(
            long transactionId,
            string idReaderType,
            string idNumber,
            string playerId,
            DateTime startDateTime);

        /// <summary>
        ///     This method is used by an EGM to notify the host when a player session has ended
        /// </summary>
        /// <param name="sessionEnd">The playerSessionEnd data</param>
        /// <returns>The transactionId of the completed session</returns>
        long? EndSession<T>(T sessionEnd) where T : c_baseCommand;
    }
}
