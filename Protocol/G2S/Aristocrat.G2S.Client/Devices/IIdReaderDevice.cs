namespace Aristocrat.G2S.Client.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to interact with the idReader device
    /// </summary>
    public interface IIdReaderDevice : IDevice, IRestartStatus
    {
        /// <summary>
        ///     Gets the time-to-live value for requests originated by the device.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        ///     ID removal delay in milliseconds.
        /// </summary>
        int RemovalDelay { get; }

        /// <summary>
        ///     Message duration in milliseconds.
        /// </summary>
        int MsgDuration { get; }

        // ID Reader Messages Sub-Parameter Definitions

        /// <summary>
        ///     Message to display when no ID is present.
        /// </summary>
        string AttractMsg { get; }

        /// <summary>
        ///     Message to display while waiting for validation.
        /// </summary>
        string WaitMsg { get; }

        /// <summary>
        ///     Message to display while a valid ID is present.
        /// </summary>
        string ValidMsg { get; }

        /// <summary>
        ///     Message to display while an invalid ID is present.
        /// </summary>
        string InvalidMsg { get; }

        /// <summary>
        ///     Message to display while a lost ID is present.
        /// </summary>
        string LostMsg { get; }

        /// <summary>
        ///     Message to display if an ID cannot be validated.
        /// </summary>
        string OffLineMsg { get; }

        /// <summary>
        ///     Message to display if an ID is abandoned.
        /// </summary>
        string AbandonMsg { get; }

        /// <summary>
        ///     Indicates whether a host can disable player tracking
        ///     messages for a specific player.
        /// </summary>
        bool NoPlayerMessages { get; }

        /// <summary>
        ///     Request for an ID to be validated from host.
        /// </summary>
        /// <param name="idValidationCommand">The data to use in validation.</param>
        /// <param name="timeout">Timeout period for the request.</param>
        /// <param name="allowOffline">true if offline validation is supported</param>
        /// <param name="offlinePatterns">Pattern match when offline to determine ID type</param>
        /// <returns></returns>
        Task<setIdValidation> GetIdValidation(
            getIdValidation idValidationCommand,
            TimeSpan timeout,
            bool allowOffline,
            IReadOnlyCollection<(string type, string pattern)> offlinePatterns);

        /// <summary>
        ///     Stop all processing and generating of getIdValidation content.
        /// </summary>
        void CancelGetIdValidation();
    }
}
