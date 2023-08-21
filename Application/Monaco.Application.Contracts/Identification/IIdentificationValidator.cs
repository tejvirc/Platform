namespace Aristocrat.Monaco.Application.Contracts.Identification
{
    using System.Threading;
    using System.Threading.Tasks;
    using Hardware.Contracts.CardReader;

    /// <summary>
    ///     An interface that handles validating identification
    /// </summary>
    public interface IIdentificationValidator
    {
        /// <summary>
        ///     Initializes validation for the specified reader id.
        /// </summary>
        /// <param name="readerId">The id of the id reader.</param>
        void InitializeValidation(int readerId);

        /// <summary>
        ///     Clears validation for the specified reader id.
        /// </summary>
        /// <param name="readerId">The id of the id reader.</param>
        /// <param name="token">The cancellation token.</param>
        Task ClearValidation(int readerId, CancellationToken token);

        /// <summary>
        ///     Handles card read errors.
        /// </summary>
        /// <param name="readerId">The id of the id reader.</param>
        void HandleReadError(int readerId);

        /// <summary>
        ///     Validates the identification
        /// </summary>
        /// <param name="readerId">The id of the id reader.</param>
        /// <param name="trackData">The track data.</param>
        /// <param name="token">The cancellation token.</param>
        Task<bool> ValidateIdentification(int readerId, TrackData trackData, CancellationToken token);

        /// <summary>
        ///     Ends the player session and logs off the player
        /// </summary>
        Task LogoffPlayer();

        /// <summary>
        /// Ignore Key Switches property allows pages that test the keys to not affect operator menu access
        /// </summary>
        bool IgnoreKeySwitches { set; }
    }
}
