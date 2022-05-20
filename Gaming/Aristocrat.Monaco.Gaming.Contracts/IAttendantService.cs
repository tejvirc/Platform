namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     Manages the call attendant state and notifies observers when state changes
    /// </summary>
    public interface IAttendantService : IService
    {
        /// <summary>
        ///     Gets or sets a value indicating whether attendant service is requested
        /// </summary>
        bool IsServiceRequested { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Media Content is used
        /// </summary>
        bool IsMediaContentUsed { get; set; }

        /// <summary>
        ///     Handles Service Button press event
        /// </summary>
        void OnServiceButtonPressed();
    }
}
